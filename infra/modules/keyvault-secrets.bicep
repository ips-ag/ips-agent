// ─────────────────────────────────────────────────────────────────
// Key Vault Secrets Module
// Creates secrets as child resources of an existing Key Vault.
// Declared separately so that the vault and role assignments exist
// before secrets are written.
// ─────────────────────────────────────────────────────────────────

@description('Name of the existing Key Vault')
param keyVaultName string

@secure()
@description('Application Insights connection string')
param appiConnectionString string

@description('SQL Server fully qualified domain name')
param sqlServerFqdn string

@description('SQL Database name')
param sqlDatabaseName string

@description('SQL Server administrator login')
param sqlAdminLogin string

@secure()
@description('SQL Server administrator password')
param sqlAdminPassword string

@description('Entra ID Tenant ID')
param authTenantId string

@description('Entra ID Client ID')
param authClientId string

// ─────────────────────────────────────────────────────────────────
// Existing Key Vault reference
// ─────────────────────────────────────────────────────────────────

resource keyVault 'Microsoft.KeyVault/vaults@2025-05-01' existing = {
  name: keyVaultName
}

// ─────────────────────────────────────────────────────────────────
// Secrets
// ─────────────────────────────────────────────────────────────────

resource appiConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2025-05-01' = {
  parent: keyVault
  name: 'appi-connection-string'
  properties: {
    value: appiConnectionString
  }
}

resource sqlConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2025-05-01' = {
  parent: keyVault
  name: 'sql-connection-string'
  properties: {
    value: 'Server=tcp:${sqlServerFqdn},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
  }
}

resource authTenantIdSecret 'Microsoft.KeyVault/vaults/secrets@2025-05-01' = {
  parent: keyVault
  name: 'auth-tenant-id'
  properties: {
    value: authTenantId
  }
}

resource authClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2025-05-01' = {
  parent: keyVault
  name: 'auth-client-id'
  properties: {
    value: authClientId
  }
}
