using Microsoft.Extensions.Primitives;

namespace WhatToCook.Api.Infrastructure.Security;

/// <summary>
/// Blocks browser-initiated mutations to the internal-only API. Web calls the
/// API server-to-server; browser clients are not part of the API trust contract.
/// This is a defense-in-depth browser signal check, NOT authentication.
/// </summary>
public sealed class BrowserMutationGuardMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldReject(context.Request))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        await next(context);
    }

    internal static bool ShouldReject(HttpRequest request)
    {
        if (
            !request.Path.StartsWithSegments("/api")
            || HttpMethods.IsGet(request.Method)
            || HttpMethods.IsHead(request.Method)
            || HttpMethods.IsOptions(request.Method)
        )
        {
            return false;
        }

        // A browser POST/fetch normally sends Origin; this internal API has no
        // supported browser mutation surface, including same-origin requests.
        if (request.Headers.ContainsKey("Origin"))
        {
            return true;
        }

        StringValues site = request.Headers["Sec-Fetch-Site"];
        // Browsers can provide Fetch Metadata even when Origin is absent.
        // Missing headers remain allowed for the existing Web->API server calls.
        return !StringValues.IsNullOrEmpty(site)
            && !string.Equals(site.ToString(), "none", StringComparison.OrdinalIgnoreCase);
    }
}
