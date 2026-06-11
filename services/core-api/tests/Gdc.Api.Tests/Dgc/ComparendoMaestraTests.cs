using Gdc.Infrastructure.Dgc;
using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Dgc.Application.Maestra;
using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Api.Tests.Dgc;

public sealed class ComparendoMaestraTests
{
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid SecretariaId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void Calculate_uses_secretaria_matrix_over_global()
    {
        var matrices = new List<DescuentoMatriz>
        {
            CreateMatrix(null, 30),
            CreateMatrix(SecretariaId, 10),
        };

        var dias = DiscountDaysCalculator.Calculate(
            new DateOnly(2025, 6, 1),
            matrices,
            SecretariaId,
            new DateOnly(2025, 6, 5));

        Assert.Equal(6, dias);
    }

    [Fact]
    public void Calculate_falls_back_to_global_matrix()
    {
        var matrices = new List<DescuentoMatriz> { CreateMatrix(null, 20) };

        var dias = DiscountDaysCalculator.Calculate(
            new DateOnly(2025, 6, 1),
            matrices,
            SecretariaId,
            new DateOnly(2025, 6, 11));

        Assert.Equal(10, dias);
    }

    [Fact]
    public async Task ListAsync_returns_fifteen_columns_and_pagination()
    {
        await using var db = CreateDbContext();
        SeedComparendos(db);
        await db.SaveChangesAsync();

        var service = new ComparendoMaestraService(db, new FakeTenantContext(TenantId));
        var result = await service.ListAsync(
            new ComparendoMaestraQuery(Estado: "Pendiente", Page: 1, PageSize: 10),
            CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        var item = Assert.Single(result.Items);
        Assert.Equal("CMP-100", item.NumeroComparendo);
        Assert.Equal("Pendiente", item.Estado);
        Assert.Equal("ABC123", item.Placa);
        Assert.Equal("ocr", item.Fuente);
        Assert.Equal("Juan (123)", item.Contraventor);
        Assert.Equal("Juan", item.ContraventorNombre);
        Assert.Equal("123", item.ContraventorDocumento);
        Assert.NotNull(item.DiasRestantes);
    }

    [Fact]
    public async Task ListAsync_filters_by_placa()
    {
        await using var db = CreateDbContext();
        SeedComparendos(db);
        db.Comparendos.Add(new Comparendo
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            NumeroComparendo = "CMP-200",
            Estado = "Pagado",
            Placa = "XYZ999",
            Fuente = "manual",
            PendienteContraventor = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var service = new ComparendoMaestraService(db, new FakeTenantContext(TenantId));
        var result = await service.ListAsync(
            new ComparendoMaestraQuery(Placa: "xyz999"),
            CancellationToken.None);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("CMP-200", result.Items[0].NumeroComparendo);
    }

    [Fact]
    public async Task GetByIdAsync_recalculates_dias_restantes()
    {
        await using var db = CreateDbContext();
        SeedComparendos(db);
        await db.SaveChangesAsync();

        var comparendoId = await db.Comparendos.Select(c => c.Id).FirstAsync();
        var service = new ComparendoMaestraService(db, new FakeTenantContext(TenantId));

        var item = await service.GetByIdAsync(comparendoId, CancellationToken.None);

        Assert.NotNull(item);
        Assert.Equal(15, item!.DiasRestantes);
    }

    private static void SeedComparendos(GdcDbContext db)
    {
        var comparendoId = Guid.CreateVersion7();
        db.DescuentoMatrices.Add(CreateMatrix(null, 20));
        db.Comparendos.Add(new Comparendo
        {
            Id = comparendoId,
            TenantId = TenantId,
            NumeroComparendo = "CMP-100",
            Estado = "Pendiente",
            Placa = "ABC123",
            FechaComparendo = new DateOnly(2025, 5, 1),
            FechaNotificacion = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5),
            TotalValor = 500000m,
            Fuente = "ocr",
            PendienteContraventor = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        db.Contraventors.Add(new Contraventor
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            ComparendoId = comparendoId,
            Nombre = "Juan",
            Documento = "123",
            AsociacionAutomatica = false,
            CreatedAt = DateTimeOffset.UtcNow,
        });
    }

    private static DescuentoMatriz CreateMatrix(Guid? secretariaId, int dias) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            TenantId = TenantId,
            SecretariaId = secretariaId,
            DiasDescuento = dias,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };

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
