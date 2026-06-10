using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gdc.Infrastructure.Auth;

public static class AuthDependencyInjection
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<InvitationSettings>(configuration.GetSection(InvitationSettings.SectionName));
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthSessionService, AuthSessionService>();
        services.AddScoped<IUserReadService, UserReadService>();
        services.AddScoped<IUserInvitationService, UserInvitationService>();
        services.AddSingleton<IEmailSender, LoggingEmailSender>();
        return services;
    }
}
