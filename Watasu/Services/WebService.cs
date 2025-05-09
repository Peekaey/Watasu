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
    
    public async Task<ServiceResult> TransferFile(IList<IBrowserFile> files, string passcode)
    {
        if (files.Count == 0)
        {
            return ServiceResult.AsFailure("No files selected.");
        }

        if (string.IsNullOrEmpty(passcode))
        {
            return ServiceResult.AsFailure("Passcode is required.");
        }
        
        var isPasscodeValid = _validationService.ValidatePasscode(passcode);
        
        if (!isPasscodeValid)
        {
            return ServiceResult.AsFailure("Invalid passcode.");
        }
        
        var copyClipboardResult = await _backgroundWindowService.ConvertBrowserFileToClipboard(files[0]);
        
        if (!copyClipboardResult.Success)
        {
            return ServiceResult.AsFailure($"Error copying file to clipboard: {copyClipboardResult.Error}");
        }
        
        var pasteToPaintResult = _backgroundWindowService.PasteToPaint();
        
        if (!pasteToPaintResult.Success)
        {
            return ServiceResult.AsFailure($"Error pasting to Paint: {pasteToPaintResult.Error}");
        }
        
        return ServiceResult.AsSuccess();
        
    }
}