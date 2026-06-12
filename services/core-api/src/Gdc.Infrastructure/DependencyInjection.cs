using Gdc.Infrastructure.Auth;
using Gdc.Infrastructure.Dgc;
using Gdc.Infrastructure.Dgc.Renting;
using Gdc.Infrastructure.Notif;
using Gdc.Infrastructure.Notif.EmailSenders;
using Gdc.Infrastructure.Persistence;
using Gdc.Infrastructure.Reglas;
using Gdc.Modules.Reglas.Application.Abstractions;
using Gdc.Modules.Reglas.Application.Rules.Validators;
using FluentValidation;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Notif.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<Gdc.Infrastructure.Persistence.ITenantContext, TenantContext>();

        var connectionString = configuration.GetConnectionString("Core")
            ?? throw new InvalidOperationException("Connection string 'Core' is not configured.");

        if (configuration.GetValue<bool>("Testing:UseInMemoryDatabase"))
        {
            services.AddDbContext<GdcDbContext>(options =>
                options.UseInMemoryDatabase(configuration["Testing:InMemoryDatabaseName"] ?? "gdc_test"));
        }
        else
        {
            services.AddDbContext<GdcDbContext>((serviceProvider, options) =>
            {
                var tenantContext = serviceProvider.GetRequiredService<Gdc.Infrastructure.Persistence.ITenantContext>();
                options
                    .UseNpgsql(connectionString, npgsql =>
                        npgsql.MigrationsHistoryTable("__ef_migrations_history", "core"))
                    .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .AddInterceptors(new NpgsqlTenantRlsInterceptor(tenantContext));
            });
        }

        services.AddAuthServices(configuration);

        services.Configure<DgcOcrOptions>(configuration.GetSection(DgcOcrOptions.SectionName));
        services.Configure<DgcContraventorJobOptions>(
            configuration.GetSection(DgcContraventorJobOptions.SectionName));
        services.AddScoped<OcrIngestionService>();
        services.AddScoped<ComparendoMaestraService>();
        services.AddScoped<ContraventorAssociationJob>();
        services.AddScoped<ContraventorManualService>();
        services.AddScoped<EmailLogQueryService>();
        services.AddScoped<NotifCompanyService>();
        services.AddScoped<NotifProviderService>();
        services.AddScoped<NotifTemplateService>();
        services.AddScoped<NotifRuleService>();
        services.AddValidatorsFromAssemblyContaining<CreateReglasRuleRequestValidator>();
        services.AddScoped<ReglasAccessService>();
        services.AddScoped<ReglasRuleService>();
        services.AddScoped<ReglasSecretariatContactService>();
        services.AddScoped<IReglasSecretariatResolver, ReglasSecretariatResolver>();
        services.AddScoped<ReglasExecutionJob>();
        services.AddScoped<ReglasOrchestrationJob>();
        services.AddScoped<ReglasExecutionService>();
        services.AddScoped<IGdcPdfTemplateRenderer, GdcPdfTemplateRendererStub>();
        services.AddScoped<IReglasEmailDispatcher, ReglasEmailDispatcher>();
        services.Configure<ReglasExecutionOptions>(configuration.GetSection(ReglasExecutionOptions.SectionName));
        services.AddHostedService<ReglasExecutionHostedService>();
        services.AddScoped<NotifQueueService>();
        services.AddScoped<NotifSwitchService>();
        services.AddScoped<NotifDispatchJob>();
        services.AddScoped<IDgcComparendoReader, DgcComparendoReader>();
        services.AddScoped<IEmailLogWriter, DgcEmailLogWriter>();
        services.Configure<NotifDispatchOptions>(configuration.GetSection(NotifDispatchOptions.SectionName));
        services.AddHostedService<NotifDispatchHostedService>();
        services.Configure<NotifEncryptionOptions>(configuration.GetSection(NotifEncryptionOptions.SectionName));
        services.AddSingleton<IEmailCredentialEncryptor, AesEmailCredentialEncryptor>();
        services.AddSingleton<IEmailSenderFactory, EmailSenderFactory>();
        services.AddHttpClient("NotifEmailApi");
        services.AddHttpClient("NotifSendGrid");
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
