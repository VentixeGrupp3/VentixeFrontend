using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Identity;
using WebApp.Protos;
using WebApp.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<VerificationService>();

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

// Viv invoice inclusions
builder.Services.AddHttpClient("UserClient", client =>
{
    client.BaseAddress = new Uri("https://ventixe-invoice-microservice-group3.azurewebsites.net/api/");
    client.DefaultRequestHeaders.Add("x-api-key", "1a76c263-4d83-4c98-b913-9029f9dfad7d");
});

// for admins
builder.Services.AddHttpClient("AdminClient", client =>
{
    client.BaseAddress = new Uri("https://ventixe-invoice-microservice-group3.azurewebsites.net/api/");
    client.DefaultRequestHeaders.Add("x-api-key", "fba16aa0-4bb4-4bb7-9201-d81937292329");
});

builder.Services.AddScoped<InvoiceApiService.IInvoiceApiClient>(sp =>
{
    // grab the current user
    var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext!;
    // pick the right named client by role
    var clientName = httpContext.User.IsInRole("Admin")
        ? "AdminClient"
        : "UserClient";

    // get the HttpClientFactory and create the named client
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient(clientName);

    // return your concrete implementation
    return new InvoiceApiService.InvoiceApiClient(httpClient);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();
app.UseHsts();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.Run();
