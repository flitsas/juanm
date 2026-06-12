using Gdc.Modules.Reglas.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Modules.Reglas.Infrastructure.Persistence;

public static class ReglasModelBuilderExtensions
{
    public static ModelBuilder ApplyReglasConfigurations(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DynamicRuleConfiguration());
        modelBuilder.ApplyConfiguration(new RuleConditionConfiguration());
        modelBuilder.ApplyConfiguration(new RuleExecutionRunConfiguration());
        modelBuilder.ApplyConfiguration(new RuleProcessingRecordConfiguration());
        modelBuilder.ApplyConfiguration(new SecretariatContactConfiguration());
        return modelBuilder;
    }
}
