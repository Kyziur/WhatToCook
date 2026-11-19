using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace WhatToCook.Web;

// Durable Data Protection keys for the Web host. Keys persist to a key ring directory and are
// encrypted to a certificate whose unencrypted RSA private key is supplied in the same PEM file.
// Missing paths and unusable certificates are rejected during host construction.
internal static class WebDataProtectionRegistration
{
    internal const string KeyRingPathKey = "DataProtection:KeyRingPath";
    internal const string CertificatePathKey = "DataProtection:CertificatePath";
    private const string ApplicationName = "WhatToCook.Web";

    internal static X509Certificate2? AddWebDataProtection(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var keyRingPath = configuration[KeyRingPathKey];
        var certificatePath = configuration[CertificatePathKey];

        if (string.IsNullOrWhiteSpace(keyRingPath) && string.IsNullOrWhiteSpace(certificatePath))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(keyRingPath) || string.IsNullOrWhiteSpace(certificatePath))
        {
            throw new InvalidOperationException(
                $"{KeyRingPathKey} and {CertificatePathKey} must be configured together."
            );
        }

        // Require the key ring directory prepared by the deployment.
        if (!Directory.Exists(keyRingPath))
        {
            throw new InvalidOperationException(
                $"Data Protection key ring directory '{keyRingPath}' does not exist."
            );
        }

        // CreateFromPemFile rejects a certificate whose public key does not match its private key.
        var certificate = X509Certificate2.CreateFromPemFile(certificatePath);
        if (!HasRsaPrivateKey(certificate))
        {
            certificate.Dispose();
            throw new InvalidOperationException(
                $"Data Protection certificate '{certificatePath}' must contain an unencrypted RSA private key."
            );
        }

        services
            .AddDataProtection()
            .SetApplicationName(ApplicationName)
            .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
            .ProtectKeysWithCertificate(certificate);

        // The caller owns the returned certificate and must dispose it when the host stops.
        return certificate;
    }

    internal static void VerifyExistingKeys(IServiceProvider services)
    {
        const string failure =
            "The existing Data Protection key ring cannot be decrypted with the configured certificate. Restore the matching certificate and key ring.";

        // Loading individual keys does not let Data Protection generate a replacement key.
        foreach (var key in services.GetRequiredService<IKeyManager>().GetAllKeys())
        {
            if (key.IsRevoked)
            {
                continue;
            }

            try
            {
                if (key.CreateEncryptor() is null)
                {
                    throw new InvalidOperationException(failure);
                }
            }
            catch (CryptographicException)
            {
                throw new InvalidOperationException(failure);
            }
        }
    }

    private static bool HasRsaPrivateKey(X509Certificate2 certificate)
    {
        using var rsaPrivateKey = certificate.GetRSAPrivateKey();
        return rsaPrivateKey is not null;
    }
}
