using Microsoft.AspNetCore.Components.Forms;

namespace Watasu.Interfaces;

public interface IValidationService
{
    bool ValidatePasscode(string passcode);
    bool ValidateDebugKey(string passcode);
    bool ValidateFileType(IBrowserFile browserFile);
    bool ValidateFilesType(IList<IBrowserFile> files);
}