using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Reglas.Infrastructure.Persistence.Configurations;

internal sealed class RuleConditionConfiguration : IEntityTypeConfiguration<RuleCondition>
{
    public void Configure(EntityTypeBuilder<RuleCondition> builder)
    {
        builder.ToTable("rule_conditions", "reglas");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(e => e.DynamicRuleId).HasColumnName("dynamic_rule_id").IsRequired();
        builder.Property(e => e.ParentId).HasColumnName("parent_id");
        builder.Property(e => e.NodeType).HasColumnName("node_type").HasMaxLength(16).IsRequired();
        builder.Property(e => e.LogicOperator).HasColumnName("logic_operator").HasMaxLength(8);
        builder.Property(e => e.FieldKey).HasColumnName("field_key").HasMaxLength(64);
        builder.Property(e => e.ComparisonOperator).HasColumnName("comparison_operator").HasMaxLength(16);
        builder.Property(e => e.ComparisonValue).HasColumnName("comparison_value").HasMaxLength(256);
        builder.Property(e => e.SortOrder).HasColumnName("sort_order").IsRequired();

        builder.HasOne(e => e.DynamicRule)
            .WithMany(r => r.Conditions)
            .HasForeignKey(e => e.DynamicRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Parent)
            .WithMany(p => p.Children)
            .HasForeignKey(e => e.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.DynamicRuleId).HasDatabaseName("ix_rule_conditions_dynamic_rule_id");
        builder.HasIndex(e => e.ParentId).HasDatabaseName("ix_rule_conditions_parent_id");
    }
}
