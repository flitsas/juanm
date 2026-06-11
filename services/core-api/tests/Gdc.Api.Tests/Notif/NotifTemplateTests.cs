using Gdc.Infrastructure.Notif;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Notif.Application.Templates;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Notif;

public sealed class NotifTemplateTests
{
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task CreateAsync_persists_template_with_banner_and_footer()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);

        var created = await service.CreateAsync(
            new CreateTemplateRequest(
                "Notificación inicial",
                "Comparendo {{numero_comparendo}}",
                "<p>Estimado {{infractor}}</p>",
                "https://cdn.flit.dev/banner.png",
                "https://cdn.flit.dev/footer.jpg"),
            CancellationToken.None);

        Assert.Equal("Notificación inicial", created.Name);
        Assert.Contains("banner.png", created.BannerUrl);
        Assert.Single(await db.EmailTemplates.ToListAsync());
    }

    [Fact]
    public void SaveAsset_rejects_non_image_content_type()
    {
        var service = CreateService(CreateDbContext());
        using var stream = new MemoryStream([1, 2, 3]);

        var ex = Assert.Throws<ArgumentException>(
            () => service.SaveAsset(stream, "doc.pdf", "application/pdf"));

        Assert.Contains("PNG or JPG", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_rejects_template_without_body()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                new CreateTemplateRequest("Vacía", "Asunto", "  ", null, null),
                CancellationToken.None));
    }

    [Fact]
    public async Task PreviewAsync_merges_variables_and_wraps_banner()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);
        var created = await service.CreateAsync(
            new CreateTemplateRequest(
                "Preview",
                "Hola {{infractor}}",
                "<p>Placa {{placa}}</p>",
                "https://cdn.flit.dev/banner.png",
                null),
            CancellationToken.None);

        var preview = await service.PreviewAsync(
            created.Id,
            new PreviewTemplateRequest(new Dictionary<string, string>
            {
                ["infractor"] = "María",
                ["placa"] = "XYZ99",
            }),
            CancellationToken.None);

        Assert.NotNull(preview);
        Assert.Equal("Hola María", preview.Subject);
        Assert.Contains("XYZ99", preview.HtmlBody);
        Assert.Contains("banner.png", preview.HtmlBody);
    }

    [Fact]
    public async Task CreateAsync_rejects_invalid_banner_extension()
    {
        await using var db = CreateDbContext();
        var service = CreateService(db);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateAsync(
                new CreateTemplateRequest("T", "S", "<p>x</p>", "https://cdn/banner.gif", null),
                CancellationToken.None));
    }

    private static NotifTemplateService CreateService(GdcDbContext db) =>
        new(db, new FakeTenantContext(TenantId), TimeProvider.System);

    private static GdcDbContext CreateDbContext()
    {
        NotifTemplateAssetStore.Clear();
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new GdcDbContext(options);
    }

    private sealed class FakeTenantContext(Guid tenantId) : Gdc.Modules.Dgc.Application.Abstractions.ITenantContext
    {
        public Guid TenantId => tenantId;

        public Guid? UserId => Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    }
}
