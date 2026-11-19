using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WhatToCook.Web.ComponentTests;

public sealed class WebDataProtectionTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "whattocook-dp-" + Guid.NewGuid().ToString("N")
    );
    private readonly RSA _certificateKey = RSA.Create(2048);
    private readonly string _keyRingPath;
    private readonly string _certificatePath;

    public WebDataProtectionTests()
    {
        _keyRingPath = CreateKeyRing("keys");
        _certificatePath = Path.Combine(_root, "web-data-protection.pem");
        File.WriteAllText(_certificatePath, CreatePem(_certificateKey, _certificateKey));
    }

    public void Dispose()
    {
        _certificateKey.Dispose();
        Directory.Delete(_root, recursive: true);
    }

    [Fact]
    public async Task TokenIssuedBeforeRecreation_IsAcceptedByRecreatedHost()
    {
        var configuration = ConfigurationFor(_keyRingPath, _certificatePath);
        var issued = IssueToken(configuration);

        using var recreatedHost = BuildHost(configuration);

        Assert.True(
            await IsAcceptedAsync(
                recreatedHost,
                issued.Cookie,
                issued.HeaderName,
                issued.RequestToken
            )
        );
    }

    [Fact]
    public async Task TokenIssuedBeforeRecreation_IsRejectedWhenKeyRingIsNotPersisted()
    {
        var issued = IssueToken(ConfigurationFor(_keyRingPath, _certificatePath));

        using var freshHost = BuildHost(
            ConfigurationFor(CreateKeyRing("fresh-keys"), _certificatePath)
        );

        Assert.False(
            await IsAcceptedAsync(freshHost, issued.Cookie, issued.HeaderName, issued.RequestToken)
        );
    }

    [Fact]
    public async Task TamperedRequestToken_IsRejectedAfterRecreation()
    {
        var configuration = ConfigurationFor(_keyRingPath, _certificatePath);
        var issued = IssueToken(configuration);

        using var recreatedHost = BuildHost(configuration);

        Assert.False(
            await IsAcceptedAsync(
                recreatedHost,
                issued.Cookie,
                issued.HeaderName,
                FlipMiddleCharacter(issued.RequestToken)
            )
        );
    }

    [Fact]
    public async Task MissingAntiforgeryCookie_IsRejectedAfterRecreation()
    {
        var configuration = ConfigurationFor(_keyRingPath, _certificatePath);
        var issued = IssueToken(configuration);

        using var recreatedHost = BuildHost(configuration);

        Assert.False(
            await IsAcceptedAsync(recreatedHost, null, issued.HeaderName, issued.RequestToken)
        );
    }

    [Fact]
    public async Task TokenIssuedUnderOneCertificate_IsRejectedUnderAnotherCertificate()
    {
        var issued = IssueToken(ConfigurationFor(_keyRingPath, _certificatePath));

        using var otherKey = RSA.Create(2048);
        var otherCertificatePath = Path.Combine(_root, "other-web-data-protection.pem");
        File.WriteAllText(otherCertificatePath, CreatePem(otherKey, otherKey));

        using var otherHost = BuildHost(ConfigurationFor(_keyRingPath, otherCertificatePath));

        Assert.False(
            await IsAcceptedAsync(otherHost, issued.Cookie, issued.HeaderName, issued.RequestToken)
        );
    }

    [Fact]
    public void KeyRingFiles_StoreKeysEncryptedToTheCertificate()
    {
        IssueToken(ConfigurationFor(_keyRingPath, _certificatePath));

        var keyFiles = Directory.GetFiles(_keyRingPath, "*.xml");
        Assert.NotEmpty(keyFiles);

        var privateKeyBase64 = Convert.ToBase64String(_certificateKey.ExportPkcs8PrivateKey());
        foreach (var keyFile in keyFiles)
        {
            var elementNames = XDocument
                .Load(keyFile)
                .Descendants()
                .Select(element => element.Name.LocalName)
                .ToHashSet(StringComparer.Ordinal);
            Assert.True(
                elementNames.Contains("encryptedSecret"),
                "Key ring file has no certificate-encrypted secret."
            );
            Assert.False(
                elementNames.Contains("masterKey"),
                "Key ring file contains a plaintext master key."
            );
            Assert.False(
                File.ReadAllText(keyFile).Contains(privateKeyBase64, StringComparison.Ordinal),
                "Key ring file contains the certificate private key."
            );
        }
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void PartialDataProtectionConfiguration_IsRejected(bool hasKeyRing, bool hasCertificate)
    {
        var configuration = ConfigurationFor(
            hasKeyRing ? _keyRingPath : null,
            hasCertificate ? _certificatePath : null
        );
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddWebDataProtection(configuration);
        });
    }

    [Fact]
    public void MissingKeyRingDirectory_IsRejected()
    {
        var configuration = ConfigurationFor(Path.Combine(_root, "missing-keys"), _certificatePath);
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddWebDataProtection(configuration);
        });
    }

    [Fact]
    public void CertificateWithoutPrivateKey_IsRejected()
    {
        var certificatePath = Path.Combine(_root, "public-only.pem");
        File.WriteAllText(
            certificatePath,
            PemEncoding.WriteString("CERTIFICATE", SelfSign(_certificateKey))
        );
        var configuration = ConfigurationFor(_keyRingPath, certificatePath);
        var services = new ServiceCollection();

        Assert.Throws<CryptographicException>(() =>
        {
            services.AddWebDataProtection(configuration);
        });
    }

    [Fact]
    public void CertificateWithNonRsaPrivateKey_IsRejected()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var certificatePath = Path.Combine(_root, "ecdsa.pem");
        var certificateDer = SelfSign(
            new CertificateRequest("CN=WhatToCook.Web test", ecdsa, HashAlgorithmName.SHA256)
        );
        File.WriteAllText(
            certificatePath,
            PemEncoding.WriteString("CERTIFICATE", certificateDer)
                + "\n"
                + PemEncoding.WriteString("PRIVATE KEY", ecdsa.ExportPkcs8PrivateKey())
        );
        var configuration = ConfigurationFor(_keyRingPath, certificatePath);
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() =>
        {
            services.AddWebDataProtection(configuration);
        });
    }

    [Fact]
    public void CertificateAndPrivateKeyMismatch_IsRejected()
    {
        using var otherKey = RSA.Create(2048);
        var certificatePath = Path.Combine(_root, "mismatched.pem");
        File.WriteAllText(certificatePath, CreatePem(_certificateKey, otherKey));
        var configuration = ConfigurationFor(_keyRingPath, certificatePath);
        var services = new ServiceCollection();

        // CreateFromPemFile rejects a certificate whose public key does not match its private key.
        Assert.Throws<CryptographicException>(() =>
        {
            services.AddWebDataProtection(configuration);
        });
    }

    private string CreateKeyRing(string name)
    {
        var path = Path.Combine(_root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private static IConfiguration ConfigurationFor(string? keyRingPath, string? certificatePath) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    [WebDataProtectionRegistration.KeyRingPathKey] = keyRingPath,
                    [WebDataProtectionRegistration.CertificatePathKey] = certificatePath,
                }
            )
            .Build();

    private static TestHost BuildHost(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAntiforgery();
        var certificate = services.AddWebDataProtection(configuration);
        return new TestHost(services.BuildServiceProvider(), certificate);
    }

    private static IssuedToken IssueToken(IConfiguration configuration)
    {
        using var host = BuildHost(configuration);
        var context = new DefaultHttpContext { RequestServices = host.Services };
        var tokens = host.Services.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);
        var setCookie = context.Response.Headers["Set-Cookie"].ToString();
        return new IssuedToken(setCookie.Split(';')[0], tokens.HeaderName!, tokens.RequestToken!);
    }

    private static Task<bool> IsAcceptedAsync(
        TestHost host,
        string? cookie,
        string headerName,
        string requestToken
    )
    {
        var context = new DefaultHttpContext { RequestServices = host.Services };
        context.Request.Method = HttpMethods.Post;
        if (cookie is not null)
        {
            context.Request.Headers["Cookie"] = cookie;
        }

        context.Request.Headers[headerName] = requestToken;
        return host.Services.GetRequiredService<IAntiforgery>().IsRequestValidAsync(context);
    }

    private static string FlipMiddleCharacter(string token)
    {
        var characters = token.ToCharArray();
        var index = characters.Length / 2;
        characters[index] = characters[index] == 'A' ? 'B' : 'A';
        return new string(characters);
    }

    private static byte[] SelfSign(RSA key) =>
        SelfSign(
            new CertificateRequest(
                "CN=WhatToCook.Web test",
                key,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1
            )
        );

    private static byte[] SelfSign(CertificateRequest request)
    {
        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(30)
        );
        return certificate.RawData;
    }

    private static string CreatePem(RSA certificateKey, RSA privateKey) =>
        PemEncoding.WriteString("CERTIFICATE", SelfSign(certificateKey))
        + "\n"
        + PemEncoding.WriteString("PRIVATE KEY", privateKey.ExportPkcs8PrivateKey());

    private sealed record IssuedToken(string Cookie, string HeaderName, string RequestToken);

    private sealed class TestHost : IDisposable
    {
        private readonly ServiceProvider _services;
        private readonly X509Certificate2? _certificate;

        public TestHost(ServiceProvider services, X509Certificate2? certificate)
        {
            _services = services;
            _certificate = certificate;
        }

        public IServiceProvider Services => _services;

        public void Dispose()
        {
            _services.Dispose();
            _certificate?.Dispose();
        }
    }
}
