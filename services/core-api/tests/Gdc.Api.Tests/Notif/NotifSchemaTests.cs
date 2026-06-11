using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Gdc.Api.Tests.Notif;

public sealed class NotifSchemaTests
{
    [Fact]
    public void Model_contains_required_notif_entities()
    {
        var model = BuildModel();

        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "email_provider_configs" && e.GetSchema() == "notif");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "email_templates" && e.GetSchema() == "notif");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "notification_rules" && e.GetSchema() == "notif");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "email_queues" && e.GetSchema() == "notif");
        Assert.Contains(model.GetEntityTypes(), e => e.GetTableName() == "email_send_logs" && e.GetSchema() == "notif");
    }

    [Fact]
    public void All_notif_entities_have_tenant_id_column()
    {
        var model = BuildModel();
        var notifEntities = model.GetEntityTypes().Where(e => e.GetSchema() == "notif");

        foreach (var entity in notifEntities)
        {
            Assert.NotNull(entity.FindProperty("TenantId"));
        }
    }

    [Fact]
    public void Email_provider_has_unique_active_per_tenant_index()
    {
        var model = BuildModel();
        var provider = model.FindEntityType(typeof(EmailProviderConfig))!;

        var index = provider.GetIndexes()
            .Single(i => i.GetDatabaseName() == "ix_email_provider_configs_tenant_active");

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Migration_enables_row_level_security()
    {
        var migrationPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..",
            "src", "Gdc.Infrastructure", "Migrations"));

        var migrationFile = Directory.GetFiles(migrationPath, "*NotifInitialSchema.cs")
            .SingleOrDefault();

        Assert.NotNull(migrationFile);

        var sql = File.ReadAllText(migrationFile);

        Assert.Contains("ENABLE ROW LEVEL SECURITY", sql);
        Assert.Contains("CREATE POLICY tenant_isolation ON notif.email_provider_configs", sql);
        Assert.Contains("schema: \"notif\"", sql);
    }

    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseNpgsql("Host=localhost;Database=notif_test;Username=test;Password=test")
            .Options;

        using var context = new GdcDbContext(options);
        return context.Model;
    }
}
