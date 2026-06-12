using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Reglas.Infrastructure.Persistence.Configurations;

internal sealed class RuleExecutionRunConfiguration : IEntityTypeConfiguration<RuleExecutionRun>
{
    public void Configure(EntityTypeBuilder<RuleExecutionRun> builder)
    {
        builder.ToTable("rule_execution_runs", "reglas");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(e => e.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(e => e.FinishedAt).HasColumnName("finished_at");
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(16).IsRequired();
        builder.Property(e => e.TriggerType).HasColumnName("trigger_type").HasMaxLength(16).IsRequired();
        builder.Property(e => e.EvaluatedCount).HasColumnName("evaluated_count").IsRequired();
        builder.Property(e => e.MatchedCount).HasColumnName("matched_count").IsRequired();
        builder.Property(e => e.ProcessedCount).HasColumnName("processed_count").IsRequired();
        builder.Property(e => e.FailedCount).HasColumnName("failed_count").IsRequired();
        builder.Property(e => e.ErrorMessage).HasColumnName("error_message");

        builder.HasIndex(e => new { e.TenantId, e.StartedAt })
            .HasDatabaseName("ix_rule_execution_runs_tenant_started");
    }
}
