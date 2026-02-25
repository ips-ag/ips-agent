using '../main.bicep'

param env = 'test'

param appServicePlanSku = {
  name: 'S1'
  tier: 'Standard'
}

param sqlUseFreeLimit = false
param sqlDatabaseSku = {
  name: 'GP_S_Gen5_2'
  tier: 'GeneralPurpose'
  capacity: 5
}
param sqlDatabaseMaxSizeBytes = 268435456000 // 250 GB

param sqlAllowPublicAccess = false

param logAnalyticsRetentionDays = 90

param keyVaultSoftDeleteRetentionDays = 30
param keyVaultEnablePurgeProtection = true

param tags = {
  environment: 'test'
  application: 'ipsagent'
  managedBy: 'bicep'
}

param entraIdClientId = ''
param entraIdTenantId = ''
param sqlAdminLogin = ''
param sqlAdminPassword = ''
