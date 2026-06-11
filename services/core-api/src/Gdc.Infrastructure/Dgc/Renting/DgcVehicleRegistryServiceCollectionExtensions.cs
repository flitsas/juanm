using Gdc.Infrastructure.Dgc;
using Gdc.Modules.Dgc.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Dgc.Renting;

public static class DgcVehicleRegistryServiceCollectionExtensions
{
    public static IServiceCollection AddDgcVehicleRegistry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DgcVehicleRegistryOptions>(
            configuration.GetSection(DgcVehicleRegistryOptions.SectionName));
        services.Configure<DgcRentingApiOptions>(
            configuration.GetSection(DgcRentingApiOptions.SectionName));

        var provider = configuration
            .GetSection(DgcVehicleRegistryOptions.SectionName)
            .GetValue<string>("Provider");

        if (string.Equals(provider, "Renting", StringComparison.OrdinalIgnoreCase))
        {
            services.AddMemoryCache();
            services.AddHttpClient<RentingApiClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<DgcRentingApiOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            })
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var options = sp.GetRequiredService<IOptions<DgcRentingApiOptions>>();
                var environment = sp.GetRequiredService<IHostEnvironment>();
                return RentingApiClient.CreateHandler(options, environment);
            });

            services.AddScoped<IExternalVehicleRegistry, RentingVehicleRegistry>();
        }
        else
        {
            services.AddScoped<IExternalVehicleRegistry, StubVehicleRegistry>();
        }

        return services;
    }
}
