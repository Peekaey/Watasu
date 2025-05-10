using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Components.Forms;
using Watasu.Interfaces;
using Watasu.Models;

namespace Watasu.Service;

public class BackgroundWindowService : IBackgroundWindowService
{
    [DllImport("user32.dll")]
    static extern bool SetForegroundWindow(IntPtr hWnd);
    
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    
    [DllImport("ole32.dll")]
    private static extern int OleFlushClipboard();
    
    public async Task<ServiceResult> ConvertBrowserFileToClipboard(IBrowserFile browserFile)
    {
        try 
        {
            await using (var stream = browserFile.OpenReadStream(maxAllowedSize: 20_000_000))
            {
                using (var ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms);
                    var imageBytes = ms.ToArray();
                    
                    // Run clipboard copy operation on a dedicated STA thread as Clipboard.SetImage requires STA 
                    // (Single Thread Apartment) mode & Blazor runs on a background thread by default
                    var thread = new Thread(() =>
                    {
                        using (var imageStream = new MemoryStream(imageBytes))
                        {
                            using (var image = Image.FromStream(imageStream))
                            {
                                var bitmap = new Bitmap(image);

                                using (var bmpStream = new MemoryStream())
                                {
                                    // W10/W11 Support
                                    bitmap.Save(bmpStream, ImageFormat.Bmp);
                                    var bmpBytes = bmpStream.ToArray();
                                    
                                    var dibBytes = bmpBytes.Skip(14).ToArray();
                                    var data = new DataObject();
                                    data.SetData(DataFormats.Dib, new MemoryStream(dibBytes));
                                    Clipboard.SetDataObject(data, true);
                                    OleFlushClipboard();
                                }
                                
                                // Wiin 11 Only
                                // Clipboard.SetImage(bitmap);
                                // OleFlushClipboard();
                                
                            }
                        }
                    });
                    
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();

                    return ServiceResult.AsSuccess();

                }
            }
        } 
        catch (Exception ex)
        {
            return ServiceResult.AsFailure($"Error converting file to clipboard: {ex.Message}", ex.StackTrace);
        }
    }

    public ServiceResult PasteToPaint()
    {
        try
        {
            var allProcesses = Process.GetProcesses();
            // Check if Microsoft Paint Process is running
            var msPaintProcess = allProcesses.FirstOrDefault(process => process.ProcessName.Contains("Paint"));

            // Start the process if not found
            if (msPaintProcess == null)
            {
                msPaintProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "mspaint.exe",
                        UseShellExecute = true
                    }
                };
                msPaintProcess.Start();
            }
            
            // Wait for process to be ready && give extra time for application to load
            msPaintProcess.WaitForInputIdle();
            
            for (var i = 0; i < 10; i++)
            {
                msPaintProcess.Refresh();
                if (msPaintProcess.MainWindowHandle != IntPtr.Zero)
                {
                    break;
                }

                Thread.Sleep(200);
            }
            
            // Switch to the application 
            if (msPaintProcess.MainWindowHandle != IntPtr.Zero)
            {
                ShowWindow(msPaintProcess.MainWindowHandle, 9); // Restore if minimized
                SetForegroundWindow(msPaintProcess.MainWindowHandle);
            }
            
            // Ctrl + V paste
            SendKeys.SendWait("^v"); 
            return ServiceResult.AsSuccess();
            
        } 
        catch (Exception ex)
        {
            return ServiceResult.AsFailure($"Error pasting to Paint: {ex.Message}", ex.StackTrace);
        }
    }
    
    public ServiceResult PasteToCorelDraw()
    {
        try
        {
            var allProcesses = Process.GetProcesses();
            // Check if CorelDraw Essentials is running
            var corelDrawProcess = allProcesses.FirstOrDefault(process => process.ProcessName.Contains("DrawEssentials"));

            // Start the process if not found
            if (corelDrawProcess == null)
            {
                corelDrawProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "DrawEssentials.exe",
                        UseShellExecute = true
                    }
                };
                corelDrawProcess.Start();
            }
            
            // Wait for process to be ready && give extra time for application to load
            corelDrawProcess.WaitForInputIdle();
            
            for (var i = 0; i < 10; i++)
            {
                corelDrawProcess.Refresh();
                if (corelDrawProcess.MainWindowHandle != IntPtr.Zero)
                {
                    break;
                }

                Thread.Sleep(200);
            }
            
            // Switch to the application 
            if (corelDrawProcess.MainWindowHandle != IntPtr.Zero)
            {
                ShowWindow(corelDrawProcess.MainWindowHandle, 9); // Restore if minimized
                SetForegroundWindow(corelDrawProcess.MainWindowHandle);
            }
            
            // Ctrl + V paste
            SendKeys.SendWait("%esb{ENTER}");
            return ServiceResult.AsSuccess();
            
        } 
        catch (Exception ex)
        {
            return ServiceResult.AsFailure($"Error pasting to CorelDraw: {ex.Message}", ex.StackTrace);
        }
    }

    public async Task<ServiceResult> SaveFilesToFileSystem(IList<IBrowserFile> files, FileSystemSaveLocationEnum saveLocation)
    {

        var saveLocationFolder = string.Empty;
        var errorMessage = new List<string>();
        switch (saveLocation)
        {
            case FileSystemSaveLocationEnum.MyPictures:
                saveLocationFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                break;
            case FileSystemSaveLocationEnum.MyDocuments:
                saveLocationFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(saveLocation), saveLocation, null);
        }
            
        if (!Directory.Exists(saveLocationFolder))
        {
            throw new DirectoryNotFoundException($"Directory {saveLocationFolder} does not exist");
        }
            
        foreach (var file in files)
        {
            try
            {
                var filePath = Path.Combine(saveLocationFolder, file.Name);

                using (var stream = file.OpenReadStream(maxAllowedSize: 20_000_000))
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    { 
                        await stream.CopyToAsync(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage.Add("Unable to save file: " + file.Name + "Error: " + ex.Message);
            }
        }
        if (errorMessage.Count > 0)
        {
            return ServiceResult.AsFailure(string.Join(Environment.NewLine, errorMessage), null);
        }
        return ServiceResult.AsSuccess();
    }
}