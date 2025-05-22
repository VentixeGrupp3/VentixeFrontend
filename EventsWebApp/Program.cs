using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
var builder = WebApplication.CreateBuilder(args);

// 1) MVC
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();


// 2) Read our Services:EventsApi section
var eventsApiSection = builder.Configuration.GetSection("Services:EventsApi");
var eventsApiBase  = eventsApiSection["BaseUrl"];
var apiKey         = eventsApiSection["AdminApiKey"];   // match your JSON key

// 3) Register named HttpClient
builder.Services.AddHttpClient("EventsApi", client =>
{
    client.BaseAddress = new Uri(eventsApiBase);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
    }
});

// 4) Logging, etc.
builder.Services.AddLogging();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name:      "default",
    pattern:   "{controller=Events}/{action=Index}/{id?}"
);

app.Run();
