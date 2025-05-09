namespace Watasu.Models;

public class ApplicationConfigurationSettings
{
    public string UploadPassword { get; set; }
    public string DebugKey { get; set; }
    private readonly Dictionary<string, string> ValidFileTypes = new()
    {
        { "image/jpg", ".jpg" },
        { "image/jpeg", ".jpeg" },
        { "image/png", ".png" },
        { "image/gif", ".gif" },
    };

    public bool IsValidFileType(string contentType)
    {
        return ValidFileTypes.ContainsKey(contentType);   
    }
    
    public string? GetFileExtension(string contentType)
    {
        return ValidFileTypes[contentType];
    }
    
    
}