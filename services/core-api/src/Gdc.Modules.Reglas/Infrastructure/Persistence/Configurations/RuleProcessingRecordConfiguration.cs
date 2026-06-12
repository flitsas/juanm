using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gdc.Modules.Reglas.Infrastructure.Persistence.Configurations;

internal sealed class RuleProcessingRecordConfiguration : IEntityTypeConfiguration<RuleProcessingRecord>
{
    public void Configure(EntityTypeBuilder<RuleProcessingRecord> builder)
    {
        builder.ToTable("rule_processing_records", "reglas");
        TenantAuditableEntityConfiguration.ConfigureTenantAuditable(builder);

        builder.Property(e => e.DynamicRuleId).HasColumnName("dynamic_rule_id").IsRequired();
        builder.Property(e => e.ComparendoId).HasColumnName("comparendo_id").IsRequired();
        builder.Property(e => e.RuleExecutionRunId).HasColumnName("rule_execution_run_id");
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(16).IsRequired();
        builder.Property(e => e.ProcessedAt).HasColumnName("processed_at");
        builder.Property(e => e.PdfDocumentRef).HasColumnName("pdf_document_ref").HasMaxLength(512);
        builder.Property(e => e.EmailSendRef).HasColumnName("email_send_ref").HasMaxLength(128);
        builder.Property(e => e.ErrorMessage).HasColumnName("error_message");

        builder.HasOne(e => e.DynamicRule)
            .WithMany(r => r.ProcessingRecords)
            .HasForeignKey(e => e.DynamicRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.RuleExecutionRun)
            .WithMany(r => r.ProcessingRecords)
            .HasForeignKey(e => e.RuleExecutionRunId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.ComparendoId).HasDatabaseName("ix_rule_processing_records_comparendo_id");
        builder.HasIndex(e => e.RuleExecutionRunId).HasDatabaseName("ix_rule_processing_records_run_id");
        builder.HasIndex(e => new { e.TenantId, e.DynamicRuleId, e.ComparendoId })
            .HasDatabaseName("uq_rule_processing_records_tenant_rule_comparendo_success")
            .IsUnique()
            .HasFilter("status = 'success' AND deleted_at IS NULL");
    }
}
