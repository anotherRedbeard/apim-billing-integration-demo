using './main.bicep'

// Environment Configuration
param environment = 'dev'
param location = 'southcentralus'
param baseName = 'apimbilling'

// App Service Plan Configuration
param appServicePlanSku = 'B1'

// APIM Product IDs (must match products in your APIM instance)
param productBronze = 'bronze'
param productSilver = 'silver'
param productGold = 'gold'
