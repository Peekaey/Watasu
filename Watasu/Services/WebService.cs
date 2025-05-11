using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Caching.Memory;
using Watasu.Interfaces;
using Watasu.Models;

namespace Watasu.Service;

public class WebService : IWebService
{
    private readonly IValidationService _validationService;
    private readonly IBackgroundWindowService _backgroundWindowService;
    private readonly IMemoryCache _memoryCache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    private const int MaxAttemptsInPeriod = 5;
    private readonly TimeSpan _timeoutPeriod = TimeSpan.FromMinutes(5);
    
    private class FailureEntry
    {
        public int               Count;
        public DateTimeOffset    FirstFailureUtc;
    }

    public WebService(IValidationService validationService, IBackgroundWindowService backgroundWindowService,
        IMemoryCache memoryCache, IHttpContextAccessor httpContextAccessor)
    {
        _validationService = validationService;
        _backgroundWindowService = backgroundWindowService;
        _memoryCache = memoryCache;
        _httpContextAccessor = httpContextAccessor;
    }
    
    
    public async Task<ServiceResult> TransferFile(IList<IBrowserFile> files, string passcode, SendLocationEnum sendLocationEnum)
    {
        string clientIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        string cacheKey = $"upload_failures_{clientIp}";
        
        // Create initial failure key
        var failures = _memoryCache.GetOrCreate(cacheKey, entry =>
        {
            entry.SetAbsoluteExpiration(_timeoutPeriod);
            return new FailureEntry
            {
                Count           = 0,
                FirstFailureUtc = DateTimeOffset.UtcNow
            };
        });

        if (failures.Count >= MaxAttemptsInPeriod)
        {
            var retryAfter = failures.FirstFailureUtc
                .Add(_timeoutPeriod)
                .Subtract(DateTimeOffset.UtcNow);
            var minutes = Math.Ceiling(Math.Max(0, retryAfter.TotalMinutes));
            return ServiceResult.AsFailure($"Too many failed attempts. Try again in {minutes} minute(s).", null);
        }
        
        if (files.Count == 0)
        {
            return ServiceResult.AsFailure("No files selected.", null);
        }
        
        if (string.IsNullOrEmpty(passcode))
        {
            return ServiceResult.AsFailure("Passcode is required.", null);
        }
        
        if (!_validationService.ValidateFileType(files[0]))
        {
            return ServiceResult.AsFailure("Invalid file type. Only JPEG, JPG, PNG, and GIF are allowed", null);
        }

        if (sendLocationEnum == SendLocationEnum.CorelDraw)
        {
            if (!_validationService.ValidatePasscode(passcode))
            {
                failures.Count++;
                return ServiceResult.AsFailure("Invalid passcode.", null);
            }
            _memoryCache.Remove(cacheKey);

            
            var copyClipboardResult = await _backgroundWindowService.ConvertBrowserFileToClipboard(files[0]);
            
            if (!copyClipboardResult.Success)
            {
                return ServiceResult.AsFailure($"{copyClipboardResult.Error}", copyClipboardResult.StackTrace);
            }
            
            var pasteToCorelDrawResult = _backgroundWindowService.PasteToCorelDraw();
            
            if (!pasteToCorelDrawResult.Success)
            {
                return ServiceResult.AsFailure($"{pasteToCorelDrawResult.Error}", pasteToCorelDrawResult.StackTrace);
            }
        }

        if (sendLocationEnum == SendLocationEnum.MsPaint)
        {
            if (!_validationService.ValidateDebugKey(passcode))
            {
                failures.Count++;
                return ServiceResult.AsFailure("Invalid debug key.", null);
            }
            _memoryCache.Remove(cacheKey);

            
            var copyClipboardResult = await _backgroundWindowService.ConvertBrowserFileToClipboard(files[0]);
        
            if (!copyClipboardResult.Success)
            {
                return ServiceResult.AsFailure($"{copyClipboardResult.Error}", copyClipboardResult.StackTrace);
            }
            
            var pasteToPaintResult = _backgroundWindowService.PasteToPaint();
        
            if (!pasteToPaintResult.Success)
            {
                return ServiceResult.AsFailure($"{pasteToPaintResult.Error}",pasteToPaintResult.StackTrace);
            }
        }
        
        return ServiceResult.AsSuccess();
        
    }

    public async Task<ServiceResult> CopyFilesToFileSystem(IList<IBrowserFile> files, string passcode,
        FileSystemSaveLocationEnum saveLocation)
    {
        
        string clientIp = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        string cacheKey = $"upload_failures_{clientIp}";
        
        // Create initial failure key
        var failures = _memoryCache.GetOrCreate(cacheKey, entry =>
        {
            entry.SetAbsoluteExpiration(_timeoutPeriod);
            return new FailureEntry
            {
                Count           = 0,
                FirstFailureUtc = DateTimeOffset.UtcNow
            };
        });

        if (failures.Count >= MaxAttemptsInPeriod)
        {
            var retryAfter = failures.FirstFailureUtc
                .Add(_timeoutPeriod)
                .Subtract(DateTimeOffset.UtcNow);
            var minutes = Math.Ceiling(Math.Max(0, retryAfter.TotalMinutes));
            return ServiceResult.AsFailure($"Too many failed attempts. Try again in {minutes} minute(s).", null);
        }
        
        if (files.Count == 0)
        {
            return ServiceResult.AsFailure("No files selected.", null);
        }
        
        if (string.IsNullOrEmpty(passcode))
        {
            return ServiceResult.AsFailure("Passcode is required.", null);
        }
        
        if (!_validationService.ValidateFilesType(files))
        {
            return ServiceResult.AsFailure("One or more files has an invalid file type. Only JPEG, JPG, PNG, and GIF are allowed", null);
        }
        
        if (!_validationService.ValidatePasscode(passcode))
        {
            failures.Count++;
            return ServiceResult.AsFailure("Invalid passcode.", null);
        }
        _memoryCache.Remove(cacheKey);

        
        var saveResult = await _backgroundWindowService.SaveFilesToFileSystem(files, saveLocation);
        
        if (!saveResult.Success)
        {
            return ServiceResult.AsFailure($"{saveResult.Error}", saveResult.StackTrace);
        }
        
        return ServiceResult.AsSuccess();
    }
}