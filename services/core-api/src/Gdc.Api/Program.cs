using Gdc.Api.Auth;
using Gdc.Api.Endpoints;
using Gdc.Infrastructure;
using Gdc.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration["Cors:Origins"]?.Split(';', StringSplitOptions.RemoveEmptyEntries)
                ?? ["http://localhost:4001"])
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseTenantContext();
app.UseAuthorization();

app.MapGet("/health", async (GdcDbContext db, CancellationToken ct) =>
{
    var canConnect = await db.Database.CanConnectAsync(ct);
    return Results.Ok(new
    {
        status = canConnect ? "ok" : "degraded",
        env = app.Environment.EnvironmentName,
        database = canConnect ? "connected" : "unavailable",
    });
})
.WithName("HealthCheck")
.WithTags("System");

app.MapAuthEndpoints();

app.Run();

public partial class Program;
