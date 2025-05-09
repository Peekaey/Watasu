using Microsoft.AspNetCore.Components.Forms;
using Watasu.Interfaces;
using Watasu.Models;

namespace Watasu.Service;

public class WebService : IWebService
{
    private readonly IValidationService _validationService;
    private readonly IBackgroundWindowService _backgroundWindowService;
    
    public WebService(IValidationService validationService, IBackgroundWindowService backgroundWindowService)
    {
        _validationService = validationService;
        _backgroundWindowService = backgroundWindowService;
    }
    
    public async Task<ServiceResult> TransferFile(IList<IBrowserFile> files, string passcode, SendLocationEnum sendLocationEnum)
    {
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
                return ServiceResult.AsFailure("Invalid passcode.", null);
            }
            
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
                return ServiceResult.AsFailure("Invalid debug key.", null);
            }
            
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
            return ServiceResult.AsFailure("Invalid passcode.", null);
        }
        
        var saveResult = await _backgroundWindowService.SaveFilesToFileSystem(files, saveLocation);
        
        if (!saveResult.Success)
        {
            return ServiceResult.AsFailure($"{saveResult.Error}", saveResult.StackTrace);
        }
        
        return ServiceResult.AsSuccess();
    }
}