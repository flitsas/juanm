using Gdc.Infrastructure.Persistence;
using Gdc.Modules.Dgc.Application.Abstractions;
using Gdc.Modules.Dgc.Application.Maestra;
using Gdc.Modules.Dgc.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Dgc;

public sealed class ComparendoMaestraService(GdcDbContext db, ITenantContext tenantContext)
{
    public async Task<ComparendoMaestraListResult> ListAsync(
        ComparendoMaestraQuery query,
        CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var comparendosQuery = db.Comparendos
            .AsNoTracking()
            .Include(c => c.Contraventor)
            .Where(c => c.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(query.Estado))
        {
            comparendosQuery = comparendosQuery.Where(c => c.Estado == query.Estado);
        }

        if (!string.IsNullOrWhiteSpace(query.Placa))
        {
            var placa = query.Placa.Trim().ToUpperInvariant();
            comparendosQuery = comparendosQuery.Where(c => c.Placa != null && c.Placa.ToUpper() == placa);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLowerInvariant();
            comparendosQuery = comparendosQuery.Where(c =>
                c.NumeroComparendo.ToLower().Contains(search)
                || (c.Placa != null && c.Placa.ToLower().Contains(search))
                || (c.InfractorNombre != null && c.InfractorNombre.ToLower().Contains(search))
                || (c.Documento != null && c.Documento.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(query.Secretaria))
        {
            var secretaria = query.Secretaria.Trim();
            comparendosQuery = comparendosQuery.Where(c =>
                c.SecretariaId != null && c.SecretariaId.ToString() == secretaria);
        }

        if (query.FechaDesde is not null)
        {
            comparendosQuery = comparendosQuery.Where(c =>
                c.FechaComparendo != null && c.FechaComparendo >= query.FechaDesde);
        }

        if (query.FechaHasta is not null)
        {
            comparendosQuery = comparendosQuery.Where(c =>
                c.FechaComparendo != null && c.FechaComparendo <= query.FechaHasta);
        }

        comparendosQuery = ApplySort(comparendosQuery, query.SortBy, query.SortDir);

        var total = await comparendosQuery.CountAsync(cancellationToken);
        var items = await comparendosQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var matrices = await db.DescuentoMatrices
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.IsActive)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(c => MapToDto(c, matrices)).ToList();
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        return new ComparendoMaestraListResult(dtos, page, pageSize, total, totalPages);
    }

    public async Task<ComparendoMaestraItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        var comparendo = await db.Comparendos
            .AsNoTracking()
            .Include(c => c.Contraventor)
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId, cancellationToken);

        if (comparendo is null)
        {
            return null;
        }

        var matrices = await db.DescuentoMatrices
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId && m.IsActive)
            .ToListAsync(cancellationToken);

        return MapToDto(comparendo, matrices);
    }

    private static ComparendoMaestraItemDto MapToDto(Comparendo c, IReadOnlyList<DescuentoMatriz> matrices) =>
        new(
            c.Id,
            c.Estado,
            c.NumeroComparendo,
            c.InfractorNombre,
            c.Documento,
            c.Placa,
            c.InfraccionCodigo,
            c.FechaComparendo,
            c.FechaNotificacion,
            DiscountDaysCalculator.Calculate(c.FechaNotificacion, matrices, c.SecretariaId),
            c.SecretariaId?.ToString(),
            c.TotalValor,
            c.EstadoPago,
            FormatContraventor(c),
            c.Contraventor?.Nombre,
            c.Contraventor?.Documento,
            c.Contraventor?.Correo,
            c.DpReferencia,
            c.Fuente);

    private static string? FormatContraventor(Comparendo c)
    {
        if (c.Contraventor is not null)
        {
            return $"{c.Contraventor.Nombre} ({c.Contraventor.Documento})";
        }

        return c.PendienteContraventor ? "Pendiente" : null;
    }

    private static IQueryable<Comparendo> ApplySort(
        IQueryable<Comparendo> query,
        string sortBy,
        string sortDir)
    {
        var desc = sortDir.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy.ToLowerInvariant()) switch
        {
            "estado" => desc ? query.OrderByDescending(c => c.Estado) : query.OrderBy(c => c.Estado),
            "numerocomparendo" => desc
                ? query.OrderByDescending(c => c.NumeroComparendo)
                : query.OrderBy(c => c.NumeroComparendo),
            "placa" => desc ? query.OrderByDescending(c => c.Placa) : query.OrderBy(c => c.Placa),
            "total" => desc ? query.OrderByDescending(c => c.TotalValor) : query.OrderBy(c => c.TotalValor),
            _ => desc
                ? query.OrderByDescending(c => c.FechaComparendo)
                : query.OrderBy(c => c.FechaComparendo),
        };
    }
}
