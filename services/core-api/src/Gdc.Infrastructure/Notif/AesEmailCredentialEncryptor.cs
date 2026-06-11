using System.Security.Cryptography;
using System.Text;
using Gdc.Modules.Notif.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Gdc.Infrastructure.Notif;

public sealed class AesEmailCredentialEncryptor(IOptions<NotifEncryptionOptions> options) : IEmailCredentialEncryptor
{
    public string Encrypt(string plainText)
    {
        var key = GetKeyBytes();
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(key, 16);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        var payload = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, payload, nonce.Length + tag.Length, cipherBytes.Length);
        return Convert.ToBase64String(payload);
    }

    public string Decrypt(string cipherText)
    {
        var key = GetKeyBytes();
        var payload = Convert.FromBase64String(cipherText);
        var nonce = payload.AsSpan(0, 12);
        var tag = payload.AsSpan(12, 16);
        var cipherBytes = payload.AsSpan(28);

        var plainBytes = new byte[cipherBytes.Length];
        using var aes = new AesGcm(key, 16);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }

    private byte[] GetKeyBytes()
    {
        var keyMaterial = options.Value.Key;
        if (string.IsNullOrWhiteSpace(keyMaterial))
        {
            throw new InvalidOperationException("Notif:Encryption:Key is not configured.");
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes(keyMaterial));
    }
}
