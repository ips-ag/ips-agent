#disable-next-line BCP258
using '../main.bicep'

param env = 'test'

param appServicePlanSku = {
  name: 'S1'
  tier: 'Standard'
}

param sqlUseFreeLimit = false
param sqlDatabaseSku = {
  name: 'S1'
  tier: 'Standard'
  capacity: 20
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
