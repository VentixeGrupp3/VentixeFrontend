using EventsWebApp.Services.Interfaces;
using System.Net;
using System.Text.Json;

namespace EventsWebApp.Middleware;
public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context, IErrorHandlingService errorHandlingService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await errorHandlingService.LogErrorAsync(
                "Unhandled exception in request pipeline", 
                ex, 
                new { 
                    RequestPath = context.Request.Path,
                    RequestMethod = context.Request.Method,
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    IPAddress = context.Connection.RemoteIpAddress?.ToString()
                });

            await HandleExceptionAsync(context, ex, errorHandlingService);
        }
    }
    private static async Task HandleExceptionAsync(
        HttpContext context, 
        Exception exception, 
        IErrorHandlingService errorHandlingService)
    {
        var (statusCode, userMessage) = GetErrorResponse(exception, errorHandlingService);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        if (IsAjaxRequest(context) || IsApiRequest(context))
        {
            await WriteJsonErrorResponse(context, statusCode, userMessage, exception);
        }
        else
        {
            await HandlePageErrorResponse(context, statusCode, userMessage, exception);
        }
    }

    //AI generated method to handle error responses
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

            var errorPath = $"/Home/HandleError/{(int)statusCode}";

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