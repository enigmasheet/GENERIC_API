using Microsoft.AspNetCore.Http;

namespace ModularMonolith.Infrastructure.Security;

public sealed class SecureHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecureHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Cross-Origin-Resource-Policy"] = "same-site";

        await _next(context);
    }
}
