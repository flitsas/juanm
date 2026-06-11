using Gdc.Infrastructure.Dgc;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Dgc.Application.Contraventor;
using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Dgc;

public sealed class ContraventorAssociationTests
{
    private static readonly Guid TenantId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    [Fact]
    public void IsWindowActive_matches_daily_cron_window()
    {
        var evaluator = new ContraventorWindowEvaluator();
        var now = new DateTimeOffset(2026, 6, 10, 8, 0, 0, TimeSpan.Zero);

        Assert.True(evaluator.IsWindowActive("0 8 * * *", now));
        Assert.False(evaluator.IsWindowActive("0 9 * * *", now));
    }

    [Fact]
    public async Task ProcessTenantAsync_associates_contraventor_when_registry_finds_match()
    {
        await using var db = CreateDbContext();
        var comparendoId = SeedPendingComparendo(db, "ABC123");
        await db.SaveChangesAsync();

        var job = new ContraventorAssociationJob(db, new FoundVehicleRegistry());
        var processed = await job.ProcessTenantAsync(TenantId, CancellationToken.None);

        Assert.Equal(1, processed);
        var comparendo = await db.Comparendos.Include(c => c.Contraventor).FirstAsync();
        Assert.False(comparendo.PendienteContraventor);
        Assert.NotNull(comparendo.Contraventor);
        Assert.Equal("Maria Lopez", comparendo.Contraventor!.Nombre);
        Assert.True(comparendo.Contraventor.AsociacionAutomatica);
    }

    [Fact]
    public async Task ProcessTenantAsync_does_not_overwrite_comparendo_infractor_fields()
    {
        await using var db = CreateDbContext();
        var comparendoId = SeedPendingComparendo(db, "ABC123", "BANCOLOMBIA S.A.", "890903938");
        await db.SaveChangesAsync();

        var job = new ContraventorAssociationJob(db, new FoundVehicleRegistry());
        await job.ProcessTenantAsync(TenantId, CancellationToken.None);

        var comparendo = await db.Comparendos.Include(c => c.Contraventor).FirstAsync();
        Assert.Equal("BANCOLOMBIA S.A.", comparendo.InfractorNombre);
        Assert.Equal("890903938", comparendo.Documento);
        Assert.Equal("Maria Lopez", comparendo.Contraventor!.Nombre);
    }

    [Fact]
    public async Task ProcessTenantAsync_marks_pendiente_when_registry_not_found()
    {
        await using var db = CreateDbContext();
        SeedPendingComparendo(db, "XYZ999");
        await db.SaveChangesAsync();

        var job = new ContraventorAssociationJob(db, new StubVehicleRegistry());
        await job.ProcessTenantAsync(TenantId, CancellationToken.None);

        var comparendo = await db.Comparendos.FirstAsync();
        Assert.True(comparendo.PendienteContraventor);
        Assert.NotNull(comparendo.UltimoIntentoAsociacion);
        Assert.Equal(0, await db.Contraventors.CountAsync());
    }

    [Fact]
    public async Task ProcessTenantAsync_keeps_pendiente_when_provider_unavailable()
    {
        await using var db = CreateDbContext();
        SeedPendingComparendo(db, "UNV001");
        await db.SaveChangesAsync();

        var job = new ContraventorAssociationJob(db, new UnavailableVehicleRegistry());
        await job.ProcessTenantAsync(TenantId, CancellationToken.None);

        var comparendo = await db.Comparendos.FirstAsync();
        Assert.True(comparendo.PendienteContraventor);
        Assert.NotNull(comparendo.UltimoIntentoAsociacion);
        Assert.Equal(0, await db.Contraventors.CountAsync());
    }

    [Fact]
    public async Task ProcessTenantAsync_associates_with_empty_documento_when_only_nombre_from_renting()
    {
        await using var db = CreateDbContext();
        SeedPendingComparendo(db, "LZW620");
        await db.SaveChangesAsync();

        var job = new ContraventorAssociationJob(db, new PartialRentingRegistry());
        await job.ProcessTenantAsync(TenantId, CancellationToken.None);

        var comparendo = await db.Comparendos.Include(c => c.Contraventor).FirstAsync();
        Assert.False(comparendo.PendienteContraventor);
        Assert.NotNull(comparendo.Contraventor);
        Assert.Equal("SANTIAGO URIBE MARQUEZ", comparendo.Contraventor!.Nombre);
        Assert.Equal(string.Empty, comparendo.Contraventor.Documento);
        Assert.Equal("santiagouri@gmail.com", comparendo.Contraventor.Correo);
        Assert.True(comparendo.Contraventor.AsociacionAutomatica);
    }

