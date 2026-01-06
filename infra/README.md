# Infrastructure Deployment

This folder contains Bicep infrastructure-as-code for the APIM Billing Integration Demo.

## Resources Created

- **App Service Plan** (Linux, .NET 9)
- **Frontend App Service** (ASP.NET Core MVC)
- **Backend App Service** (Minimal API)
- **Application Insights** (with Log Analytics workspace)
- **Managed Identity** (System-assigned for Backend API)

## Prerequisites

1. Azure CLI installed
2. Existing APIM instance with products created (Bronze, Silver, Gold)
3. Contributor access to Azure subscription

## Configuration

Edit `main.bicepparam` with your values:

```bicep
param apimResourceGroup = 'your-apim-resource-group'
param apimName = 'your-apim-instance'
param productBronze = 'bronze-api-product'
param productSilver = 'silver-api-product'
param productGold = 'gold-api-product'
```

## Deployment

### 1. Login to Azure

```bash
az login
az account set --subscription "your-subscription-id"
```

### 2. Create Resource Group

```bash
az group create \
  --name rg-apimbilling-dev \
  --location southcentralus
```

### 3. Deploy Infrastructure

```bash
az deployment group create \
  --resource-group rg-apimbilling-dev \
  --template-file main.bicep \
  --parameters main.bicepparam
```

### 4. Assign RBAC Role to Backend Managed Identity

After deployment, grant the backend's managed identity permission to manage APIM subscriptions:

```bash
# Get the backend managed identity principal ID
BACKEND_PRINCIPAL_ID=$(az deployment group show \
  --resource-group rg-apimbilling-dev \
  --name main \
  --query properties.outputs.backendManagedIdentityPrincipalId.value \
  --output tsv)

# Get your APIM resource ID
APIM_ID=$(az apim show \
  --name your-apim-instance \
  --resource-group your-apim-resource-group \
  --query id \
  --output tsv)

# Assign "API Management Service Contributor" role
az role assignment create \
  --assignee $BACKEND_PRINCIPAL_ID \
  --role "API Management Service Contributor" \
  --scope $APIM_ID
```

## Outputs

After deployment, you'll get:

- **Frontend URL**: `https://apimbilling-web-dev-xxxxx.azurewebsites.net`
- **Backend URL**: `https://apimbilling-api-dev-xxxxx.azurewebsites.net`
- **Application Insights Connection String**
- **Managed Identity Principal ID**

## Environment Variables

The Bicep template automatically configures these environment variables in the App Services:

### Backend API
- `APIM_NAME`
- `APIM_RESOURCE_GROUP`
- `AZURE_SUBSCRIPTION_ID`
- `PRODUCT_BRONZE`
- `PRODUCT_SILVER`
- `PRODUCT_GOLD`
- `APPLICATIONINSIGHTS_CONNECTION_STRING`

### Frontend Web
- `BillingApiBaseUrl`
- `APPLICATIONINSIGHTS_CONNECTION_STRING`

## Clean Up

```bash
az group delete --name rg-apimbilling-dev --yes --no-wait
```

## Troubleshooting

### Issue: Backend can't access APIM

**Solution**: Ensure the Managed Identity has been assigned the "API Management Service Contributor" role (see step 4 above).

### Issue: Frontend can't reach Backend

**Solution**: Check that CORS is configured correctly in the Backend App Service settings.

### Issue: Deployment fails with "location not available"

**Solution**: Change the `location` parameter in `main.bicepparam` to a region where all services are available (e.g., eastus, westus2, westeurope).
