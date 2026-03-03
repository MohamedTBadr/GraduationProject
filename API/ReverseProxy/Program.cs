using Yarp.ReverseProxy;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to use HTTPS
builder.WebHost.ConfigureKestrel(options =>
{
    // Listen on HTTPS port 5000 with dev certificate
    options.ListenLocalhost(5000, listenOptions =>
    {
        listenOptions.UseHttps(); // Uses the dev certificate
        
    });
});

// Add YARP reverse proxy, reading configuration from appsettings.json
builder.Services.AddReverseProxy()
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Redirect HTTP to HTTPS (optional)
app.UseHttpsRedirection();

// Map reverse proxy
app.MapReverseProxy();

// Run the app
app.Run();
