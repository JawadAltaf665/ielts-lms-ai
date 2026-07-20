using System.Security.Claims;
using IELTSLms.Api.Constants;
using IELTSLms.Api.Contracts;
using Microsoft.IdentityModel.JsonWebTokens;
using IELTSLms.Api.Extensions;
using IELTSLms.Application.Identity;
using IELTSLms.Application.Identity.ForgotPassword;
using IELTSLms.Application.Identity.Login;
using IELTSLms.Application.Identity.Logout;
using IELTSLms.Application.Identity.Profile;
using IELTSLms.Application.Identity.Refresh;
using IELTSLms.Application.Identity.Register;
using IELTSLms.Application.Identity.ResendVerification;
using IELTSLms.Application.Identity.ResetPassword;
using IELTSLms.Application.Identity.Sessions;
using IELTSLms.Application.Identity.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IELTSLms.Api.Endpoints;

/// <summary>
/// Maps the identity/authentication endpoints under <c>/api/v1/auth</c> (Auth §, API Design
/// Standards §). Endpoints are thin: they translate HTTP to a MediatR command/query, then render the
/// <see cref="IELTSLms.SharedKernel.Results.Result"/> as either the success envelope or an RFC 7807
/// problem. The opaque refresh token is carried in an HttpOnly cookie, never in the JSON body.
/// </summary>
internal static class AuthEndpoints
{
    /// <summary>Name of the HttpOnly cookie carrying the opaque refresh token (Auth §3.3).</summary>
    private const string RefreshTokenCookie = "ielts_refresh_token";

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/auth")
            .WithTags("Authentication");

        group.MapPost("/register", RegisterAsync)
            .AllowAnonymous()
            .WithName("Register")
            .WithSummary("Registers a new account and sends an email-verification link.");

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Authenticates with email and password, returning an access token.");

        group.MapPost("/refresh", RefreshAsync)
            .AllowAnonymous()
            .WithName("RefreshToken")
            .WithSummary("Exchanges the refresh-token cookie for a new access/refresh token pair.");

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .WithSummary("Ends the current session and revokes its refresh-token family.");

        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .AllowAnonymous()
            .WithName("ForgotPassword")
            .WithSummary("Starts the password-reset flow (uniform response, no account enumeration).");

        group.MapPost("/reset-password", ResetPasswordAsync)
            .AllowAnonymous()
            .WithName("ResetPassword")
            .WithSummary("Completes a password reset and revokes all existing sessions.");

        group.MapPost("/verify-email", VerifyEmailAsync)
            .AllowAnonymous()
            .WithName("VerifyEmail")
            .WithSummary("Verifies an email address from a verification token.");

        group.MapPost("/resend-verification", ResendVerificationAsync)
            .AllowAnonymous()
            .WithName("ResendVerification")
            .WithSummary("Re-sends the email-verification link (uniform response).");

