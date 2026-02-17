#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Register and configure an Entra ID application for TimeTracker/ProjectEstimate
.DESCRIPTION
    This script creates an Entra ID application with all necessary configurations
    including SPA authentication, API scopes, permissions, and optional claims.
.NOTES
    Prerequisites:
    - Azure CLI installed and logged in (az login)
    - Appropriate permissions to create Entra ID applications
#>

param(
  [Parameter(Mandatory = $false)]
  [string]$DisplayName = "app-fakeintra-dev",
    
  [Parameter(Mandatory = $false)]
  [string]$HomePageUrl = "https://app-fakeintra-ui-dev.azurewebsites.net/",
    
  [Parameter(Mandatory = $false)]
  [string]$ApplicationName = "FakeIntra",
    
  [Parameter(Mandatory = $false)]
  [string[]]$RedirectUris = @(
    "https://app-fakeintra-ui-dev.azurewebsites.net/",
    "http://localhost:5173/",
    "http://localhost:5000/",
    "http://localhost:5000/swagger/oauth2-redirect.html"
  )
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Import required modules
Import-Module Az.Resources

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Entra ID Application Registration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get tenant information
Write-Host "Getting tenant information..." -ForegroundColor Yellow
$tenantInfo = az account show | ConvertFrom-Json
$tenantId = $tenantInfo.tenantId
$tenantName = $tenantInfo.name

Write-Host "Tenant ID: $tenantId" -ForegroundColor Green
Write-Host "Tenant Name: $tenantName" -ForegroundColor Green
Write-Host ""

# Step 1: Check if application exists, create if not
Write-Host "Step 1: Checking if application exists..." -ForegroundColor Yellow
$existingApp = az ad app list --display-name $DisplayName --query "[0]" | ConvertFrom-Json

if ($existingApp) {
  Write-Host "Application '$DisplayName' already exists!" -ForegroundColor Yellow
  $appObjectId = $existingApp.id
  $appId = $existingApp.appId
  Write-Host "  Object ID: $appObjectId" -ForegroundColor Green
  Write-Host "  App ID (Client ID): $appId" -ForegroundColor Green
  Write-Host "  Skipping creation, will proceed with configuration..." -ForegroundColor Yellow
}
else {
  Write-Host "Application not found. Creating new application..." -ForegroundColor Yellow
  $appJson = az ad app create `
    --display-name $DisplayName `
    --enable-access-token-issuance true `
    --enable-id-token-issuance true `
    --sign-in-audience AzureADMyOrg `
    --web-home-page-url $HomePageUrl | ConvertFrom-Json

  $appObjectId = $appJson.id
  $appId = $appJson.appId

  Write-Host "Application created successfully!" -ForegroundColor Green
  Write-Host "  Object ID: $appObjectId" -ForegroundColor Green
  Write-Host "  App ID (Client ID): $appId" -ForegroundColor Green
}
Write-Host ""

# # Step 2: Configure SPA redirect URIs
# Write-Host "Step 2: Configuring SPA redirect URIs..." -ForegroundColor Yellow
# $spaConfig = Get-Content "spa.json" | ConvertFrom-Json
# $spaConfig.redirectUris = $RedirectUris
# $spaJsonString = $spaConfig | ConvertTo-Json -Depth 10 -Compress
# az ad app update --id $appObjectId --set "spa=$spaJsonString" --debug
# Write-Host "SPA redirect URIs configured successfully!" -ForegroundColor Green
# Write-Host ""

# # Step 3: Set identifier URI (App ID URI)
# Write-Host "Step 3: Setting App ID URI..." -ForegroundColor Yellow
# $appIdUri = "api://$appId"
# az ad app update --id $appObjectId --identifier-uris $appIdUri
# Write-Host "App ID URI set to: $appIdUri" -ForegroundColor Green
# Write-Host ""

# # Step 4: Configure API scopes
# Write-Host "Step 4: Configuring API scopes..." -ForegroundColor Yellow
# $apiJson = Get-Content "api.json" -Raw
# $apiJson = $apiJson -replace '\{\{ApplicationName\}\}', $ApplicationName
# az ad app update --id $appObjectId --set "api=$apiJson" --debug
# Write-Host "API scopes configured successfully!" -ForegroundColor Green
# Write-Host ""

# # Step 5: Add required resource access (permissions)
# Write-Host "Step 5: Adding required resource access..." -ForegroundColor Yellow
# $requiredResourceAccess = Get-Content "requiredResourceAccess.json" | ConvertFrom-Json
# $requiredResourceAccess[0].resourceAppId = $appId
# $requiredResourceAccessJson = $requiredResourceAccess | ConvertTo-Json -Depth 10 -Compress
# az ad app update --id $appObjectId --set "requiredResourceAccess=$requiredResourceAccessJson" --debug
# Write-Host "Required resource access configured successfully!" -ForegroundColor Green
# Write-Host ""

# Step 6: Configure optional claims
Write-Host "Step 6: Configuring optional claims..." -ForegroundColor Yellow
$optionalClaimsJson = Get-Content "optionalClaims.json" -Raw
$optionalClaims = [Microsoft.Azure.PowerShell.Cmdlets.Resources.MSGraph.Models.ApiV10.MicrosoftGraphOptionalClaims]::FromJsonString($optionalClaimsJson)
Update-AzADApplication -ObjectId $appObjectId -OptionalClaim $optionalClaims
Write-Host "Optional claims configured successfully!" -ForegroundColor Green
Write-Host ""


# # Output configuration summary
# Write-Host "========================================" -ForegroundColor Cyan
# Write-Host "CONFIGURATION COMPLETE" -ForegroundColor Cyan
# Write-Host "========================================" -ForegroundColor Cyan
# Write-Host ""

# Write-Host "IMPORTANT CONFIGURATION VALUES:" -ForegroundColor Yellow
# Write-Host "================================" -ForegroundColor Yellow
# Write-Host ""

# Write-Host "Azure/Entra ID Configuration:" -ForegroundColor Cyan
# Write-Host "  Tenant ID:                    $tenantId" -ForegroundColor White
# Write-Host "  Client ID (Application ID):   $appId" -ForegroundColor White
# Write-Host "  App Object ID:                $appObjectId" -ForegroundColor White
# Write-Host "  App ID URI:                   $appIdUri" -ForegroundColor White
# Write-Host ""

# Write-Host "Authority & Endpoints:" -ForegroundColor Cyan
# Write-Host "  Authority:                    https://login.microsoftonline.com/$tenantId" -ForegroundColor White
# Write-Host "  Issuer:                       https://login.microsoftonline.com/$tenantId/v2.0" -ForegroundColor White
# Write-Host "  Token Endpoint:               https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token" -ForegroundColor White
# Write-Host "  Authorization Endpoint:       https://login.microsoftonline.com/$tenantId/oauth2/v2.0/authorize" -ForegroundColor White
# Write-Host ""

# Write-Host "API Configuration:" -ForegroundColor Cyan
# Write-Host "  API Scope:                    $appIdUri/$ApplicationName" -ForegroundColor White
# Write-Host "  API Scope (Full):             api://$appId/$ApplicationName" -ForegroundColor White
# Write-Host ""

# Write-Host "Client Scopes to Request:" -ForegroundColor Cyan
# Write-Host "  openid" -ForegroundColor White
# Write-Host "  profile" -ForegroundColor White
# Write-Host "  email" -ForegroundColor White
# Write-Host "  $appIdUri/$ApplicationName" -ForegroundColor White
# Write-Host ""

# Write-Host "Optional Claims Configured:" -ForegroundColor Cyan
# Write-Host "  - email" -ForegroundColor White
# Write-Host "  - given_name" -ForegroundColor White
# Write-Host "  - family_name" -ForegroundColor White
# Write-Host ""

# Write-Host "SPA Redirect URIs:" -ForegroundColor Cyan
# $spaConfig = Get-Content "spa.json" | ConvertFrom-Json
# foreach ($uri in $spaConfig.redirectUris) {
#   Write-Host "  - $uri" -ForegroundColor White
# }
# Write-Host ""

# Write-Host "========================================" -ForegroundColor Cyan
# Write-Host "NEXT STEPS" -ForegroundColor Cyan
# Write-Host "========================================" -ForegroundColor Cyan
# Write-Host ""

# Write-Host "1. Grant Admin Consent:" -ForegroundColor Yellow
# Write-Host "   Go to: https://login.microsoftonline.com/$tenantId/adminconsent?client_id=$appId" -ForegroundColor White
# Write-Host "   Or use Azure Portal:" -ForegroundColor White
# Write-Host "   - Navigate to: Entra ID > Enterprise Applications > $DisplayName" -ForegroundColor White
# Write-Host "   - Click: Permissions > Grant admin consent for $tenantName" -ForegroundColor White
# Write-Host ""

# Write-Host "2. Configure API (appsettings.json):" -ForegroundColor Yellow
# Write-Host @"
# {
#   "AzureAd": {
#     "Instance": "https://login.microsoftonline.com/",
#     "TenantId": "$tenantId",
#     "ClientId": "$appId",
#     "Audience": "$appIdUri",
#     "ValidAudiences": [
#       "$appIdUri",
#       "$appId"
#     ]
#   }
# }
# "@ -ForegroundColor White
# Write-Host ""

# Write-Host "3. Configure SPA (React/TypeScript):" -ForegroundColor Yellow
# Write-Host @"
# {
#   "auth": {
#     "clientId": "$appId",
#     "authority": "https://login.microsoftonline.com/$tenantId",
#     "redirectUri": "http://localhost:5173/",
#     "postLogoutRedirectUri": "http://localhost:5173/"
#   },
#   "scopes": {
#     "api": ["$appIdUri/$ApplicationName"]
#   }
# }
# "@ -ForegroundColor White
# Write-Host ""

# Write-Host "4. Install Required NuGet Packages for API:" -ForegroundColor Yellow
# Write-Host "   dotnet add package Microsoft.Identity.Web" -ForegroundColor White
# Write-Host "   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer" -ForegroundColor White
# Write-Host ""

# Write-Host "5. Install Required npm Packages for SPA:" -ForegroundColor Yellow
# Write-Host "   npm install @azure/msal-browser @azure/msal-react" -ForegroundColor White
# Write-Host ""

# Write-Host "Registration completed successfully!" -ForegroundColor Green
# Write-Host "All configuration values have been output above." -ForegroundColor Green
# Write-Host ""
