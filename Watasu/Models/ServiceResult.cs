namespace Watasu.Models;

public class ServiceResult
{
    public bool Success { get; set; }
    public string Error { get; set; }
    public string StackTrace { get; set; }
    public ServiceResult(bool isSuccess, string? errorMessage = null, string? stackTrace = null)
    {
        Success = isSuccess;
        Error = errorMessage ?? string.Empty;
        StackTrace = stackTrace ?? string.Empty;
    }

    public static ServiceResult AsSuccess()
    {
        return new ServiceResult(true);
    }
    
    public static ServiceResult AsFailure(string errorMessage, string? stackTrace)
    {
        return new ServiceResult(false, errorMessage, stackTrace);
    }
}