        group.MapGet("/me", GetCurrentUserAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithSummary("Returns the authenticated caller's profile.");

        group.MapGet("/sessions", ListSessionsAsync)
            .RequireAuthorization()
            .WithName("ListSessions")
            .WithSummary("Lists the caller's active device sessions.");

        group.MapDelete("/sessions/{id:guid}", RevokeSessionAsync)
            .RequireAuthorization()
            .WithName("RevokeSession")
            .WithSummary("Revokes one of the caller's device sessions.");

        return builder;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        SetNoStore(httpContext);
        var result = await sender.Send(
            new RegisterCommand(request.Email, request.Password, request.DisplayName), cancellationToken);

        return result.IsSuccess
            ? Results.Accepted(value: ApiResponse.Empty(httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        SetNoStore(httpContext);
        var result = await sender.Send(
            new LoginCommand(request.Email, request.Password, GetClientIp(httpContext), GetUserAgent(httpContext)),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        return WriteTokens(httpContext, result.Value);
    }

    private static async Task<IResult> RefreshAsync(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        SetNoStore(httpContext);

        if (!httpContext.Request.Cookies.TryGetValue(RefreshTokenCookie, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            return AuthErrors.InvalidToken.ToProblem(httpContext);
        }

        var result = await sender.Send(
            new RefreshTokenCommand(refreshToken, GetClientIp(httpContext), GetUserAgent(httpContext)),
            cancellationToken);

        if (result.IsFailure)
        {
            ClearRefreshCookie(httpContext);
            return result.ToProblem(httpContext);
        }

        return WriteTokens(httpContext, result.Value);
    }

    private static async Task<IResult> LogoutAsync(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        SetNoStore(httpContext);

        if (!TryGetUserId(httpContext, out var userId) || !TryGetSessionId(httpContext, out var sessionId))
        {
            return AuthErrors.InvalidCredentials.ToProblem(httpContext);
        }

        var result = await sender.Send(new LogoutCommand(userId, sessionId), cancellationToken);
        ClearRefreshCookie(httpContext);

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Empty(httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        SetNoStore(httpContext);
        var result = await sender.Send(new ForgotPasswordCommand(request.Email), cancellationToken);

        return result.IsSuccess
            ? Results.Accepted(value: ApiResponse.Empty(httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        SetNoStore(httpContext);
        var result = await sender.Send(
            new ResetPasswordCommand(request.Token, request.NewPassword), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToProblem(httpContext);
        }

        // A reset invalidates every session; drop any refresh cookie on this device too.
        ClearRefreshCookie(httpContext);
        return Results.Ok(ApiResponse.Empty(httpContext));
    }

    private static async Task<IResult> VerifyEmailAsync(
        VerifyEmailRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        SetNoStore(httpContext);
        var result = await sender.Send(new VerifyEmailCommand(request.Token), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(ApiResponse.Empty(httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> ResendVerificationAsync(
        ResendVerificationRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        SetNoStore(httpContext);
        var result = await sender.Send(new ResendVerificationCommand(request.Email), cancellationToken);

        return result.IsSuccess
            ? Results.Accepted(value: ApiResponse.Empty(httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> GetCurrentUserAsync(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        SetNoStore(httpContext);

        if (!TryGetUserId(httpContext, out var userId))
        {
            return AuthErrors.InvalidCredentials.ToProblem(httpContext);
        }

        var result = await sender.Send(new GetCurrentUserQuery(userId), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> ListSessionsAsync(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        SetNoStore(httpContext);

        if (!TryGetUserId(httpContext, out var userId))
        {
            return AuthErrors.InvalidCredentials.ToProblem(httpContext);
        }

        var result = await sender.Send(new ListSessionsQuery(userId), cancellationToken);

        return result.IsSuccess
            ? Results.Ok(ApiResponse.From(result.Value, httpContext))
            : result.ToProblem(httpContext);
    }

    private static async Task<IResult> RevokeSessionAsync(
        Guid id,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        SetNoStore(httpContext);

        if (!TryGetUserId(httpContext, out var userId))
        {
            return AuthErrors.InvalidCredentials.ToProblem(httpContext);
        }

        var result = await sender.Send(new RevokeSessionCommand(userId, id), cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : result.ToProblem(httpContext);
    }

    private static IResult WriteTokens(HttpContext httpContext, AuthTokensResult tokens)
    {
        SetRefreshCookie(httpContext, tokens.RefreshToken, tokens.RefreshTokenExpiresAtUtc);

        var body = new AccessTokenResponse(
            tokens.AccessToken,
            "Bearer",
            tokens.AccessTokenExpiresAtUtc,
            tokens.SessionId);

        return Results.Ok(ApiResponse.From(body, httpContext));
    }

    private static void SetRefreshCookie(HttpContext httpContext, string refreshToken, DateTimeOffset expiresAtUtc)
    {
        httpContext.Response.Cookies.Append(RefreshTokenCookie, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAtUtc,
            Path = "/api",
            IsEssential = true,
        });
    }

    private static void ClearRefreshCookie(HttpContext httpContext) =>
        httpContext.Response.Cookies.Delete(RefreshTokenCookie, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api",
        });

    private static void SetNoStore(HttpContext httpContext) =>
        httpContext.Response.Headers.CacheControl = "no-store";

    private static bool TryGetUserId(HttpContext httpContext, out Guid userId) =>
        Guid.TryParse(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub), out userId);

    private static bool TryGetSessionId(HttpContext httpContext, out Guid sessionId) =>
        Guid.TryParse(httpContext.User.FindFirstValue("sid"), out sessionId);

    private static string? GetClientIp(HttpContext httpContext) =>
        httpContext.Connection.RemoteIpAddress?.ToString();

    private static string? GetUserAgent(HttpContext httpContext) =>
        httpContext.Request.Headers.UserAgent.ToString() is { Length: > 0 } ua ? ua : null;
}
