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
}