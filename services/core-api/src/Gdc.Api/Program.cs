using Gdc.Api;
using Gdc.Api.Auth;
using Gdc.Api.Configuration;
using Gdc.Api.Endpoints;
using Gdc.Api.Tenancy;
using Gdc.Infrastructure;
using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Dgc;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Notif.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;

DotEnvLoader.LoadIfPresent();
DotEnvLoader.LoadRentingEnvFromRepoRoot();

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile(
        "appsettings.Development.local.json",
        optional: true,
        reloadOnChange: true);
}

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHostedService<ContraventorAssociationHostedService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<DgcTenantContext, HeaderTenantContext>();
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
    var smtpSettings = new SmtpSettings();
    SmtpSettings.Bind(app.Configuration, smtpSettings);
    var emailSender = app.Services.GetRequiredService<Gdc.Infrastructure.Auth.IEmailSender>();
    app.Logger.LogInformation(
        "Correo: {SenderType} | SMTP {Host}:{Port} | From {From} | Enabled {Enabled}",
        emailSender.GetType().Name,
        string.IsNullOrWhiteSpace(smtpSettings.Host) ? "(sin host)" : smtpSettings.Host,
        smtpSettings.Port,
        string.IsNullOrWhiteSpace(smtpSettings.FromAddress) ? "(sin remitente)" : smtpSettings.FromAddress,
        smtpSettings.Enabled);
}

if (app.Environment.IsDevelopment()
    && !app.Configuration.GetValue<bool>("Testing:UseInMemoryDatabase"))
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GdcDbContext>();
    await db.Database.MigrateAsync();
    await DevDataSeeder.SeedIfEmptyAsync(db);
}
else if (app.Environment.IsDevelopment())
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
app.MapDgcOcrEndpoints();
app.MapDgcComparendoEndpoints();
app.MapDgcEmailLogEndpoints();
app.MapNotifCompanyEndpoints();
app.MapNotifProviderEndpoints();
app.MapNotifTemplateEndpoints();
app.MapNotifRuleEndpoints();
app.MapReglasRuleEndpoints();

app.Run();

public partial class Program;
