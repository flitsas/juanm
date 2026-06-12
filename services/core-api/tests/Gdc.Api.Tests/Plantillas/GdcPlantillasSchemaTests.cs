using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Plantillas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Gdc.Api.Tests.Plantillas;

[Collection("Plantillas")]
public sealed class GdcPlantillasSchemaTests
{
    [Fact]
    public void Model_contains_required_gdc_plantillas_entities()
    {
        var model = BuildModel();

        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "pdf_templates" && e.GetSchema() == "gdc");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "pdf_template_fields" && e.GetSchema() == "gdc");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "derechos_peticion" && e.GetSchema() == "gdc");
    }

    [Fact]
    public void All_gdc_plantillas_entities_have_tenant_id_column()
    {
        var model = BuildModel();
        var plantillasEntities = model.GetEntityTypes()
            .Where(e => e.GetSchema() == "gdc" && e.ClrType.Namespace?.Contains("Plantillas") == true);

        Assert.Equal(3, plantillasEntities.Count());

        foreach (var entity in plantillasEntities)
        {
            Assert.NotNull(entity.FindProperty("TenantId"));
        }
    }

    [Fact]
    public void Pdf_template_has_unique_name_per_tenant_index()
    {
        var model = BuildModel();
        var template = model.FindEntityType(typeof(PdfTemplate))!;

        var index = template.GetIndexes()
            .Single(i => i.GetDatabaseName() == "uq_pdf_templates_tenant_name");

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Migration_enables_row_level_security()
    {
        var migrationPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "Gdc.Infrastructure", "Migrations"));

        var migrationFile = Directory.GetFiles(migrationPath, "*GdcPlantillasInitialSchema.cs")
            .SingleOrDefault(f => !f.EndsWith(".Designer.cs", StringComparison.Ordinal));

        Assert.NotNull(migrationFile);

        var sql = File.ReadAllText(migrationFile);

        Assert.Contains("ENABLE ROW LEVEL SECURITY", sql);
        Assert.Contains("CREATE POLICY tenant_isolation ON gdc.pdf_templates", sql);
        Assert.Contains("schema: \"gdc\"", sql);
        Assert.Contains("fk_derechos_peticion_comparendos", sql);
    }

    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseNpgsql("Host=localhost;Database=plantillas_test;Username=test;Password=test")
            .Options;

        using var context = new GdcDbContext(options, new TestSupport.TestTenantContext());
        return context.Model;
    }
}
