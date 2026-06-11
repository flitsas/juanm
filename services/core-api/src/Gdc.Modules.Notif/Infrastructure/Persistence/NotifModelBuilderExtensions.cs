using Gdc.Modules.Notif.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Modules.Notif.Infrastructure.Persistence;

public static class NotifModelBuilderExtensions
{
    public static ModelBuilder ApplyNotifConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new EmailProviderConfigConfiguration());
        modelBuilder.ApplyConfiguration(new EmailTemplateConfiguration());
        modelBuilder.ApplyConfiguration(new NotificationRuleConfiguration());
        modelBuilder.ApplyConfiguration(new EmailQueueConfiguration());
        modelBuilder.ApplyConfiguration(new EmailSendLogConfiguration());
        return modelBuilder;
    }
}
