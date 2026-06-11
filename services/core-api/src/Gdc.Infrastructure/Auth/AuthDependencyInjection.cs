using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Infrastructure.Auth;

public static class AuthDependencyInjection
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<InvitationSettings>(configuration.GetSection(InvitationSettings.SectionName));
        services.Configure<PasswordRecoverySettings>(configuration.GetSection(PasswordRecoverySettings.SectionName));
        services.Configure<LockoutSettings>(configuration.GetSection(LockoutSettings.SectionName));
        services.Configure<SmtpSettings>(options => SmtpSettings.Bind(configuration, options));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthSessionService, AuthSessionService>();
        services.AddScoped<IUserReadService, UserReadService>();
        services.AddScoped<IUserInvitationService, UserInvitationService>();
        services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
        services.AddScoped<IRbacMatrixService, RbacMatrixService>();
        var smtpProbe = new SmtpSettings();
        SmtpSettings.Bind(configuration, smtpProbe);
        if (smtpProbe.Enabled)
        {
            services.AddSingleton<IEmailSender, SmtpEmailSender>();
        }
        else
        {
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
        }

        return services;
    }
}
