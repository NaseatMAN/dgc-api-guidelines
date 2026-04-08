using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using DGC.Sample.Domain.Constants.ApiErrorConstants;
using DGC.Sample.Domain.Exceptions.Errors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DGC.Sample.Api.Extensions;

public static class ApiAuthenticationExtension
{
    public const string OidcJwtBearerScheme = "oidc-bearer";

    public static IServiceCollection ConfigureApiAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<OidcAuthenticationOptions>()
            .Bind(configuration.GetSection(OidcAuthenticationOptions.SectionName));

        services
            .AddOptions<ApiKeyAuthenticationOptions>()
            .Bind(configuration.GetSection(ApiKeyAuthenticationOptions.SectionName));

        var oidcOptions = configuration.GetSection(OidcAuthenticationOptions.SectionName).Get<OidcAuthenticationOptions>()
            ?? new OidcAuthenticationOptions();

        services
            .AddAuthentication()
            .AddJwtBearer(OidcJwtBearerScheme, options => ConfigureJwtBearer(options, oidcOptions));

        services.AddAuthorization();

        return services;
    }

    private static void ConfigureJwtBearer(JwtBearerOptions options, OidcAuthenticationOptions oidcOptions)
    {
        options.RequireHttpsMetadata = oidcOptions.RequireHttpsMetadata;
        options.Authority = oidcOptions.Authority;
        options.Audience = oidcOptions.Audience;
        options.MapInboundClaims = false;

        if (!string.IsNullOrWhiteSpace(oidcOptions.MetadataAddress))
        {
            options.MetadataAddress = oidcOptions.MetadataAddress;
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier,
            ValidateAudience = true,
            ValidAudience = oidcOptions.Audience,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        if (oidcOptions.ValidIssuers.Count > 0)
        {
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidIssuers = oidcOptions.ValidIssuers;
        }

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                var error = new AzureError(
                    Code: context.AuthenticateFailure is null
                        ? UnauthorizedErrorCode.AuthenticationRequired
                        : UnauthorizedErrorCode.InvalidCredentials,
                    Message: context.AuthenticateFailure is null
                        ? "Authentication is required for this endpoint."
                        : "The supplied bearer token could not be validated.",
                    InnerError: new AzureInnerError(TraceId: context.HttpContext.TraceIdentifier));

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                context.Response.Headers["x-ms-error-code"] = error.Code;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new AzureErrorResponse(error)));
            }
        };
    }

    public sealed class OidcAuthenticationOptions
    {
        public const string SectionName = "Authentication:Oidc";

        public string Authority { get; init; } = string.Empty;

        public string MetadataAddress { get; init; } = string.Empty;

        public string Audience { get; init; } = string.Empty;

        public bool RequireHttpsMetadata { get; init; } = true;

        public List<string> ValidIssuers { get; init; } = new();
    }

    public sealed class ApiKeyAuthenticationOptions
    {
        public const string SectionName = "Authentication:ApiKey";

        [Required]
        public string HeaderName { get; init; } = "X-Api-Key";

        public Dictionary<string, string[]> EndpointKeys { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    }
}