    [Fact]
    public async Task UpsertAsync_persists_manual_contraventor_fields()
    {
        await using var db = CreateDbContext();
        var comparendoId = SeedPendingComparendo(db, "MAN001");
        await db.SaveChangesAsync();

        var service = new ContraventorManualService(db, new FakeTenantContext(TenantId));
        var (success, response, errorCode) = await service.UpsertAsync(
            comparendoId,
            new UpsertContraventorRequest("Pedro Ruiz", "99887766", "pedro@flit.test"),
            CancellationToken.None);

        Assert.True(success);
        Assert.Null(errorCode);
        Assert.NotNull(response);
        Assert.Equal("Pedro Ruiz", response!.Nombre);
        Assert.Equal("99887766", response.Documento);
        Assert.Equal("pedro@flit.test", response.Correo);
        Assert.False(response.AsociacionAutomatica);

        var comparendo = await db.Comparendos.Include(c => c.Contraventor).FirstAsync();
        Assert.False(comparendo.PendienteContraventor);
        Assert.Equal("Pedro Ruiz", comparendo.Contraventor!.Nombre);
        Assert.Null(comparendo.InfractorNombre);
    }

    [Fact]
    public async Task UpsertAsync_rejects_empty_nombre()
    {
        await using var db = CreateDbContext();
        var comparendoId = SeedPendingComparendo(db, "ERR001");
        await db.SaveChangesAsync();

        var service = new ContraventorManualService(db, new FakeTenantContext(TenantId));
        var (success, _, errorCode) = await service.UpsertAsync(
            comparendoId,
            new UpsertContraventorRequest("  ", "123", null),
            CancellationToken.None);

        Assert.False(success);
        Assert.Equal("DGC_CONTRAVENTOR_INVALID", errorCode);
    }

    private static Guid SeedPendingComparendo(
        GdcDbContext db,
        string placa,
        string? infractorNombre = null,
        string? documento = null)
    {
        var comparendoId = Guid.CreateVersion7();
        db.Comparendos.Add(new Comparendo
        {
            Id = comparendoId,
            TenantId = TenantId,
            NumeroComparendo = $"CMP-{placa}",
            Estado = "Pendiente",
            Placa = placa,
            InfractorNombre = infractorNombre,
            Documento = documento,
            FechaComparendo = new DateOnly(2026, 5, 15),
            Fuente = "ocr",
            PendienteContraventor = true,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        return comparendoId;
    }

    private static GdcDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GdcDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GdcDbContext(options);
    }

    private sealed class FoundVehicleRegistry : IExternalVehicleRegistry
    {
        public Task<ContraventorLookupResult> LookupAsync(
            string placa,
            DateOnly fechaInfraccion,
            CancellationToken cancellationToken) =>
            Task.FromResult(new ContraventorLookupResult(
                ContraventorLookupStatus.Found,
                new ContraventorLookupData("Maria Lopez", "55443322", "maria@flit.test")));
    }

    private sealed class PartialRentingRegistry : IExternalVehicleRegistry
    {
        public Task<ContraventorLookupResult> LookupAsync(
            string placa,
            DateOnly fechaInfraccion,
            CancellationToken cancellationToken) =>
            Task.FromResult(new ContraventorLookupResult(
                ContraventorLookupStatus.Found,
                new ContraventorLookupData("SANTIAGO URIBE MARQUEZ", string.Empty, "santiagouri@gmail.com")));
    }

    private sealed class UnavailableVehicleRegistry : IExternalVehicleRegistry
    {
        public Task<ContraventorLookupResult> LookupAsync(
            string placa,
            DateOnly fechaInfraccion,
            CancellationToken cancellationToken) =>
            Task.FromResult(new ContraventorLookupResult(ContraventorLookupStatus.ProviderUnavailable));
    }

    private sealed class FakeTenantContext(Guid tenantId) : Gdc.Modules.Dgc.Application.Abstractions.ITenantContext
    {
        public Guid TenantId => tenantId;

        public Guid? UserId => null;
    }
}
