using Gdc.Infrastructure.Dgc;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Dgc;

public sealed class EmailLogQueryTests
{
    private static readonly Guid TenantId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    [Fact]
    public async Task ListByComparendoAsync_returns_tracking_fields_ordered_by_sent_at()
    {
        await using var db = CreateDbContext();
        var comparendoId = SeedComparendo(db);
        var olderId = SeedEmailLog(db, comparendoId, new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero));
        var newerId = SeedEmailLog(db, comparendoId, new DateTimeOffset(2026, 6, 2, 15, 30, 0, TimeSpan.Zero));
        await db.SaveChangesAsync();

        var service = new EmailLogQueryService(db, new FakeTenantContext(TenantId));
        var result = await service.ListByComparendoAsync(comparendoId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Items.Count);
        Assert.Equal(newerId, result.Items[0].Id);
        Assert.Equal(olderId, result.Items[1].Id);

        var item = result.Items[0];
        Assert.Equal("notificaciones@flit.test", item.Origen);
        Assert.Equal("contraventor@flit.test", item.Destino);
        Assert.Equal("cc@flit.test", item.Cc);
        Assert.Equal("Recordatorio", item.TipoAlerta);
        Assert.Equal("Entregado", item.EstadoEntrega);
    }

    [Fact]
    public async Task ListByComparendoAsync_returns_null_when_comparendo_missing()
    {
        await using var db = CreateDbContext();
        var service = new EmailLogQueryService(db, new FakeTenantContext(TenantId));

        var result = await service.ListByComparendoAsync(Guid.CreateVersion7(), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task ListByComparendoAsync_returns_empty_list_when_no_emails()
    {
        await using var db = CreateDbContext();
        var comparendoId = SeedComparendo(db);
        await db.SaveChangesAsync();

        var service = new EmailLogQueryService(db, new FakeTenantContext(TenantId));
        var result = await service.ListByComparendoAsync(comparendoId, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result!.Items);
    }

    [Fact]
    public async Task GetEvidenceAsync_returns_html_when_stored()
    {
        await using var db = CreateDbContext();
        var comparendoId = SeedComparendo(db);
        var emailId = SeedEmailLog(db, comparendoId, DateTimeOffset.UtcNow, "<p>Hola contraventor</p>");
        await db.SaveChangesAsync();

        var service = new EmailLogQueryService(db, new FakeTenantContext(TenantId));
        var evidence = await service.GetEvidenceAsync(emailId, CancellationToken.None);

        Assert.NotNull(evidence);
        Assert.Equal(emailId, evidence!.Id);
        Assert.Equal("<p>Hola contraventor</p>", evidence.HtmlEvidencia);
    }

    [Fact]
    public async Task GetEvidenceAsync_returns_null_when_html_missing()
    {
        await using var db = CreateDbContext();
        var comparendoId = SeedComparendo(db);
        var emailId = SeedEmailLog(db, comparendoId, DateTimeOffset.UtcNow);
        await db.SaveChangesAsync();

        var service = new EmailLogQueryService(db, new FakeTenantContext(TenantId));
        var evidence = await service.GetEvidenceAsync(emailId, CancellationToken.None);

        Assert.Null(evidence);
    }

    private static Guid SeedComparendo(GdcDbContext db)
    {
        var comparendoId = Guid.CreateVersion7();
        db.Comparendos.Add(new Comparendo
        {
            Id = comparendoId,
            TenantId = TenantId,
            NumeroComparendo = "CMP-EMAIL-1",
            Estado = "Pendiente",
            Fuente = "manual",
            PendienteContraventor = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        return comparendoId;
    }

    private static Guid SeedEmailLog(
        GdcDbContext db,
        Guid comparendoId,
        DateTimeOffset sentAt,
        string? html = null)
    {
        var emailId = Guid.CreateVersion7();
        db.EmailLogs.Add(new EmailLog
        {
            Id = emailId,
            TenantId = TenantId,
            ComparendoId = comparendoId,
            SentAt = sentAt,
            Origen = "notificaciones@flit.test",
            Destino = "contraventor@flit.test",
            Cc = "cc@flit.test",
            TipoAlerta = "Recordatorio",
            EstadoEntrega = "Entregado",
            HtmlEvidencia = html,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        return emailId;
    }

    private static GdcDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GdcDbContext(options);
    }

    private sealed class FakeTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid TenantId => tenantId;

        public Guid? UserId => null;
    }
}
