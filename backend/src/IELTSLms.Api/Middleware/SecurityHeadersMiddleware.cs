namespace IELTSLms.Api.Middleware;

/// <summary>
/// Adds the baseline security response headers (Security Standards §12) to every response:
/// clickjacking, MIME-sniffing, referrer, and feature-policy protections, plus HSTS over HTTPS.
/// A conservative API-only Content-Security-Policy denies framing and inline content — the API
/// serves JSON, not HTML, so it needs no script/style sources. Cache-Control for authenticated
/// responses is set on the auth endpoints themselves (<c>no-store</c>).
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        // Set the headers just before the response starts, so they apply even to short-circuited responses.
        context.Response.OnStarting(static state =>
        {
            var ctx = (HttpContext)state;
            var h = ctx.Response.Headers;

            h["X-Content-Type-Options"] = "nosniff";
            h["X-Frame-Options"] = "DENY";
            h["Referrer-Policy"] = "strict-origin-when-cross-origin";
            h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";
            h["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";

            if (ctx.Request.IsHttps)
            {
                h["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
            }

            return Task.CompletedTask;
        }, context);

        return next(context);
    }
}
