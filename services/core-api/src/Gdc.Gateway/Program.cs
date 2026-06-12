// Gdc.Gateway — API publica YARP (ADR-0017). Expone /api y /hubs y los enruta al
// servicio interno core-api. Las rutas/clusters viven en appsettings.json (seccion
// "ReverseProxy") para poder ajustarlos sin recompilar.

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? ["http://localhost:40103"];

        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // necesario para SignalR (/hubs) con credenciales.
    });
});

var app = builder.Build();

app.UseCors();

// Health propio del gateway (el compose hace wget a /health en :8080).
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "gateway" }))
    .WithName("GatewayHealth");

app.MapReverseProxy();

app.Run();
