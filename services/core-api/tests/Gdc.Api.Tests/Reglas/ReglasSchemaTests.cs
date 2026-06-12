using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Gdc.Api.Tests.Reglas;

public sealed class ReglasSchemaTests
{
    [Fact]
    public void Model_contains_required_reglas_entities()
    {
        var model = BuildModel();

        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "dynamic_rules" && e.GetSchema() == "reglas");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "rule_conditions" && e.GetSchema() == "reglas");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "rule_execution_runs" && e.GetSchema() == "reglas");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "rule_processing_records" && e.GetSchema() == "reglas");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "secretariat_contacts" && e.GetSchema() == "reglas");
    }

    [Fact]
    public void All_reglas_entities_have_tenant_id_column()
    {
        var model = BuildModel();
        var reglasEntities = model.GetEntityTypes().Where(e => e.GetSchema() == "reglas");

        foreach (var entity in reglasEntities)
        {
            Assert.NotNull(entity.FindProperty("TenantId"));
        }
    }

    [Fact]
    public void Rule_processing_record_has_unique_success_per_rule_comparendo_index()
    {
        var model = BuildModel();
        var record = model.FindEntityType(typeof(RuleProcessingRecord))!;

        var index = record.GetIndexes()
            .Single(i => i.GetDatabaseName() == "uq_rule_processing_records_tenant_rule_comparendo_success");

        Assert.True(index.IsUnique);
        Assert.Equal("status = 'success' AND deleted_at IS NULL", index.GetFilter());
    }

    [Fact]
    public void Migration_enables_row_level_security()
    {
        var migrationPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "Gdc.Infrastructure", "Migrations"));

        var migrationFile = Directory.GetFiles(migrationPath, "*ReglasInitialSchema.cs")
            .SingleOrDefault();

        Assert.NotNull(migrationFile);

        var sql = File.ReadAllText(migrationFile);

        Assert.Contains("ENABLE ROW LEVEL SECURITY", sql);
        Assert.Contains("CREATE POLICY tenant_isolation ON reglas.dynamic_rules", sql);
        Assert.Contains("schema: \"reglas\"", sql);
    }

    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseNpgsql("Host=localhost;Database=reglas_test;Username=test;Password=test")
            .Options;

        using var context = new GdcDbContext(options, new TestSupport.TestTenantContext());
        return context.Model;
    }
}
