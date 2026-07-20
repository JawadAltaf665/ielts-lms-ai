using IELTSLms.Application.Identity;
using IELTSLms.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace IELTSLms.Api.Authentication;

/// <summary>
/// Wires JWT bearer authentication and authorization at the composition root (Auth §3.2, §9;
/// Security Standards §5.1). Tokens are validated against the same asymmetric key used to sign them
/// (via <see cref="JwtSigningKeyProvider"/>), with issuer, audience, lifetime, and — critically —
/// the signing algorithm pinned so <c>alg:none</c> and algorithm-substitution attacks are rejected.
/// </summary>
internal static class AuthenticationRegistration
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // AuthOptions lives in Application; bind it here where configuration is available.
        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Configure the bearer options from the shared signing key + JwtOptions, so signing and
        // validation can never diverge. A typed configurator keeps DI clean (no per-request mutation).
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Builds the JWT bearer validation parameters from the resolved <see cref="JwtOptions"/> and the
    /// shared <see cref="JwtSigningKeyProvider"/>. Registered as a singleton configurator so the
    /// parameters are constructed once with the real key material.
    /// </summary>
    private sealed class ConfigureJwtBearerOptions(
        JwtSigningKeyProvider keyProvider,
        IOptions<JwtOptions> jwtOptions)
        : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly JwtOptions _jwt = jwtOptions.Value;

        public void Configure(string? name, JwtBearerOptions options)
        {
            if (name is not JwtBearerDefaults.AuthenticationScheme)
            {
                return;
            }

            Configure(options);
        }

        public void Configure(JwtBearerOptions options)
        {
            // Keep the token's own claim types (sub, sid, tenantId) so downstream code reads exactly
            // what was signed, and treat "sub" as the name/identity claim.
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwt.Audience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true,
                RequireSignedTokens = true,
                IssuerSigningKey = keyProvider.ValidationKey,
                // Pin the exact algorithm so alg:none and algorithm substitution are rejected (§5.1).
                ValidAlgorithms = [keyProvider.Algorithm],
                ClockSkew = _jwt.ClockSkew,
                NameClaimType = JwtRegisteredClaimNames.Sub,
            };
        }
    }
}
