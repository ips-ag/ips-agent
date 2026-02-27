using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using ModelContextProtocol.AspNetCore.Authentication;
using ModelContextProtocol.Authentication;
using OpenTelemetry.Trace;
using TimeTracker.Application;
using TimeTracker.Application.Common.Interfaces;
using TimeTracker.Infrastructure;
using TimeTracker.Infrastructure.Services;
using TimeTracker.Middleware;
using TimeTracker.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureOpenTelemetryTracerProvider((_, trace) => trace.AddProcessor(new HealthRequestFilter()));
builder.Services.AddOpenTelemetry().UseAzureMonitor();

string entraInstance = builder.Configuration["AzureAd:Instance"]!.TrimEnd('/');
string entraTenantId = builder.Configuration["AzureAd:TenantId"]!;
string entraAudience = builder.Configuration["AzureAd:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddMcp(options =>
{
    options.ResourceMetadata = new ProtectedResourceMetadata
    {
        AuthorizationServers = { $"{entraInstance}/{entraTenantId}/v2.0" },
        ScopesSupported = [$"api://{entraAudience}/FakeIntra"]
    };
}).AddMicrosoftIdentityWebApi(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "oauth2",
        new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl =
                        new Uri(
                            $"{builder.Configuration["AzureAd:Instance"]}{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),
                    TokenUrl =
                        new Uri(
                            $"{builder.Configuration["AzureAd:Instance"]}{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { $"{builder.Configuration["AzureAd:Audience"]}/FakeIntra", "Access FakeIntra API" }
                    }
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
                [$"{builder.Configuration["AzureAd:Audience"]}/FakeIntra"]
            }
        });
});

// MCP server
builder.Services
    .AddMcpServer()
    .WithHttpTransport()
    .WithToolsFromAssembly();

// Health checks
builder.Services.AddHealthChecks();

// CORS for React dev server
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
        options.OAuthUsePkce();
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserSyncMiddleware>();
app.MapHealthChecks("/health").AllowAnonymous();
app.MapGet("/", () => Results.Ok()).AllowAnonymous();
app.MapGet("/robots933456.txt", () => Results.Ok()).AllowAnonymous();
app.MapControllers();
app.MapMcp("/mcp").RequireAuthorization();

app.Run();
