#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Register and configure an Entra ID application for TimeTracker/ProjectEstimate
.DESCRIPTION
    This script creates an Entra ID application with all necessary configurations
    including SPA authentication, API scopes, permissions, and optional claims.
.NOTES
    Prerequisites:
    - Azure PowerShell module installed (Install-Module Az)
    - Appropriate permissions to create Entra ID applications
#>

param(
  [Parameter(Mandatory = $false)]
  [string]$DisplayName = "app-fakeintra-dev",
    
  [Parameter(Mandatory = $false)]
  [string]$HomePageUrl = "https://app-ipsagent-app-dev.azurewebsites.net/",
    
  [Parameter(Mandatory = $false)]
  [string]$ApplicationName = "FakeIntra",
    
  [Parameter(Mandatory = $false)]
  [string[]]$RedirectUris = @(
    "http://localhost:5173/",
    "http://localhost:5108/",
    "http://localhost:5108/swagger/oauth2-redirect.html",
    "https://app-ipsagent-app-dev.azurewebsites.net/",
    "http://127.0.0.1:33418",
    "https://vscode.dev/redirect"
  ),

  [Parameter(Mandatory = $false)]
  [string]$PermissionId = "6fba1db8-a24e-4788-abf4-030f0d72e37d"
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Import required modules
Import-Module Az.Resources

# Connect to Azure - ensures correct tenant/user context for all subsequent commands
Connect-AzAccount
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Entra ID Application Registration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get tenant information
Write-Host "Getting tenant information..." -ForegroundColor Yellow
$context = Get-AzContext
$tenantId = $context.Tenant.Id
$tenantName = (Get-AzTenant -TenantId $tenantId).Name
$userName = $context.Account.Id

Write-Host "Tenant ID: $tenantId" -ForegroundColor Green
Write-Host "Tenant Name: $tenantName" -ForegroundColor Green
Write-Host "Username: $userName" -ForegroundColor Green
Write-Host ""

# Step 1: Check if application exists, create if not
Write-Host "Step 1: Checking if application exists..." -ForegroundColor Yellow
$existingApp = Get-AzADApplication -DisplayName $DisplayName | Select-Object -First 1

if ($existingApp) {
  Write-Host "Application '$DisplayName' already exists!" -ForegroundColor Yellow
  $appObjectId = $existingApp.Id
  $appId = $existingApp.AppId
  Write-Host "  Object ID: $appObjectId" -ForegroundColor Green
  Write-Host "  App ID (Client ID): $appId" -ForegroundColor Green
  Write-Host "  Skipping creation, will proceed with configuration..." -ForegroundColor Yellow
}
else {
  Write-Host "Application not found. Creating new application..." -ForegroundColor Yellow
  $newApp = New-AzADApplication `
    -DisplayName $DisplayName `
    -EnableAccessTokenIssuance `
    -EnableIdTokenIssuance `
    -SignInAudience AzureADMyOrg `
    -WebHomePageUrl $HomePageUrl

  $appObjectId = $newApp.Id
  $appId = $newApp.AppId

  Write-Host "Application created successfully!" -ForegroundColor Green
  Write-Host "  Object ID: $appObjectId" -ForegroundColor Green
  Write-Host "  App ID (Client ID): $appId" -ForegroundColor Green
}
Write-Host ""

# Step 2: Configure SPA redirect URIs
Write-Host "Step 2: Configuring SPA redirect URIs..." -ForegroundColor Yellow
Update-AzADApplication -ObjectId $appObjectId -SPARedirectUri $RedirectUris
Write-Host "SPA redirect URIs configured successfully!" -ForegroundColor Green
Write-Host ""

# Step 3: Set identifier URI (App ID URI)
Write-Host "Step 3: Setting App ID URI..." -ForegroundColor Yellow
$appIdUri = "api://$appId"
Update-AzADApplication -ObjectId $appObjectId -IdentifierUris @($appIdUri)
Write-Host "App ID URI set to: $appIdUri" -ForegroundColor Green
Write-Host ""

# Step 4: Configure API scopes
Write-Host "Step 4: Configuring API scopes..." -ForegroundColor Yellow
$apiJson = Get-Content "api.json" -Raw
$api = [Microsoft.Azure.PowerShell.Cmdlets.Resources.MSGraph.Models.ApiV10.MicrosoftGraphApiApplication]::FromJsonString($apiJson)
$api.Oauth2PermissionScope[0].AdminConsentDisplayName = "$ApplicationName API Access"
$api.Oauth2PermissionScope[0].AdminConsentDescription = "Allow the application $ApplicationName to access the API on behalf of the signed-in user."
$api.Oauth2PermissionScope[0].Value = $ApplicationName
$api.Oauth2PermissionScope[0].Id = $PermissionId
Update-AzADApplication -ObjectId $appObjectId -Api $api
Write-Host "API scopes configured successfully!" -ForegroundColor Green
Write-Host ""

# Step 5: Add required resource access (permissions)
Write-Host "Step 5: Adding required resource access..." -ForegroundColor Yellow
$graphResourceAccessJson = Get-Content "graphResourceAccess.json" -Raw
$graphResourceAccess = [Microsoft.Azure.PowerShell.Cmdlets.Resources.MSGraph.Models.ApiV10.MicrosoftGraphRequiredResourceAccess]::FromJsonString($graphResourceAccessJson)
$apiResourceAccess = [Microsoft.Azure.PowerShell.Cmdlets.Resources.MSGraph.Models.ApiV10.MicrosoftGraphRequiredResourceAccess]::new()
$apiResourceAccess.ResourceAppId = $appId
$resourceAccess = [Microsoft.Azure.PowerShell.Cmdlets.Resources.MSGraph.Models.ApiV10.MicrosoftGraphResourceAccess]::new()
$resourceAccess.Id = $PermissionId
$resourceAccess.Type = "Scope"
$apiResourceAccess.ResourceAccess = @($resourceAccess)
Update-AzADApplication -ObjectId $appObjectId -RequiredResourceAccess @($graphResourceAccess, $apiResourceAccess)
Write-Host "Required resource access configured successfully!" -ForegroundColor Green
Write-Host ""

# Step 6: Configure optional claims
Write-Host "Step 6: Configuring optional claims..." -ForegroundColor Yellow
$optionalClaimsJson = Get-Content "optionalClaims.json" -Raw
$optionalClaims = [Microsoft.Azure.PowerShell.Cmdlets.Resources.MSGraph.Models.ApiV10.MicrosoftGraphOptionalClaims]::FromJsonString($optionalClaimsJson)
Update-AzADApplication -ObjectId $appObjectId -OptionalClaim $optionalClaims
Write-Host "Optional claims configured successfully!" -ForegroundColor Green
Write-Host ""

# Output configuration summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "CONFIGURATION COMPLETE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "IMPORTANT CONFIGURATION VALUES:" -ForegroundColor Yellow
Write-Host "================================" -ForegroundColor Yellow
Write-Host ""

Write-Host "Azure/Entra ID Configuration:" -ForegroundColor Cyan
Write-Host "  Tenant ID:                    $tenantId" -ForegroundColor White
Write-Host "  Client ID (Application ID):   $appId" -ForegroundColor White
Write-Host "  App Object ID:                $appObjectId" -ForegroundColor White
Write-Host "  App ID URI:                   $appIdUri" -ForegroundColor White
Write-Host ""

Write-Host "Authority & Endpoints:" -ForegroundColor Cyan
Write-Host "  Authority:                    https://login.microsoftonline.com/$tenantId" -ForegroundColor White
Write-Host "  Issuer:                       https://login.microsoftonline.com/$tenantId/v2.0" -ForegroundColor White
Write-Host "  Token Endpoint:               https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token" -ForegroundColor White
Write-Host "  Authorization Endpoint:       https://login.microsoftonline.com/$tenantId/oauth2/v2.0/authorize" -ForegroundColor White
Write-Host ""

Write-Host "Client Scopes to Request:" -ForegroundColor Cyan
Write-Host "  openid" -ForegroundColor White
Write-Host "  profile" -ForegroundColor White
Write-Host "  email" -ForegroundColor White
Write-Host "  offline_access" -ForegroundColor White
Write-Host "  User.Read" -ForegroundColor White
Write-Host "  $appIdUri/$ApplicationName" -ForegroundColor White
Write-Host ""

Write-Host "Optional Claims Configured:" -ForegroundColor Cyan
Write-Host "  - email" -ForegroundColor White
Write-Host "  - given_name" -ForegroundColor White
Write-Host "  - family_name" -ForegroundColor White
Write-Host ""

Write-Host "SPA Redirect URIs:" -ForegroundColor Cyan
foreach ($uri in $RedirectUris) {
  Write-Host "  - $uri" -ForegroundColor White
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "NEXT STEPS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "1. Grant Admin Consent:" -ForegroundColor Yellow
Write-Host "   Go to: https://login.microsoftonline.com/$tenantId/adminconsent?client_id=$appId" -ForegroundColor White
Write-Host "   Or use Azure Portal:" -ForegroundColor White
Write-Host "   - Navigate to: Entra ID > Enterprise Applications > $DisplayName" -ForegroundColor White
Write-Host "   - Click: Permissions > Grant admin consent for $tenantName" -ForegroundColor White
Write-Host ""

Write-Host "2. Configure API (appsettings.json):" -ForegroundColor Yellow
Write-Host @"
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "$tenantId",
    "ClientId": "$appId",
    "Audience": "$appIdUri",
    "ValidAudiences": [
      "$appIdUri",
      "$appId"
    ]
  }
}
"@ -ForegroundColor White
Write-Host ""

Write-Host "3. Configure SPA (React/TypeScript):" -ForegroundColor Yellow
Write-Host @"
{
  "auth": {
    "clientId": "$appId",
    "authority": "https://login.microsoftonline.com/$tenantId",
    "redirectUri": "http://localhost:5173/",
    "postLogoutRedirectUri": "http://localhost:5173/"
  },
  "scopes": {
    "api": ["$appIdUri/$ApplicationName"]
  }
}
"@ -ForegroundColor White
Write-Host ""

Write-Host "4. Install Required NuGet Packages for API:" -ForegroundColor Yellow
Write-Host "   dotnet add package Microsoft.Identity.Web" -ForegroundColor White
Write-Host "   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer" -ForegroundColor White
Write-Host ""

Write-Host "5. Install Required npm Packages for SPA:" -ForegroundColor Yellow
Write-Host "   npm install @azure/msal-browser @azure/msal-react" -ForegroundColor White
Write-Host ""

Write-Host "Registration completed successfully!" -ForegroundColor Green
Write-Host "All configuration values have been output above." -ForegroundColor Green
Write-Host ""
