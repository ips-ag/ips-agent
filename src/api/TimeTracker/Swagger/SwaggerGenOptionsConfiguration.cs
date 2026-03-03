using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TimeTracker.Swagger;

internal sealed class SwaggerGenOptionsConfiguration : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IConfiguration _configuration;

    public SwaggerGenOptionsConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Configure(SwaggerGenOptions options)
    {
        var instance = new Uri(_configuration["AzureAd:Instance"]!);
        string tenantId = _configuration["AzureAd:TenantId"]!;
        string audience = _configuration["AzureAd:Audience"]!;

        var authorizeUrl = new Uri(instance, $"{tenantId}/oauth2/v2.0/authorize");
        var tokenUrl = new Uri(instance, $"{tenantId}/oauth2/v2.0/token");
        var scope = $"{audience}/FakeIntra";

        options.AddSecurityDefinition(
            "oauth2",
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = authorizeUrl,
                        TokenUrl = tokenUrl,
                        Scopes = new Dictionary<string, string> { { scope, "Access FakeIntra API" } }
                    }
                }
            });

        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                    },
                    [scope]
                }
            });
    }
}
