using Microsoft.Extensions.Options;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;

namespace TimeTracker.Mcp;

internal sealed class McpAuthenticationOptionsConfiguration : IPostConfigureOptions<McpAuthenticationOptions>
{
    private readonly IConfiguration _configuration;

    public McpAuthenticationOptionsConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void PostConfigure(string? name, McpAuthenticationOptions options)
    {
        var instance = new Uri(_configuration["AzureAd:Instance"]!);
        string tenantId = _configuration["AzureAd:TenantId"]!;
        string clientId = _configuration["AzureAd:ClientId"]!;

        options.ResourceMetadata = new ProtectedResourceMetadata
        {
            AuthorizationServers = { new Uri(instance, $"{tenantId}/v2.0").ToString() },
            ScopesSupported = [$"api://{clientId}/FakeIntra"]
        };
    }
}
