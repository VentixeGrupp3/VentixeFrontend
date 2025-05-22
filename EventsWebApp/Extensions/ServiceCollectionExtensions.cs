using EventsWebApp.Configuration;
using EventsWebApp.Middleware;
using EventsWebApp.Services.Implementation;
using EventsWebApp.Services.Interfaces;
using System.Net.Http.Headers;

namespace EventsWebApp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
       
        services.AddConfiguration(configuration);
        
        
        services.AddHttpClients(configuration);
        
        
        services.AddBusinessServices();
        
        
        services.AddUtilityServices();

        return services;
    }
    private static IServiceCollection AddConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Configure strongly-typed configuration objects
        services.Configure<ApiConfiguration>(
            configuration.GetSection(ApiConfiguration.SectionName));
        
        services.Configure<ApplicationConfiguration>(
            configuration.GetSection(ApplicationConfiguration.SectionName));

        // Register configuration service
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        // Validate configuration at startup
        var serviceProvider = services.BuildServiceProvider();
        var configService = serviceProvider.GetRequiredService<IConfigurationService>();
        
        if (!configService.ValidateConfiguration())
        {
            throw new InvalidOperationException(
                "Application configuration validation failed. Check logs for details.");
        }

        return services;
    }
    private static IServiceCollection AddHttpClients(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddHttpClient("EventsApi", (serviceProvider, client) =>
        {
            var configService = serviceProvider.GetRequiredService<IConfigurationService>();
            
            // Configure base address
            var baseUrl = configService.GetEventsApiBaseUrl();
            client.BaseAddress = new Uri(baseUrl);
            
            // Configure timeout
            client.Timeout = configService.GetApiTimeout();
            
            // Configure headers
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            
            // Add API key if available
            var apiKey = configService.GetEventsApiKey();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }
            
            // Add user agent for API identification
            client.DefaultRequestHeaders.UserAgent.ParseAdd("EventsWebApp/1.0");
        });

        return services;
    }
    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // Register API communication service
        services.AddScoped<IEventsApiService, EventsApiService>();
        
        // Register mapping service for converting between models
        services.AddScoped<IModelMappingService, ModelMappingService>();
        
        return services;
    }

    private static IServiceCollection AddUtilityServices(this IServiceCollection services)
    {
        // Register error handling service
        services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
        
        return services;
    }

    public static IApplicationBuilder UseApplicationMiddleware(
        this IApplicationBuilder app, 
        IWebHostEnvironment env)
    {
        // Add error handling middleware first (catches errors from other middleware)
        app.UseMiddleware<ErrorHandlingMiddleware>();

        // Add standard ASP.NET Core middleware
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        return app;
    }

    public static IServiceCollection ValidateServiceRegistration(this IServiceCollection services)
    {
        var serviceProvider = services.BuildServiceProvider();
        
        try
        {
            serviceProvider.GetRequiredService<IConfigurationService>();
            serviceProvider.GetRequiredService<IEventsApiService>();
            serviceProvider.GetRequiredService<IModelMappingService>();
            serviceProvider.GetRequiredService<IErrorHandlingService>();
            
            // Validate HTTP client registration
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            httpClientFactory.CreateClient("EventsApi"); // Should not throw
            
            return services;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Service registration validation failed: {ex.Message}", ex);
        }
    }
}