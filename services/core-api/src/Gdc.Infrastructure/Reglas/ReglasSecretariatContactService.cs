using Gdc.Infrastructure.Persistence;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Reglas.Application.Secretariat;
using Gdc.Modules.Reglas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Reglas;

public sealed class ReglasSecretariatContactService(
    GdcDbContext db,
    DgcTenantContext tenantContext,
    ReglasAccessService access,
    TimeProvider timeProvider)
{
    public async Task<ReglasSecretariatContactListResponse> ListAsync(CancellationToken cancellationToken)
    {
        access.EnsureCanRead();

        var items = await db.SecretariatContacts
            .Where(c => c.TenantId == tenantContext.TenantId && c.DeletedAt == null)
            .OrderBy(c => c.SecretariatName)
            .Select(c => ToResponse(c))
            .ToListAsync(cancellationToken);

        return new ReglasSecretariatContactListResponse(items);
    }

    public async Task<ReglasSecretariatContactResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        access.EnsureCanRead();

        var contact = await db.SecretariatContacts
            .FirstOrDefaultAsync(
                c => c.Id == id && c.TenantId == tenantContext.TenantId && c.DeletedAt == null,
                cancellationToken);

        return contact is null ? null : ToResponse(contact);
    }

    public async Task<ReglasSecretariatContactResponse> CreateAsync(
        CreateReglasSecretariatContactRequest request,
        CancellationToken cancellationToken)
    {
        access.EnsureCanManage();
        await EnsureUniqueCodeAsync(request.SecretariatCode.Trim(), excludeId: null, cancellationToken);

        var now = timeProvider.GetUtcNow();
        var contact = new SecretariatContact
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantContext.TenantId,
            SecretariatCode = request.SecretariatCode.Trim(),
            SecretariatName = request.SecretariatName.Trim(),
            ContactName = request.ContactName.Trim(),
            ContactEmail = request.ContactEmail.Trim(),
            ContactPhone = NormalizeOptional(request.ContactPhone),
            IsActive = request.IsActive,
            CreatedAt = now,
            CreatedBy = tenantContext.UserId,
        };

        db.SecretariatContacts.Add(contact);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(contact);
    }

    public async Task<ReglasSecretariatContactResponse?> UpdateAsync(
        Guid id,
        UpdateReglasSecretariatContactRequest request,
        CancellationToken cancellationToken)
    {
        access.EnsureCanManage();

        var contact = await db.SecretariatContacts
            .FirstOrDefaultAsync(
                c => c.Id == id && c.TenantId == tenantContext.TenantId && c.DeletedAt == null,
                cancellationToken);

        if (contact is null)
        {
            return null;
        }

        await EnsureUniqueCodeAsync(request.SecretariatCode.Trim(), excludeId: id, cancellationToken);

        var now = timeProvider.GetUtcNow();
        contact.SecretariatCode = request.SecretariatCode.Trim();
        contact.SecretariatName = request.SecretariatName.Trim();
        contact.ContactName = request.ContactName.Trim();
        contact.ContactEmail = request.ContactEmail.Trim();
        contact.ContactPhone = NormalizeOptional(request.ContactPhone);
        contact.IsActive = request.IsActive;
        contact.UpdatedAt = now;
        contact.UpdatedBy = tenantContext.UserId;

        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(contact);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        access.EnsureCanManage();

        var contact = await db.SecretariatContacts
            .FirstOrDefaultAsync(
                c => c.Id == id && c.TenantId == tenantContext.TenantId && c.DeletedAt == null,
                cancellationToken);

        if (contact is null)
        {
            return false;
        }

        var inUse = await db.DynamicRules
            .AnyAsync(
                r => r.SecretariatContactId == id
                     && r.TenantId == tenantContext.TenantId
                     && r.DeletedAt == null,
                cancellationToken);

        if (inUse)
        {
            throw new ArgumentException("Contact is referenced by one or more dynamic rules.");
        }

        var now = timeProvider.GetUtcNow();
        contact.DeletedAt = now;
        contact.DeletedBy = tenantContext.UserId;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureUniqueCodeAsync(
        string code,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var exists = await db.SecretariatContacts
            .AnyAsync(
                c => c.TenantId == tenantContext.TenantId
                     && c.SecretariatCode == code
                     && c.DeletedAt == null
                     && (excludeId == null || c.Id != excludeId),
                cancellationToken);

        if (exists)
        {
            throw new ArgumentException("A contact already exists for this secretariat code.");
        }
    }

    private static ReglasSecretariatContactResponse ToResponse(SecretariatContact contact) =>
        new(
            contact.Id,
            contact.SecretariatCode,
            contact.SecretariatName,
            contact.ContactName,
            contact.ContactEmail,
            contact.ContactPhone,
            contact.IsActive,
            contact.CreatedAt,
            contact.UpdatedAt);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
