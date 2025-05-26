namespace EventsWebApp.Services.Interfaces;

public interface IErrorHandlingService
{
    string HandleApiError(HttpResponseMessage response);
    string HandleException(Exception exception);
    Task LogErrorAsync(string message, Exception? exception = null, object? context = null);
}