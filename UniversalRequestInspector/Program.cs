using UniversalRequestInspector.Components;
using UniversalRequestInspector.Hubs;
using UniversalRequestInspector.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add SignalR
builder.Services.AddSignalR();

// Add our custom services
builder.Services.AddSingleton<RequestStorageService>();
builder.Services.AddHttpClient<RequestForwardingService>();
builder.Services.AddScoped<RequestForwardingService>();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Add CORS for request sinks
builder.Services.AddCors(options =>
{
    options.AddPolicy("RequestSinkPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Enable CORS for request sink endpoints
app.UseCors("RequestSinkPolicy");

app.UseAntiforgery();

// Map SignalR hub
app.MapHub<RequestNotificationHub>("/hubs/requests");

// Map controllers for request sink endpoints
app.MapControllers();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
