
# APIM Billing Integration Demo

A complete .NET 9 reference implementation showing how to integrate an **external billing system** with **Azure API Management (APIM)** to provision, update, and deactivate API subscriptions based on customer payment actions.

This demo provides a clean, repeatable pattern for organizations implementing **API monetization**, **external billing workflows**, or **custom subscription lifecycle automation**.

---

## 🎯 What This Demo Shows

This sample demonstrates the full **browse → purchase → manage** subscription lifecycle:

1. **Customer logs in** with email and name (session-based demo authentication)
2. **Customer browses products** - Dynamically fetched from your APIM instance
3. **Customer purchases a product**:
   - Creates APIM user (or uses existing AAD user)
   - Creates active APIM subscription linked to the user
   - Receives subscription keys (hidden by default for security)
4. **Customer manages subscriptions**:
   - View "My Subscriptions" filtered by their email
   - Simulate payment actions: "User Stopped Paying" → Suspends, "User Resumed Paying" → Reactivates
   - "User Declined to Pay" → Permanently deletes subscription
   - Rotate API keys as needed
5. **Billing API interacts with ARM**:
   - Creates/updates APIM users (supports both custom and AAD users)
   - Manages subscription lifecycle via Azure Resource Manager REST API
   - Retrieves and rotates subscription keys

**Key Patterns**: 
- ARM REST APIs (api-version 2024-05-01) with Managed Identity authentication
- Dynamic product discovery - no hardcoded product lists
- User Secrets for local development secrets
- Session-based demo authentication (no real auth required)
- Proper AAD user support via APIM user lookup

---

## 🏗 Architecture

```mermaid
flowchart LR
    Customer[Customer Browser]
    Frontend[ASP.NET MVC Frontend]
    Backend[.NET Minimal API Backend]
    ARM[Azure ARM API]
    APIM[Azure APIM]
    
    Customer -->|Login/Browse Products| Frontend
    Frontend -->|HTTP Request| Backend
    Backend -->|Managed Identity Auth| ARM
    ARM -->|Manage Users & Subscriptions| APIM
    APIM -->|Keys & Status| Backend
    Backend -->|Response| Frontend
    Frontend -->|Display Keys & Details| Customer
```

### Components

- **Frontend** (`ApimBilling.Web`): 
  - ASP.NET Core MVC with Bootstrap 5 and Bootstrap Icons
  - Session-based user tracking (demo mode - no real authentication)
  - Product catalog, subscription management, and customer action workflows
  - Secure key display (hidden by default, reveal on click)
  
- **Backend** (`ApimBilling.Api`): 
  - .NET 9 Minimal API with Swagger UI
  - Dynamic product discovery from APIM
  - APIM user management (supports AAD users via API lookup)
  - Subscription CRUD operations via ARM REST API
  - Managed Identity authentication
  
- **Infrastructure**: 
  - Bicep templates for App Services and Application Insights
  - Parameter-based configuration (no hardcoded values)
  - RBAC role assignments for Managed Identity

---

## 📦 Project Structure

```
apim-billing-integration-demo/
├── src/
│   ├── ApimBilling.Web/              # Frontend MVC app (ports 5012/7108)
│   ├── ApimBilling.Api/              # Backend Minimal API (ports 5000/5001)
│   └── ApimBilling.Contracts/        # Shared DTOs
├── infra/                             # Bicep infrastructure
│   ├── main.bicep                     # Main infrastructure template
│   ├── main.bicepparam                # Parameter file (placeholders for Git)
│   └── deploy.sh.example              # Deployment script template
└── README.md
```

---

## 🚀 Quick Start

### Prerequisites

