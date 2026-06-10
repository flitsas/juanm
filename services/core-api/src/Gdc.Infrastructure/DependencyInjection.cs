using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Persistence;
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

        if (configuration.GetValue<bool>("Testing:UseInMemoryDatabase"))
        {
            services.AddDbContext<GdcDbContext>(options =>
                options.UseInMemoryDatabase(configuration["Testing:InMemoryDatabaseName"] ?? "gdc_test"));
        }
        else
        {
            services.AddDbContext<GdcDbContext>(options =>
                options.UseNpgsql(connectionString, npgsql =>
                    npgsql.MigrationsHistoryTable("__ef_migrations_history", "core")));
        }

        services.AddAuthServices(configuration);

        return services;
    }
}
