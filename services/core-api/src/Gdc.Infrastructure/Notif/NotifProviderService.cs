using System.Text.Json;
using Gdc.Infrastructure.Persistence;
using DgcTenantContext = Gdc.Modules.Dgc.Application.Abstractions.ITenantContext;
using Gdc.Modules.Notif.Application.Abstractions;
using Gdc.Modules.Notif.Application.Provider;
using Gdc.Modules.Notif.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gdc.Infrastructure.Notif;

public sealed class NotifProviderService(
    GdcDbContext db,
    DgcTenantContext tenantContext,
    IEmailSenderFactory senderFactory,
    IEmailCredentialEncryptor encryptor,
    TimeProvider timeProvider)
{
    public async Task<ProviderResponse?> GetActiveProviderAsync(CancellationToken cancellationToken)
    {
        var config = await db.EmailProviderConfigs
            .Where(p => p.TenantId == tenantContext.TenantId && p.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        return config is null ? null : ToResponse(config);
    }

    public async Task<ProviderResponse> SaveProviderAsync(
        SaveProviderRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var credentialsJson = request.Credentials.GetRawText();
        var sender = senderFactory.Create(request.ProviderType, credentialsJson);
        await sender.ValidateAsync(cancellationToken);

        var encrypted = encryptor.Encrypt(credentialsJson);
        var now = timeProvider.GetUtcNow();
        var tenantId = tenantContext.TenantId;

        var activeConfigs = await db.EmailProviderConfigs
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var existing in activeConfigs)
        {
            existing.IsActive = false;
            existing.UpdatedAt = now;
        }

        var config = new EmailProviderConfig
        {
            Id = Guid.CreateVersion7(),
            TenantId = tenantId,
            ProviderType = request.ProviderType,
            FromAddress = request.FromAddress.Trim(),
            CredentialsEncrypted = encrypted,
            IsActive = true,
            DispatchEnabled = activeConfigs.FirstOrDefault()?.DispatchEnabled ?? true,
            CreatedAt = now,
            CreatedBy = tenantContext.UserId,
        };

        db.EmailProviderConfigs.Add(config);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(config);
    }

    public async Task<TestProviderResponse> TestProviderAsync(
        TestProviderRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Destino))
        {
            throw new ArgumentException("Destino email is required.", nameof(request));
        }

        string providerType;
        string fromAddress;
        string credentialsJson;

        if (request.Credentials is { } inlineCredentials
            && !string.IsNullOrWhiteSpace(request.ProviderType)
            && !string.IsNullOrWhiteSpace(request.FromAddress))
        {
            providerType = request.ProviderType;
            fromAddress = request.FromAddress.Trim();
            credentialsJson = inlineCredentials.GetRawText();
        }
        else
        {
            var active = await db.EmailProviderConfigs
                .Where(p => p.TenantId == tenantContext.TenantId && p.IsActive)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new InvalidOperationException("No active provider configuration found.");

            providerType = active.ProviderType;
            fromAddress = active.FromAddress;
            credentialsJson = encryptor.Decrypt(active.CredentialsEncrypted ?? string.Empty);
        }

        if (!ProviderTypes.IsSupported(providerType))
        {
            throw new ArgumentException($"Unsupported provider type: {providerType}");
        }

        var sender = senderFactory.Create(providerType, credentialsJson);
        var result = await sender.SendAsync(
            new EmailMessage(fromAddress, request.Destino.Trim(), "FLIT NOTIF — test", "<p>Test de conexión NOTIF</p>"),
            cancellationToken);

        return new TestProviderResponse(result.Success, result.ErrorMessage ?? "Test email sent.");
    }

    private static void ValidateRequest(SaveProviderRequest request)
    {
        if (!ProviderTypes.IsSupported(request.ProviderType))
        {
            throw new ArgumentException($"Unsupported provider type: {request.ProviderType}");
        }

        if (string.IsNullOrWhiteSpace(request.FromAddress))
        {
            throw new ArgumentException("FromAddress is required.");
        }
    }

    private static ProviderResponse ToResponse(EmailProviderConfig config) =>
        new(
            config.Id,
            config.ProviderType,
            config.FromAddress,
            config.IsActive,
            config.DispatchEnabled,
            !string.IsNullOrEmpty(config.CredentialsEncrypted),
            config.UpdatedAt ?? config.CreatedAt);
}
