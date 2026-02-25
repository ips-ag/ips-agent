using '../main.bicep'

param env = 'dev'

param appServicePlanSku = {
  name: 'B1'
  tier: 'Basic'
}

// Dev uses the Azure SQL Free serverless offer
param sqlUseFreeLimit = true
param sqlDatabaseSku = {
  name: 'GP_S_Gen5'
  tier: 'GeneralPurpose'
  family: 'Gen5'
  capacity: 1
}
param sqlDatabaseMaxSizeBytes = 34359738368 // 32 GB

// Dev allows public internet access to SQL Server for local development
param sqlAllowPublicAccess = true

param logAnalyticsRetentionDays = 30

param keyVaultSoftDeleteRetentionDays = 7
param keyVaultEnablePurgeProtection = false

param tags = {
  environment: 'dev'
  application: 'ipsagent'
  managedBy: 'bicep'
}

param entraIdClientId = ''
param entraIdTenantId = ''
param sqlAdminLogin = ''
param sqlAdminPassword = ''
