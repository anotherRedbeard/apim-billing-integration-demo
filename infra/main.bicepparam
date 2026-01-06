using './main.bicep'

// Environment Configuration
param environment = 'dev'
param location = 'southcentralus'
param baseName = 'apimbilling'

// App Service Plan Configuration
param appServicePlanSku = 'B1'

// APIM Configuration (Existing Resources)
param apimResourceGroup = readEnvironmentVariable('APIM_RESOURCE_GROUP', 'your-apim-resource-group')
param apimName = readEnvironmentVariable('APIM_NAME', 'your-apim-instance-name')

// APIM Product IDs (must match products in your APIM instance)
param productBronze = 'bronze'
param productSilver = 'silver'
param productGold = 'gold'
