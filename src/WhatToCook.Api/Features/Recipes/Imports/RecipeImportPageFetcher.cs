using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WhatToCook.Api.Features.Recipes.Imports;

public interface IRecipeImportPageFetcher
{
    Task<string> FetchAsync(Uri uri, CancellationToken cancellationToken);
}

public sealed class HttpRecipeImportPageFetcher(
    IHttpClientFactory httpClientFactory,
    IHostEnvironment environment
) : IRecipeImportPageFetcher
{
    private const int MaxResponseBytes = 1_000_000;

    public async Task<string> FetchAsync(Uri uri, CancellationToken cancellationToken)
    {
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException("Only HTTP(S) imports are supported.");
        }

        if (!IsDevelopmentFixture(uri))
        {
            await EnsurePublicHostAsync(uri.Host, cancellationToken);
        }

        var client = httpClientFactory.CreateClient("recipe-import");
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        using var response = await client.GetAsync(
            uri,
            HttpCompletionOption.ResponseHeadersRead,
            timeout.Token
        );
        response.EnsureSuccessStatusCode();

        if (
            response.Content.Headers.ContentLength is > MaxResponseBytes
            || (
                response.Content.Headers.ContentType?.MediaType is { } mediaType
                && !mediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase)
                && !mediaType.Equals("application/xhtml+xml", StringComparison.OrdinalIgnoreCase)
            )
        )
        {
            throw new InvalidOperationException("The imported response is too large or not HTML.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true
        );
        var builder = new StringBuilder();
        var buffer = new char[8192];
        var bytesRead = 0;
        int read;
        while ((read = await reader.ReadAsync(buffer, timeout.Token)) > 0)
        {
            bytesRead += Encoding.UTF8.GetByteCount(buffer, 0, read);
            if (bytesRead > MaxResponseBytes)
            {
                throw new InvalidOperationException("The imported response is too large.");
            }

            builder.Append(buffer, 0, read);
        }

        return builder.ToString();
    }

    private bool IsDevelopmentFixture(Uri uri) =>
        environment.IsDevelopment()
        && uri.IsLoopback
        && uri.AbsolutePath.StartsWith("/import-fixtures/recipe-import/", StringComparison.Ordinal);

    private static async Task EnsurePublicHostAsync(
        string host,
        CancellationToken cancellationToken
    )
    {
        if (IPAddress.TryParse(host, out var address))
        {
            EnsurePublicAddress(address);
            return;
        }

        var addresses = await Dns.GetHostAddressesAsync(host, cancellationToken);
        if (addresses.Length == 0)
        {
            throw new InvalidOperationException("The import host could not be resolved.");
        }

        foreach (var resolvedAddress in addresses)
        {
            EnsurePublicAddress(resolvedAddress);
        }
    }

    private static void EnsurePublicAddress(IPAddress address)
    {
        if (
            IPAddress.IsLoopback(address)
            || address.IsIPv4MappedToIPv6 && IPAddress.IsLoopback(address.MapToIPv4())
            || address.AddressFamily == AddressFamily.InterNetwork
                && (
                    address.GetAddressBytes()[0] == 10
                    || address.GetAddressBytes()[0] == 127
                    || address.GetAddressBytes()[0] == 169 && address.GetAddressBytes()[1] == 254
                    || address.GetAddressBytes()[0] == 192 && address.GetAddressBytes()[1] == 168
                    || address.GetAddressBytes()[0] == 172
                        && address.GetAddressBytes()[1] is >= 16 and <= 31
                )
            || address.AddressFamily == AddressFamily.InterNetworkV6
                && (address.IsIPv6LinkLocal || address.IsIPv6SiteLocal || address.IsIPv6UniqueLocal)
        )
        {
            throw new InvalidOperationException("Private and local import hosts are not allowed.");
        }
    }
}
