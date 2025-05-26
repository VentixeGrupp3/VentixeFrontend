using EventsWebApp.Authentication;
using EventsWebApp.Extensions;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    builder.Services.AddAuthentication(ApiKeyAuthenticationSchemeOptions.DefaultScheme)
        .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationSchemeOptions.DefaultScheme, 
            options => { });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
    });

    builder.Services.AddControllersWithViews();
    builder.Services.AddApplicationServices(builder.Configuration);
    builder.Services.ValidateServiceRegistration();

    var app = builder.Build();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseApplicationMiddleware(app.Environment);

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.MapHealthChecks("/health");

    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("EventsWebApp started successfully at {Timestamp}", DateTime.UtcNow);

    app.Run();
}
catch (Exception ex)
{
    var logger = LoggerFactory.Create(builder => builder.AddConsole())
        .CreateLogger<Program>();
    
    logger.LogCritical(ex, "Application failed to start at {Timestamp}", DateTime.UtcNow);
    
    throw;
}

public partial class Program { }