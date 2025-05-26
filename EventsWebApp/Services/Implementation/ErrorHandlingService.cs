using EventsWebApp.Services.Interfaces;

namespace EventsWebApp.Services.Implementation;
//Error Handling compiled by AI
public class ErrorHandlingService(
    ILogger<ErrorHandlingService> logger,
    IConfigurationService configurationService) : IErrorHandlingService
{
private readonly ILogger<ErrorHandlingService> _logger = logger;
private readonly IConfigurationService _configurationService = configurationService;

    public string HandleApiError(HttpResponseMessage response)
    {
        if (response == null)
        {
            return "Unknown error occurred while communicating with the server.";
        }

        var userMessage = response.StatusCode switch
        {
            System.Net.HttpStatusCode.NotFound => "The requested information was not found.",
            System.Net.HttpStatusCode.Unauthorized => "You are not authorized to access this resource.",
            System.Net.HttpStatusCode.Forbidden => "Access to this resource is forbidden.",
            System.Net.HttpStatusCode.BadRequest => "The request was invalid. Please check your input and try again.",
            System.Net.HttpStatusCode.InternalServerError => "A server error occurred. Please try again later.",
            System.Net.HttpStatusCode.ServiceUnavailable => "The service is temporarily unavailable. Please try again later.",
            System.Net.HttpStatusCode.RequestTimeout => "The request timed out. Please try again.",
            _ => "An error occurred while communicating with the server."
        };

        _logger.LogWarning("API error: {StatusCode} - {ReasonPhrase}", 
                          response.StatusCode, response.ReasonPhrase);

        return userMessage;
    }

    public string HandleException(Exception exception)
    {
        if (exception == null)
        {
            return "An unknown error occurred.";
        }

        if (_configurationService.IsDevelopment())
        {
            return $"Error: {exception.Message}";
        }

        var userMessage = exception switch
        {
            ArgumentException => "Invalid input provided.",
            InvalidOperationException => "The requested operation could not be completed.",
            HttpRequestException => "A network error occurred. Please check your connection and try again.",
            TimeoutException => "The operation timed out. Please try again.",
            UnauthorizedAccessException => "You are not authorized to perform this action.",
            _ => "An unexpected error occurred. Please try again later."
        };

        return userMessage;
    }

    public async Task LogErrorAsync(string message, Exception? exception = null, object? context = null)
    {
        try
        {
            var logData = new
            {
                Message = message,
                Exception = exception?.ToString(),
                Context = context,
                Timestamp = DateTime.UtcNow,
                Environment = _configurationService.GetEnvironmentName()
            };

            if (exception != null)
            {
                _logger.LogError(exception, "Error occurred: {Message}. Context: {@Context}", message, logData);
            }
            else
            {
                _logger.LogWarning("Warning: {Message}. Context: {@Context}", message, logData);
            }

            if (!_configurationService.IsDevelopment() && IsCriticalError(exception))
            {
                await SendToCriticalErrorMonitoring(logData);
            }
        }
        catch (Exception loggingException)
        {
            Console.WriteLine($"Logging failed: {loggingException.Message}");
            Console.WriteLine($"Original error: {message}");
        }
    }

    private static bool IsCriticalError(Exception? exception)
    {
        return exception switch
        {
            OutOfMemoryException => true,
            StackOverflowException => true,
            InvalidOperationException when exception.Message.Contains("configuration") => true,
            _ => false
        };
    }

    private async Task SendToCriticalErrorMonitoring(object logData)
    {
        await Task.CompletedTask;
    }
}
