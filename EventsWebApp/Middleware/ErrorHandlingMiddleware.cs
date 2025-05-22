using EventsWebApp.Services.Interfaces;
using EventsWebApp.ViewModels;
using System.Net;
using System.Text.Json;

namespace EventsWebApp.Middleware;
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, IErrorHandlingService errorHandlingService)
    {
        try
        {
            // Continue to next middleware in the pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the error with full details for developers
            await errorHandlingService.LogErrorAsync(
                "Unhandled exception in request pipeline", 
                ex, 
                new { 
                    RequestPath = context.Request.Path,
                    RequestMethod = context.Request.Method,
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    IPAddress = context.Connection.RemoteIpAddress?.ToString()
                });

            // Handle the error and return appropriate response to user
            await HandleExceptionAsync(context, ex, errorHandlingService);
        }
    }
    private static async Task HandleExceptionAsync(
        HttpContext context, 
        Exception exception, 
        IErrorHandlingService errorHandlingService)
    {
        // Determine HTTP status code and user message based on exception type
        var (statusCode, userMessage) = GetErrorResponse(exception, errorHandlingService);

        // Set response properties
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        // Check if this is an AJAX request or API call
        if (IsAjaxRequest(context) || IsApiRequest(context))
        {
            // Return JSON error response for AJAX/API requests
            await WriteJsonErrorResponse(context, statusCode, userMessage, exception);
        }
        else
        {
            // Redirect to error page for regular page requests
            await HandlePageErrorResponse(context, statusCode, userMessage, exception);
        }
    }

    private static (HttpStatusCode statusCode, string userMessage) GetErrorResponse(
        Exception exception, 
        IErrorHandlingService errorHandlingService)
    {
        return exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "Required information is missing."),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid information provided."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "You are not authorized to access this resource."),
            FileNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            InvalidOperationException when exception.Message.Contains("configuration") => 
                (HttpStatusCode.InternalServerError, "A configuration error has occurred. Please contact support."),
            HttpRequestException => (HttpStatusCode.BadGateway, "Unable to communicate with external services. Please try again later."),
            TimeoutException => (HttpStatusCode.RequestTimeout, "The request timed out. Please try again."),
            NotImplementedException => (HttpStatusCode.NotImplemented, "This feature is not yet available."),
            
            // Default case for unexpected exceptions
            _ => (HttpStatusCode.InternalServerError, errorHandlingService.HandleException(exception))
        };
    }
    private static async Task WriteJsonErrorResponse(
        HttpContext context, 
        HttpStatusCode statusCode, 
        string userMessage, 
        Exception exception)
    {
        var errorResponse = new
        {
            Error = true,
            Message = userMessage,
            StatusCode = (int)statusCode,
            Timestamp = DateTime.UtcNow,
            RequestId = context.TraceIdentifier
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        await context.Response.WriteAsync(jsonResponse);
    }
    private static async Task HandlePageErrorResponse(
        HttpContext context, 
        HttpStatusCode statusCode, 
        string userMessage, 
        Exception exception)
    {
        // Store error information in TempData for the error page
        if (context.Features.Get<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory>() != null)
        {
            try
            {
                var tempDataFactory = context.RequestServices
                    .GetRequiredService<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionaryFactory>();
                var tempData = tempDataFactory.GetTempData(context);
                
                tempData["ErrorMessage"] = userMessage;
                tempData["StatusCode"] = (int)statusCode;
                tempData["RequestId"] = context.TraceIdentifier;
            }
            catch
            {
                // If TempData setup fails, continue without it
            }
        }

        // Redirect to appropriate error page based on status code
        var errorPath = statusCode switch
        {
            HttpStatusCode.NotFound => "/Home/NotFound",
            HttpStatusCode.Unauthorized => "/Home/Unauthorized",
            HttpStatusCode.Forbidden => "/Home/Forbidden",
            _ => "/Home/Error"
        };

        // Perform redirect
        context.Response.Redirect(errorPath);
        await Task.CompletedTask;
    }

    private static bool IsAjaxRequest(HttpContext context)
    {
        return context.Request.Headers.XRequestedWith == "XMLHttpRequest" ||
               context.Request.Headers.Accept.ToString().Contains("application/json");
    }

    private static bool IsApiRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api") ||
               context.Request.Headers.Accept.ToString().Contains("application/json");
    }
}

public static class ErrorHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlingMiddleware>();
    }
}