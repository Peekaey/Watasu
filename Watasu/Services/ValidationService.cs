using Microsoft.AspNetCore.Components.Forms;
using Watasu.Interfaces;
using Watasu.Models;

namespace Watasu.Service;

public class ValidationService : IValidationService
{
    private readonly ApplicationConfigurationSettings _applicationConfigurationService;
    
    public ValidationService(ApplicationConfigurationSettings applicationConfigurationService)
    {
        _applicationConfigurationService = applicationConfigurationService;
    }
    public bool ValidatePasscode(string passcode)
    {
        if (string.IsNullOrEmpty(passcode))
        {
            return false;
        }
        
        if (passcode != _applicationConfigurationService.UploadPassword)
        {
            return false;
        }
        
        return true;
    }

    public bool ValidateDebugKey(string passcode)
    {
        if (string.IsNullOrEmpty(passcode))
        {
            return false;
        }

        if (passcode != _applicationConfigurationService.DebugKey)
        {
            return false;
        }

        return true;
    }

    public bool ValidateFileType(IBrowserFile browserFile)
    {
        var isValidFileType = _applicationConfigurationService.IsValidFileType(browserFile.ContentType);
        
        if (!isValidFileType)
        {
            return false;
        }
        
        return true;
    }

    public bool ValidateFilesType(IList<IBrowserFile> files)
    {
        return files.Select(file => _applicationConfigurationService.IsValidFileType(file.ContentType)).All(isValidFileType => isValidFileType);
    }
}