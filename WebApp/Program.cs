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
});
builder.Services.AddGrpcClient<UserProfileProtoService.UserProfileProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration.GetConnectionString("UserProfileGrpcConnectionString"));
});



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
