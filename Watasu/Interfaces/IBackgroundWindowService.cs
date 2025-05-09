using Microsoft.AspNetCore.Components.Forms;
using Watasu.Models;

namespace Watasu.Interfaces;

public interface IBackgroundWindowService
{
    Task<ServiceResult> ConvertBrowserFileToClipboard(IBrowserFile browserFile);
    ServiceResult PasteToPaint();
    ServiceResult PasteToCorelDraw();
    Task<ServiceResult> SaveFilesToFileSystem(IList<IBrowserFile> files, FileSystemSaveLocationEnum saveLocation);
}