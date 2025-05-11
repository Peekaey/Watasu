namespace Watasu.Models;

public class ApplicationConfigurationSettings
{
    public required string UploadPassword { get; set; }
    public required string DebugKey { get; set; }
    private readonly Dictionary<string, string> _validFileTypes = new()
    {
        { "image/jpg", ".jpg" },
        { "image/jpeg", ".jpeg" },
        { "image/png", ".png" },
        { "image/gif", ".gif" },
    };

    public bool IsValidFileType(string contentType)
    {
        return _validFileTypes.ContainsKey(contentType);   
    }
    
    public string? GetFileExtension(string contentType)
    {
        return _validFileTypes[contentType];
    }
    
    
}