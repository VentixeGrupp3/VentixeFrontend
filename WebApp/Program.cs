using WebApp.Authentication;
using WebApp.Extensions;
using WebApp.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Identity;
using WebApp.Protos;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    builder.Services.AddControllersWithViews();

    // Database
    builder.Services.AddDbContext<ApplicationDbContext>(x => 
        x.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));

    // Identity Authentication
    builder.Services.AddIdentity<AppUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    // API Key Authentication (for Events API)
    builder.Services.AddAuthentication(ApiKeyAuthenticationSchemeOptions.DefaultScheme)
        .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            ApiKeyAuthenticationSchemeOptions.DefaultScheme, 
            options => { });

    // Authorization policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
    });

    // Application services
    builder.Services.AddScoped<IAccountService, AccountService>();
    builder.Services.AddScoped<VerificationService>();
    builder.Services.AddApplicationServices(builder.Configuration);

    // Identity cookie configuration
    builder.Services.ConfigureApplicationCookie(x =>
    {
        x.LoginPath = "/login";
        x.AccessDeniedPath = "/denied";
        x.Cookie.HttpOnly = true;
        x.Cookie.IsEssential = true;
        x.Cookie.SameSite = SameSiteMode.Lax;
        x.ExpireTimeSpan = TimeSpan.FromDays(30);
        x.SlidingExpiration = true;
    });

    // GRPC clients
    builder.Services.AddGrpcClient<EmailConfirmation.EmailConfirmationClient>(options =>
    {
        options.Address = new Uri(builder.Configuration.GetConnectionString("EmailConfirmationGrpcConnectionString"));
    }).ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler();
        return handler;
    });

    builder.Services.AddGrpcClient<UserProfileProtoService.UserProfileProtoServiceClient>(options =>
    {
        options.Address = new Uri(builder.Configuration.GetConnectionString("UserProfileGrpcConnectionString"));
    }).ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler();
        return handler;
    });

    // Invoice API clients
    builder.Services.AddHttpClient("UserClient", client =>
    {
        client.BaseAddress = new Uri("https://ventixe-invoice-microservice-group3.azurewebsites.net/api/");
        client.DefaultRequestHeaders.Add("x-api-key", "1a76c263-4d83-4c98-b913-9029f9dfad7d");
    });

    builder.Services.AddHttpClient("AdminClient", client =>
    {
        client.BaseAddress = new Uri("https://ventixe-invoice-microservice-group3.azurewebsites.net/api/");
        client.DefaultRequestHeaders.Add("x-api-key", "fba16aa0-4bb4-4bb7-9201-d81937292329");
    });

    builder.Services.AddScoped<InvoiceApiService.IInvoiceApiClient>(sp =>
    {
        var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext!;
        var clientName = httpContext.User.IsInRole("Admin") ? "AdminClient" : "UserClient";
        var factory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = factory.CreateClient(clientName);
        return new InvoiceApiService.InvoiceApiClient(httpClient);
    });

    // Validate service registration
    builder.Services.ValidateServiceRegistration();

    var app = builder.Build();

    // Service resolution validation
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var eventsService = scope.ServiceProvider.GetRequiredService<IEventsApiService>();
            Console.WriteLine("IEventsApiService resolved successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to resolve IEventsApiService: {ex.Message}");
            throw;
        }
    }

    // Middleware pipeline
    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseApplicationMiddleware(app.Environment);

    // Static assets and routing
    app.MapStaticAssets();
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    // Health checks
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