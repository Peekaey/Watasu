using System.Diagnostics;
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
                                Clipboard.SetImage(image);
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
            return ServiceResult.AsFailure($"Error converting file to clipboard: {ex.Message}");
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
            return ServiceResult.AsFailure($"Error pasting to Paint: {ex.Message}");
        }
    }
    
}