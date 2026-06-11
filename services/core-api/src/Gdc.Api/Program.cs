using Gdc.Api.Configuration;
using Gdc.Api.Endpoints;
using Gdc.Api.Tenancy;
using Gdc.Infrastructure;
using Gdc.Infrastructure.Dgc;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Notif.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

DotEnvLoader.LoadRentingEnvFromRepoRoot();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<ContraventorAssociationHostedService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HeaderTenantContext>();
builder.Services.AddScoped<IUserRoleContext, HeaderUserRoleContext>();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration["Cors:Origins"]?.Split(';', StringSplitOptions.RemoveEmptyEntries)
                ?? ["http://localhost:40103"])
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

app.MapDgcOcrEndpoints();
app.MapDgcComparendoEndpoints();
app.MapDgcEmailLogEndpoints();
app.MapNotifCompanyEndpoints();
app.MapNotifProviderEndpoints();
app.MapNotifTemplateEndpoints();
app.MapNotifRuleEndpoints();

app.Run();
