using EventsWebApp.Configuration;
using EventsWebApp.Middleware;
using EventsWebApp.Services.Implementation;
using EventsWebApp.Services.Interfaces;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        services.AddHealthCheckServices();

        return services;
    }

    private static IServiceCollection AddHealthCheckServices(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("application-startup", () => 
                HealthCheckResult.Healthy("Application has started successfully"))
            .AddCheck<EventsApiHealthCheck>("events-api-connectivity");
    
        services.AddScoped<EventsApiHealthCheck>();
    
        return services;
    }
    private static IServiceCollection AddConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<ApiConfiguration>(
            configuration.GetSection(ApiConfiguration.SectionName));
        
        services.Configure<ApplicationConfiguration>(
            configuration.GetSection(ApplicationConfiguration.SectionName));

        services.AddSingleton<IConfigurationService, ConfigurationService>();

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
            
            var baseUrl = configService.GetEventsApiBaseUrl();
            client.BaseAddress = new Uri(baseUrl);
            
            client.Timeout = configService.GetApiTimeout();
            
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            
            var apiKey = configService.GetEventsApiKey();
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            }
            
            client.DefaultRequestHeaders.UserAgent.ParseAdd("EventsWebApp/1.0");
        });

        return services;
    }
    private static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IEventsApiService, EventsApiService>();
        
        services.AddScoped<IModelMappingService, ModelMappingService>();
        
        return services;
    }

    private static IServiceCollection AddUtilityServices(this IServiceCollection services)
    {
        services.AddScoped<IErrorHandlingService, ErrorHandlingService>();
        
        return services;
    }

    public static IApplicationBuilder UseApplicationMiddleware(
        this IApplicationBuilder app, 
        IWebHostEnvironment env)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();

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