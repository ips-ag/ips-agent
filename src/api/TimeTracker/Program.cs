using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using ModelContextProtocol.AspNetCore.Authentication;
using OpenTelemetry.Trace;
using TimeTracker.Application;
using TimeTracker.Application.Common.Interfaces;
using TimeTracker.Infrastructure;
using TimeTracker.Infrastructure.Services;
using TimeTracker.Mcp;
using TimeTracker.Middleware;
using TimeTracker.Swagger;
using TimeTracker.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureOpenTelemetryTracerProvider((_, trace) => trace.AddProcessor(new HealthRequestFilter()));
builder.Services.AddOpenTelemetry().UseAzureMonitor();
builder.Services.ConfigureOptions<McpAuthenticationOptionsConfiguration>();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = McpAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddMcp()
    .AddMicrosoftIdentityWebApi(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureOptions<SwaggerGenOptionsConfiguration>();
builder.Services.AddSwaggerGen();

// MCP server
builder.Services.AddMcpServer().AddAuthorizationFilters().WithHttpTransport().WithToolsFromAssembly();

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
