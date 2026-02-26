targetScope = 'subscription'

// ─────────────────────────────────────────────────────────────────
// Parameters
// ─────────────────────────────────────────────────────────────────

@description('Environment tag: dev, test, or prod')
@allowed(['dev', 'test', 'prod'])
param env string

@description('Azure region for all resources')
param location string = 'swedencentral'

@description('SKU for the App Service Plan')
param appServicePlanSku {
  @description('SKU name (e.g. B1, S1, P1v3)')
  name: string
  @description('SKU tier (e.g. Basic, Standard, PremiumV3)')
  tier: string
}

@description('SKU for the SQL Database')
param sqlDatabaseSku {
  @description('SKU name')
  name: string
  @description('SKU tier')
  tier: string
  @description('Hardware generation family (required for vCore / serverless SKUs, e.g. Gen5)')
  family: string?
  @description('DTU or vCore capacity. Use 1 for the free serverless offer (GP_S_Gen5_1)')
  capacity: int?
}

@description('Enable the Azure SQL Free serverless offer (dev only)')
param sqlUseFreeLimit bool

@description('Allow public internet access to the SQL Server')
param sqlAllowPublicAccess bool = false

@secure()
@description('SQL Server administrator login name')
param sqlAdminLogin string

@secure()
@description('SQL Server administrator password')
param sqlAdminPassword string

@description('Entra ID Tenant ID for app authentication config')
param entraIdTenantId string

@description('Entra ID Client ID for the API')
param entraIdClientId string

@description('SQL Database maximum size in bytes')
param sqlDatabaseMaxSizeBytes int

@description('Log Analytics Workspace data retention in days')
@minValue(30)
@maxValue(730)
param logAnalyticsRetentionDays int

@description('Key Vault soft-delete retention in days')
@minValue(7)
@maxValue(90)
param keyVaultSoftDeleteRetentionDays int

@description('Enable Key Vault purge protection')
param keyVaultEnablePurgeProtection bool

@description('Resource tags applied to all resources')
param tags object

// ─────────────────────────────────────────────────────────────────
// Variables — Naming Conventions (CAF)
// ─────────────────────────────────────────────────────────────────

var resourceGroupName = 'rg-ipsagent-${env}'
var logAnalyticsName = 'log-ipsagent-${env}'
var appInsightsName = 'appi-ipsagent-${env}'
var keyVaultName = 'kv-ipsagent-${env}'
var appServicePlanName = 'asp-ipsagent-${env}'
var apiAppName = 'app-ipsagent-api-${env}'
var webAppName = 'app-ipsagent-app-${env}'
var sqlServerName = 'sql-ipsagent-${env}'
var sqlDatabaseName = 'sqldb-ipsagent-${env}'

// ─────────────────────────────────────────────────────────────────
// 1. Resource Group (subscription scope)
// ─────────────────────────────────────────────────────────────────

module rg 'br/public:avm/res/resources/resource-group:0.4.3' = {
  name: '${resourceGroupName}-deployment'
  params: {
    name: resourceGroupName
    location: location
    tags: tags
  }
}

// ─────────────────────────────────────────────────────────────────
// 2. Log Analytics Workspace
// ─────────────────────────────────────────────────────────────────

module logAnalytics 'br/public:avm/res/operational-insights/workspace:0.15.0' = {
  name: '${logAnalyticsName}-deployment'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: logAnalyticsName
    location: location
    tags: tags
    dataRetention: logAnalyticsRetentionDays
  }
  dependsOn: [rg]
}

// ─────────────────────────────────────────────────────────────────
// 3. Application Insights
// ─────────────────────────────────────────────────────────────────

module appInsights 'br/public:avm/res/insights/component:0.7.1' = {
  name: '${appInsightsName}-deployment'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: appInsightsName
    location: location
    tags: tags
    workspaceResourceId: logAnalytics.outputs.resourceId
  }
}

// ─────────────────────────────────────────────────────────────────
// 4. App Service Plan (shared by both App Services, Linux)
// ─────────────────────────────────────────────────────────────────

module appServicePlan 'br/public:avm/res/web/serverfarm:0.7.0' = {
  name: '${appServicePlanName}-deployment'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: appServicePlanName
    location: location
    tags: tags
    skuName: appServicePlanSku.name
    reserved: true
    kind: 'Linux'
  }
  dependsOn: [rg]
}

// ─────────────────────────────────────────────────────────────────
// 5. API App Service (ASP.NET 10, system-assigned managed identity)
// ─────────────────────────────────────────────────────────────────

