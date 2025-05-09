using Microsoft.AspNetCore.Components.Forms;
using Watasu.Models;

namespace Watasu.Interfaces;

public interface IWebService
{
    Task<ServiceResult> TransferFile(IList<IBrowserFile> files, string passcode, SendLocationEnum sendLocationEnum);

    Task<ServiceResult> CopyFilesToFileSystem(IList<IBrowserFile> files, string passcode,
        FileSystemSaveLocationEnum saveLocation);
}