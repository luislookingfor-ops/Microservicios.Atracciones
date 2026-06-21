var builder = WebApplication.CreateBuilder(args);

// Add MVC controllers
builder.Services.AddControllers();

// Add HttpClient
builder.Services.AddHttpClient();

// Add SignalR
builder.Services.AddSignalR();

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

app.UseCors("AllowAll");

// Map controllers for orchestrated endpoints
app.MapControllers();

// Map SignalR Hub
app.MapHub<Microservicios.Atracciones.Gateway.Hubs.NotificationHub>("/hub/notifications");

app.MapReverseProxy();

app.Run();