module apiApp 'br/public:avm/res/web/site:0.22.0' = {
  name: '${apiAppName}-deployment'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: apiAppName
    location: location
    tags: tags
    kind: 'app,linux'
    serverFarmResourceId: appServicePlan.outputs.resourceId
    managedIdentities: {
      systemAssigned: true
    }
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      healthCheckPath: '/health'
      cors: {
        allowedOrigins: [
          'https://${webAppName}.azurewebsites.net'
        ]
        supportCredentials: true
      }
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=appi-connection-string)'
        }
        { name: 'AzureAd__TenantId', value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=auth-tenant-id)' }
        { name: 'AzureAd__ClientId', value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=auth-client-id)' }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=sql-connection-string)'
        }
      ]
    }
    httpsOnly: true
  }
}

// ─────────────────────────────────────────────────────────────────
// 6. Web App Service (Node 22, static SPA served via npx serve)
// ─────────────────────────────────────────────────────────────────

module webApp 'br/public:avm/res/web/site:0.22.0' = {
  name: '${webAppName}-deployment'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: webAppName
    location: location
    tags: tags
    kind: 'app,linux'
    serverFarmResourceId: appServicePlan.outputs.resourceId
    managedIdentities: {
      systemAssigned: true
    }
    siteConfig: {
      linuxFxVersion: 'NODE|22-lts'
      appCommandLine: 'pm2 serve /home/site/wwwroot --spa --no-daemon'
      appSettings: [
        { name: 'WEBSITE_NODE_DEFAULT_VERSION', value: '~22' }
      ]
    }
    httpsOnly: true
  }
}

// ─────────────────────────────────────────────────────────────────
// 7. Azure SQL Server with Database
// ─────────────────────────────────────────────────────────────────

module sqlServer 'br/public:avm/res/sql/server:0.21.1' = {
  name: '${sqlServerName}-deployment'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: sqlServerName
    location: location
    tags: tags
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: sqlAllowPublicAccess ? 'Enabled' : 'Disabled'
    databases: [
      {
        name: sqlDatabaseName
        collation: 'SQL_Latin1_General_CP1_CI_AS'
        sku: sqlDatabaseSku
        maxSizeBytes: sqlDatabaseMaxSizeBytes
        availabilityZone: -1
        zoneRedundant: false
        requestedBackupStorageRedundancy: env == 'prod' ? 'Geo' : 'Local'
        useFreeLimit: sqlUseFreeLimit
        freeLimitExhaustionBehavior: sqlUseFreeLimit ? 'AutoPause' : null
        autoPauseDelay: 60
        minCapacity: '0.5'
      }
    ]
    firewallRules: [
      {
        name: 'AllowAllWindowsAzureIps'
        startIpAddress: '0.0.0.0'
        endIpAddress: '0.0.0.0'
      }
    ]
  }
  dependsOn: [rg]
}

// ─────────────────────────────────────────────────────────────────
// 8. Key Vault (RBAC authorization, role assignments for App Service MIs)
// ─────────────────────────────────────────────────────────────────

module keyVault 'br/public:avm/res/key-vault/vault:0.13.3' = {
  name: '${keyVaultName}-deployment'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: keyVaultName
    location: location
    tags: tags
    sku: 'standard'
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: keyVaultSoftDeleteRetentionDays
    enablePurgeProtection: keyVaultEnablePurgeProtection ? true : null
    publicNetworkAccess: 'Enabled'
    roleAssignments: [
      {
        principalId: apiApp.outputs.?systemAssignedMIPrincipalId ?? ''
        roleDefinitionIdOrName: 'Key Vault Secrets User'
        principalType: 'ServicePrincipal'
      }
      {
        principalId: webApp.outputs.?systemAssignedMIPrincipalId ?? ''
        roleDefinitionIdOrName: 'Key Vault Secrets User'
        principalType: 'ServicePrincipal'
      }
    ]
  }
}

// ─────────────────────────────────────────────────────────────────
// 9. Key Vault Secrets
// ─────────────────────────────────────────────────────────────────

module kvSecrets './modules/keyvault-secrets.bicep' = {
  name: '${keyVaultName}-secrets-deployment'
  scope: resourceGroup(resourceGroupName)
  params: {
    keyVaultName: keyVaultName
    appiConnectionString: appInsights.outputs.connectionString
    sqlServerFqdn: sqlServer.outputs.fullyQualifiedDomainName
    sqlDatabaseName: sqlDatabaseName
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
    authTenantId: entraIdTenantId
    authClientId: entraIdClientId
  }
  dependsOn: [keyVault]
}
