using Gdc.Infrastructure.Dgc;
using Gdc.Infrastructure.Dgc.Renting;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Core")
            ?? throw new InvalidOperationException("Connection string 'Core' is not configured.");

        services.AddDbContext<GdcDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__ef_migrations_history", "core")));

        services.Configure<DgcOcrOptions>(configuration.GetSection(DgcOcrOptions.SectionName));
        services.Configure<DgcContraventorJobOptions>(
            configuration.GetSection(DgcContraventorJobOptions.SectionName));
        services.AddScoped<OcrIngestionService>();
        services.AddScoped<ComparendoMaestraService>();
        services.AddScoped<ContraventorAssociationJob>();
        services.AddScoped<ContraventorManualService>();
        services.AddScoped<EmailLogQueryService>();
        services.AddSingleton<ContraventorWindowEvaluator>();
        services.AddSingleton(TimeProvider.System);
        services.AddDgcVehicleRegistry(configuration);
        services.AddHttpClient<IOcrExtractor, HttpOcrExtractor>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<DgcOcrOptions>>().Value;
            client.BaseAddress = new Uri(options.PythonMlBaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        return services;
    }
}
