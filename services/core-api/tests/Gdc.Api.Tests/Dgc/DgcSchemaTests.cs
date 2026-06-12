using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Gdc.Api.Tests.Dgc;

public sealed class DgcSchemaTests
{
    [Fact]
    public void Model_contains_required_dgc_entities()
    {
        var model = BuildModel();

        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "comparendos" && e.GetSchema() == "dgc");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "contraventors" && e.GetSchema() == "dgc");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "ocr_lotes" && e.GetSchema() == "dgc");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "ocr_items" && e.GetSchema() == "dgc");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "email_logs" && e.GetSchema() == "dgc");
    }

    [Fact]
    public void Comparendo_has_unique_constraint_on_tenant_and_numero_comparendo()
    {
        var model = BuildModel();
        var comparendo = model.FindEntityType(typeof(Comparendo))!;

        var uniqueIndex = comparendo.GetIndexes()
            .Single(i => i.IsUnique && i.Properties.Select(p => p.Name).OrderBy(n => n).SequenceEqual(
                new[] { "NumeroComparendo", "TenantId" }.OrderBy(n => n)));

        Assert.Equal("uq_comparendos_tenant_numero", uniqueIndex.GetDatabaseName());
    }

    [Fact]
    public void All_dgc_entities_have_tenant_id_column()
    {
        var model = BuildModel();
        var dgcEntities = model.GetEntityTypes().Where(e => e.GetSchema() == "dgc");

        foreach (var entity in dgcEntities)
        {
            Assert.NotNull(entity.FindProperty("TenantId"));
        }
    }

    [Fact]
    public void Migration_enables_row_level_security()
    {
        var migrationPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "Gdc.Infrastructure", "Migrations", "20260610194006_DgcInitialSchema.cs"));

        Assert.True(File.Exists(migrationPath), $"Migration file not found: {migrationPath}");

        var sql = File.ReadAllText(migrationPath);

        Assert.Contains("ENABLE ROW LEVEL SECURITY", sql);
        Assert.Contains("CREATE POLICY tenant_isolation ON dgc.comparendos", sql);
        Assert.Contains("uq_comparendos_tenant_numero", sql);
    }

    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseNpgsql("Host=localhost;Database=dgc_test;Username=test;Password=test")
            .Options;

        using var context = new GdcDbContext(options, new TestSupport.TestTenantContext());
        return context.Model;
    }
}
