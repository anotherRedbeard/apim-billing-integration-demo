# Local Development Configuration for API Testing

## Configuration Values

Update these in `appsettings.Development.json`:

```json
{
  "APIM_NAME": "your-apim-name",
  "APIM_RESOURCE_GROUP": "your-apim-resource-group-name",
  "AZURE_SUBSCRIPTION_ID": "your-subscription-id",
  "PRODUCT_BRONZE": "your-bronze-product-id",
  "PRODUCT_SILVER": "your-silver-product-id",
  "PRODUCT_GOLD": "your-gold-product-id"
}
```

## Get Your Product IDs

Run this Azure CLI command to list your APIM products:

```bash
az apim product list \
  --resource-group <apim-resource-group-name> \
  --service-name <apim-name> \
  --query "[].{Name:displayName, Id:name}" \
  --output table
```

## Authentication for Local Testing

The API uses `DefaultAzureCredential` which will try multiple authentication methods in order:

1. **Environment Variables** (not recommended for local dev)
2. **Managed Identity** (only works in Azure)
3. **Visual Studio** credential
4. **Azure CLI** credential ✅ **Use this for local testing**
5. **Azure PowerShell** credential
6. **Interactive Browser** credential

### Recommended: Use Azure CLI

```bash
# Login to Azure
az login

# Set the correct subscription
az account set --subscription <subscription-id>

# Verify you're logged in
az account show
```

## Run the API

```bash
cd src/ApimBilling.Api
dotnet run
```

The API will start at:

- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000
- Swagger UI: https://localhost:5001/swagger

## Test Endpoints

### 1. Health Check

```bash
curl https://localhost:5001/health -k
```

### 2. Get Products

```bash
curl https://localhost:5001/api/products -k
```

### 3. Purchase a Product (Creates APIM Subscription)

```bash
curl -X POST https://localhost:5001/api/subscriptions/purchase \
  -H "Content-Type: application/json" \
  -d '{
    "tier": "Bronze",
    "customerEmail": "test@example.com",
    "customerName": "Test User"
  }' -k
```

### 4. Get Subscription

```bash
# Replace {subscription-id} with the ID returned from purchase
curl https://localhost:5001/api/subscriptions/{subscription-id} -k
```

### 5. Update Subscription State

```bash
curl -X PATCH https://localhost:5001/api/subscriptions/{subscription-id}/state \
  -H "Content-Type: application/json" \
  -d '{"action": "suspend"}' -k
```

### 6. Rotate Key

```bash
curl -X POST https://localhost:5001/api/subscriptions/{subscription-id}/rotate-key \
  -H "Content-Type: application/json" \
  -d '{"keyType": "primary"}' -k
```

### 7. Delete Subscription

```bash
curl -X DELETE https://localhost:5001/api/subscriptions/{subscription-id} -k
```

## Using Swagger UI (Recommended)

1. Start the API: `dotnet run`
2. Open browser: https://localhost:5001/swagger
3. Click "Try it out" on any endpoint
4. Fill in the request body and click "Execute"

## Troubleshooting

### Error: "APIM_NAME is required"

- Update `appsettings.Development.json` with your values

### Error: Authentication failed

```bash
# Make sure you're logged into Azure CLI
az login
az account set --subscription <your-subscription-id>
```

### Error: Forbidden (403)

- Ensure your Azure account has permissions on the APIM instance
- You may need "API Management Service Contributor" role

### SSL Certificate Error

- Use `-k` flag with curl to ignore SSL cert validation
- Or use Swagger UI instead
