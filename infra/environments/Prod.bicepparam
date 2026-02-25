using '../main.bicep'

param env = 'prod'

param appServicePlanSku = {
  name: 'P1v3'
  tier: 'PremiumV3'
}

param sqlUseFreeLimit = false
param sqlDatabaseSku = {
  name: 'S1'
  tier: 'Standard'
  capacity: 20
}
param sqlDatabaseMaxSizeBytes = 268435456000 // 250 GB

param sqlAllowPublicAccess = false

param logAnalyticsRetentionDays = 180

param keyVaultSoftDeleteRetentionDays = 30
param keyVaultEnablePurgeProtection = true

param tags = {
  environment: 'prod'
  application: 'ipsagent'
  managedBy: 'bicep'
}

param entraIdClientId = ''
param entraIdTenantId = ''
param sqlAdminLogin = ''
param sqlAdminPassword = ''