- .NET 9 SDK
- Azure CLI
- Azure subscription with an existing APIM instance
- Products created in your APIM instance (any products - they'll be discovered dynamically)

### 1. Configure Local Development

```bash
# Clone the repository
git clone https://github.com/anotherRedbeard/apim-billing-integration-demo.git
cd apim-billing-integration-demo

# Configure user secrets for backend API
cd src/ApimBilling.Api
dotnet user-secrets set "APIM_NAME" "your-apim-instance-name"
dotnet user-secrets set "APIM_RESOURCE_GROUP" "your-apim-resource-group"
dotnet user-secrets set "AZURE_SUBSCRIPTION_ID" "your-subscription-id"
cd ../..
```

### 2. Run Locally

```bash
# Ensure you're logged into Azure CLI (for local authentication)
az login
az account set --subscription <your-subscription-id>

# Run backend API (in first terminal)
cd src/ApimBilling.Api
dotnet run
# API available at: http://localhost:5000
# Swagger UI at: http://localhost:5000/swagger

# Run frontend (in second terminal)
cd src/ApimBilling.Web
dotnet run
# Web app available at: http://localhost:5012
```

### 3. Test the Application

1. Open browser to http://localhost:5012
2. Click "Login" in the navbar and enter your email/name
3. Browse products (fetched from your APIM instance)
4. Purchase a product - creates subscription with keys
5. View "My Subscriptions" to see your active subscriptions
6. Click on a subscription to manage it (suspend, resume, delete, rotate keys)

**Note:** For HTTPS locally, you may need to trust the development certificate:
```bash
dotnet dev-certs https --trust
```
Then access at https://localhost:7108 (frontend) and https://localhost:5001 (backend).

### 4. Deploy to Azure

#### Using Azure Developer CLI (azd) - Recommended

The fastest way to deploy both infrastructure and code in one command:

```bash
# Install azd (if not already installed)
# macOS/Linux
curl -fsSL https://aka.ms/install-azd.sh | bash

# Windows
powershell -ex AllSigned -c "Invoke-RestMethod 'https://aka.ms/install-azd.ps1' | Invoke-Expression"

# Login to Azure
azd auth login

# Create a new environment (e.g., dev, staging, prod)
azd env new dev

# Set required environment variables
azd env set APIM_NAME your-apim-instance-name
azd env set APIM_RESOURCE_GROUP your-apim-resource-group
azd env set AZURE_SUBSCRIPTION_ID your-subscription-id
azd env set AZURE_LOCATION southcentralus

# Deploy everything (infrastructure + code)
azd up

# Or deploy separately:
azd provision  # Deploy infrastructure only
azd deploy     # Deploy code only
```

**What `azd up` does:**
1. ✅ Creates/updates Azure resources (App Services, Application Insights, Log Analytics)
2. ✅ Builds and deploys both backend API and frontend web applications
3. ✅ Automatically configures RBAC for Managed Identity to access APIM
4. ✅ Displays URLs for your deployed applications

**Multi-environment support:**
```bash
# Create production environment
azd env new prod
azd env set APIM_NAME your-prod-apim-instance
azd env set APIM_RESOURCE_GROUP your-prod-apim-rg
# ... set other prod values
azd up  # Deploys to production

# Switch between environments
azd env select dev
azd deploy  # Deploy to dev

azd env select prod
azd deploy  # Deploy to prod
```

#### GitHub Actions CI/CD

Push to `main` branch and the GitHub Actions workflow will automatically:
- ✅ Provision infrastructure using Bicep
- ✅ Build .NET applications
- ✅ Deploy to Azure App Services
- ✅ Configure RBAC for APIM access

The workflow uses federated identity (OIDC) for secure, secretless authentication.

See [`.github/workflows/azure-dev.yml`](.github/workflows/azure-dev.yml) for details.


---

## 🔧 Configuration

### Local Development (.NET User Secrets)

The recommended approach for local development:

```bash
cd src/ApimBilling.Api
dotnet user-secrets set "APIM_NAME" "your-apim-instance"
dotnet user-secrets set "APIM_RESOURCE_GROUP" "your-apim-rg"
dotnet user-secrets set "AZURE_SUBSCRIPTION_ID" "your-subscription-id"
```

User secrets are stored in `~/.microsoft/usersecrets/` and are never committed to source control.

### Azure Deployment (Environment Variables)

When deployed to Azure App Service, the Bicep template automatically sets these environment variables from parameters. The .NET app reads from `IConfiguration` which pulls from environment variables in Azure.

### Configuration Files

- **appsettings.Development.json** - Local development settings with localhost URLs
- **appsettings.json** - Production settings (no secrets)
- **infra/main.bicepparam** - Bicep parameters with token substitution (e.g., `${APIM_NAME}`)
- **.azure/{env}/.env** - Environment-specific values (gitignored, created by azd)

---

## ✨ Features

### Frontend Features
- 🔐 Session-based user tracking (demo mode - no real authentication required)
- 📦 Dynamic product catalog (fetched from your APIM instance)
- 🔑 Secure key display (hidden by default, reveal with button click)
- 📊 Subscription management dashboard
- 🎯 Customer action simulation (paid, stopped paying, declined to pay)
- 🔄 Key rotation functionality
- 📱 Responsive Bootstrap 5 UI with icons

### Backend Features
- 🚀 .NET 9 Minimal API with OpenAPI/Swagger
- 🔍 Dynamic product discovery from APIM
- 👤 APIM user management (creates custom users, supports AAD users)
- 📝 Full subscription CRUD operations
- 🔑 Subscription key retrieval and rotation
- 🛡️ Managed Identity authentication (no credentials in code)
- 📈 Application Insights integration
- ✅ Proper error handling and logging

### Infrastructure Features
- 🏗️ Bicep infrastructure as code
- 🎭 Managed Identity with RBAC
- 🔒 Parameter-based configuration (no secrets in templates)
- 📊 Application Insights monitoring
- 🌐 App Service hosting for both frontend and backend

---

## 🧪 Testing

The solution includes comprehensive testing capabilities:

```bash
# Run all tests (when implemented)
dotnet test

# Run specific project tests
dotnet test tests/ApimBilling.Api.Tests

# Run with c://localhost:5000
dotnet test /p:CollectCoverage=true
```

**Test locally using Swagger UI:**
1. Start the backend API: `cd src/ApimBilling.Api && dotnet run`
2. Open https://localhost:5001/swagger
3. Test endpoints interactively

---

## 🔒 Security

- ✅ **Managed Identity** for Azure authentication (no credentials in code)
- ✅ **User Secrets** for local development (secrets stored outside source control)
- ✅ **No secrets in Git** - appsettings files use placeholders only
- ✅ **HTTPS enforced** on all communication
- ✅ **CORS configured** for frontend-backend communication
- ✅ **Key security** - API keys hidden by default in UI
- ✅ **AAD user support** - proper lookup via APIM API
- ✅ **Application Insights** for monitoring and diagnostics

---

## 📚 Additional Resources

- [Azure API Management Documentation](https://learn.microsoft.com/azure/api-management/)
- [ARM REST API Reference](https://learn.microsoft.com/rest/api/apimanagement/)
- [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets)
- [Azure Managed Identity](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/)

---

## 📝 License

This project is licensed under the MIT License - see [LICENSE](LICENSE) file.

---

## 🤝 Contributing

This is a reference demo. Feel free to fork and adapt to your organization's needs. Contributions and feedback are welcome!
