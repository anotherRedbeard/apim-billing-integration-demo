# 🏗️ Project Structure Proposal: APIM Billing Integration Demo

## Overview
This document proposes a complete project structure for the APIM Billing Integration Demo using **.NET 9** with:
- **ASP.NET Core MVC** for the frontend (customer-facing billing UI)
- **.NET 9 Minimal API** for the backend (REST API for APIM subscription operations)
- **ARM Client Library** for APIM subscription management
- **xUnit** for testing
- **GitHub Actions** for CI/CD

---

## 📁 Proposed Directory Structure

```
apim-billing-integration-demo/
├── .github/
│   └── workflows/
│       ├── build-and-test.yml          # CI: Build and test all projects
│       ├── deploy-backend.yml          # CD: Deploy Minimal API to Azure
│       └── deploy-frontend.yml         # CD: Deploy MVC app to Azure
│
├── src/
│   ├── ApimBilling.Frontend/           # ASP.NET Core MVC (.NET 9)
│   │   ├── Controllers/
│   │   │   ├── HomeController.cs       # Home/landing page
│   │   │   ├── ProductsController.cs   # Browse available products
│   │   │   └── PurchaseController.cs   # Purchase flow & checkout
│   │   ├── Models/
│   │   │   ├── ProductViewModel.cs
│   │   │   ├── PurchaseViewModel.cs
│   │   │   └── SubscriptionViewModel.cs
│   │   ├── Views/
│   │   │   ├── Home/
│   │   │   │   └── Index.cshtml
│   │   │   ├── Products/
│   │   │   │   └── Index.cshtml
│   │   │   ├── Purchase/
│   │   │   │   ├── Index.cshtml
│   │   │   │   └── Success.cshtml
│   │   │   └── Shared/
│   │   │       ├── _Layout.cshtml
│   │   │       └── _ValidationScriptsPartial.cshtml
│   │   ├── Services/
│   │   │   └── BillingApiClient.cs     # HTTP client to call backend API
│   │   ├── wwwroot/
│   │   │   ├── css/
│   │   │   ├── js/
│   │   │   └── lib/
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Program.cs
│   │   └── ApimBilling.Frontend.csproj
│   │
│   ├── ApimBilling.Backend/            # .NET 9 Minimal API
│   │   ├── Endpoints/
│   │   │   ├── ProductEndpoints.cs     # GET /products
│   │   │   ├── PurchaseEndpoints.cs    # POST /purchase
│   │   │   └── SubscriptionEndpoints.cs # POST /subscriptions/{id}/rotate, DELETE /subscriptions/{id}
│   │   ├── Services/
│   │   │   ├── ISubscriptionService.cs
│   │   │   ├── SubscriptionService.cs  # Business logic for subscription operations
│   │   │   ├── IPaymentService.cs
│   │   │   └── PaymentService.cs       # Stubbed payment validation
│   │   ├── Models/
│   │   │   ├── Product.cs
│   │   │   ├── PurchaseRequest.cs
│   │   │   ├── PurchaseResponse.cs
│   │   │   ├── SubscriptionInfo.cs
│   │   │   └── ApiResponse.cs
│   │   ├── Configuration/
│   │   │   ├── ApimConfiguration.cs
│   │   │   └── ConfigurationValidator.cs # Validates required env vars at startup
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Program.cs
│   │   └── ApimBilling.Backend.csproj
│   │
│   ├── ApimBilling.ArmClient/          # Reusable ARM client library
│   │   ├── IApimArmClient.cs
│   │   ├── ApimArmClient.cs            # Implements ARM API calls
│   │   ├── Models/
│   │   │   ├── ApimSubscription.cs
│   │   │   ├── SubscriptionKeys.cs
│   │   │   └── ArmResponse.cs
│   │   ├── Authentication/
│   │   │   └── AzureAuthenticationProvider.cs # Managed Identity / Service Principal
│   │   └── ApimBilling.ArmClient.csproj
│   │
│   └── ApimBilling.Shared/             # Shared models and utilities
│       ├── Models/
│       │   └── ProductTier.cs          # Enum: Bronze, Silver, Gold
│       ├── Constants/
│       │   └── ProductConstants.cs
│       └── ApimBilling.Shared.csproj
│
├── tests/
│   ├── ApimBilling.Backend.Tests/      # Backend API tests
│   │   ├── Services/
│   │   │   └── SubscriptionServiceTests.cs
│   │   ├── Endpoints/
│   │   │   ├── ProductEndpointsTests.cs
│   │   │   ├── PurchaseEndpointsTests.cs
│   │   │   └── SubscriptionEndpointsTests.cs
│   │   └── ApimBilling.Backend.Tests.csproj
│   │
│   ├── ApimBilling.ArmClient.Tests/    # ARM client tests (mocked)
│   │   ├── ApimArmClientTests.cs
│   │   ├── AuthenticationProviderTests.cs
│   │   └── ApimBilling.ArmClient.Tests.csproj
│   │
│   └── ApimBilling.Frontend.Tests/     # Frontend tests
│       ├── Controllers/
│       │   ├── ProductsControllerTests.cs
│       │   └── PurchaseControllerTests.cs
│       └── ApimBilling.Frontend.Tests.csproj
│
├── docs/
│   ├── architecture.md                 # System architecture overview
│   ├── flows.md                        # Purchase → Provision → Consume flows with Mermaid diagrams
│   ├── api.md                          # REST API reference
│   ├── deployment.md                   # Azure deployment guide
│   └── development.md                  # Local development setup
│
├── .env.example                        # Example environment variables (no secrets)
├── .gitignore                          # Standard .NET gitignore
├── ApimBilling.sln                     # Solution file
├── README.md                           # Project overview (already exists)
├── COPILOT_INSTRUCTIONS.md             # Instructions (already exists)
├── LICENSE                             # License (already exists)
└── PROJECT_STRUCTURE_PROPOSAL.md       # This file
```

---

## 🔧 Technology Stack

### Frontend (ASP.NET Core MVC)
- **.NET 9** runtime
- **ASP.NET Core MVC** with Razor views
- **Bootstrap 5** for UI styling
- **HttpClient** for calling backend API
- **Configuration** from appsettings.json + environment variables

### Backend (Minimal API)
- **.NET 9** runtime
- **Minimal API** pattern (no controllers, just endpoint mapping)
- **Dependency Injection** for services
- **Swagger/OpenAPI** for API documentation
- **Configuration validation** at startup

### ARM Client Library
- **Azure.Identity** for authentication (Managed Identity or Service Principal)
- **HttpClient** with custom ARM endpoint handlers
- **API version: 2024-05-01** as specified
- Supports:
  - `PUT /subscriptions/{id}` - Create/update subscription (including state changes)
  - `POST /subscriptions/{id}/listSecrets` - Get keys
  - `POST /subscriptions/{id}/regeneratePrimaryKey` - Rotate primary key
  - `POST /subscriptions/{id}/regenerateSecondaryKey` - Rotate secondary key
  - `DELETE /subscriptions/{id}` - Delete subscription permanently

**Note**: For subscription deactivation, we use `PUT /subscriptions/{id}` (supported in ARM API 2024-05-01) to update the subscription state to "suspended" rather than deleting it, preserving the subscription history for audit and compliance purposes.

### Testing
- **xUnit** test framework
- **Moq** for mocking dependencies
- **FluentAssertions** for readable assertions
- **Microsoft.AspNetCore.Mvc.Testing** for integration tests

---

## 📋 Key Components Breakdown

### 1. Frontend (ApimBilling.Frontend)
**Purpose**: Customer-facing web application for browsing products and making purchases.

**Key Features**:
- Landing page with product offerings (Bronze/Silver/Gold)
- Product catalog view
- Purchase/checkout flow
- Display subscription keys after successful purchase
- Responsive UI using Bootstrap

**Configuration**:
- Backend API base URL (from appsettings or environment variable)

---

### 2. Backend (ApimBilling.Backend)
**Purpose**: REST API that handles purchase requests and orchestrates APIM subscription provisioning.

**Endpoints**:
- `GET /api/products` - List available products
- `POST /api/purchase` - Process purchase and create APIM subscription
- `POST /api/subscriptions/{id}/rotate` - Rotate subscription keys
- `DELETE /api/subscriptions/{id}` - Deactivate subscription (suspends via ARM PUT)

**Business Logic**:
- Validate purchase requests
- Stub payment processing (return success for demo)
- Call ARM client to create APIM subscription
- Map products (Bronze/Silver/Gold) to APIM product IDs

**Configuration Validation**:
- Check required environment variables at startup
- Fail fast if missing: `APIM_NAME`, `APIM_RESOURCE_GROUP`, `AZURE_SUBSCRIPTION_ID`, etc.

---

### 3. ARM Client Library (ApimBilling.ArmClient)
**Purpose**: Reusable library for interacting with Azure ARM APIs for APIM subscription management.

**Responsibilities**:
- Authenticate using Azure.Identity (DefaultAzureCredential)
- Build ARM REST API requests
- Parse ARM responses
- Handle errors and retries

**ARM API Endpoints** (base: `https://management.azure.com`):
```
PUT    /subscriptions/{azureSubId}/resourceGroups/{rg}/providers/Microsoft.ApiManagement/service/{apimName}/subscriptions/{subId}?api-version=2024-05-01
POST   /subscriptions/{azureSubId}/resourceGroups/{rg}/providers/Microsoft.ApiManagement/service/{apimName}/subscriptions/{subId}/listSecrets?api-version=2024-05-01
POST   /subscriptions/{azureSubId}/resourceGroups/{rg}/providers/Microsoft.ApiManagement/service/{apimName}/subscriptions/{subId}/regeneratePrimaryKey?api-version=2024-05-01
POST   /subscriptions/{azureSubId}/resourceGroups/{rg}/providers/Microsoft.ApiManagement/service/{apimName}/subscriptions/{subId}/regenerateSecondaryKey?api-version=2024-05-01
```

---

### 4. Shared Library (ApimBilling.Shared)
**Purpose**: Common models and constants shared across projects.

**Contents**:
- Product tier enumeration
- Constants for product names/IDs
- Shared DTOs (if needed)

---

## 🔐 Environment Variables

**Required Variables** (from COPILOT_INSTRUCTIONS.md):
```bash
# Azure APIM Configuration
APIM_NAME=my-apim-instance
APIM_RESOURCE_GROUP=my-resource-group
AZURE_SUBSCRIPTION_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx

# Azure Authentication (Service Principal)
AZURE_TENANT_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
AZURE_CLIENT_ID=xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
AZURE_CLIENT_SECRET=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

# APIM Product IDs (must match existing products in APIM)
PRODUCT_BRONZE=bronze-tier
PRODUCT_SILVER=silver-tier
PRODUCT_GOLD=gold-tier

# Optional: Backend API URL (for Frontend)
BACKEND_API_URL=https://localhost:7001
```

**Validation Strategy**:
- Backend API validates all required variables at startup in `Program.cs`
- Throws detailed exception if any are missing
- Logs configuration (without secrets) for debugging

---

## 🧪 Testing Strategy

### Unit Tests
- **ApimBilling.ArmClient.Tests**: Test ARM client with mocked HttpClient
- **ApimBilling.Backend.Tests**: Test business logic with mocked ARM client

### Integration Tests
- Test full purchase flow (end-to-end with mocked ARM responses)
- Validate configuration loading and validation

### Test Data
- Use in-memory test data for products
- Mock ARM API responses (successful subscription creation, key rotation, etc.)

---

## 🚀 GitHub Actions Workflows

### 1. `build-and-test.yml` (CI)
**Triggers**: Push to main, PRs

**Jobs**:
1. Checkout code
2. Setup .NET 9 SDK
3. Restore dependencies
4. Build solution
5. Run all tests (xUnit)
6. Upload test results

### 2. `deploy-backend.yml` (CD)
**Triggers**: Manual (workflow_dispatch) or after successful CI on main

**Jobs**:
1. Build Backend API
2. Publish artifacts
3. Deploy to Azure Web App / Container Apps
4. Uses GitHub secrets for deployment credentials

### 3. `deploy-frontend.yml` (CD)
**Triggers**: Manual (workflow_dispatch) or after successful CI on main

**Jobs**:
1. Build Frontend MVC app
2. Publish artifacts
3. Deploy to Azure Web App
4. Uses GitHub secrets for deployment credentials

---

## 📚 Documentation (docs/)

### architecture.md
- System overview with Mermaid diagram
- Component responsibilities
- Technology choices rationale

### flows.md
- **Purchase Flow**: Customer → Frontend → Backend → ARM → APIM
- **Key Rotation Flow**: Backend → ARM → APIM
- **Deactivation Flow**: Billing failure → Backend → ARM → APIM
- Sequence diagrams using Mermaid

### api.md
- REST API endpoint reference
- Request/response examples
- Authentication requirements
- Error codes

### deployment.md
- Azure prerequisites (APIM instance, products, service principal)
- Environment variable setup
- CI/CD configuration
- Manual deployment steps

### development.md
- Local development setup
- Running the apps locally
- Testing guide
- Troubleshooting

---

## 🎯 Implementation Phases (After Approval)

**Phase 1: Project Setup**
- Create solution and projects
- Add NuGet packages
- Configure .gitignore and .env.example

**Phase 2: ARM Client Library**
- Implement Azure authentication
- Implement ARM API client
- Add unit tests

**Phase 3: Backend API**
- Implement Minimal API endpoints
- Implement business services
- Add configuration validation
- Add tests

**Phase 4: Frontend MVC**
- Create controllers and views
- Implement UI for products and purchase
- Add API client service
- Add tests

**Phase 5: CI/CD**
- Create GitHub Actions workflows
- Configure deployment (placeholder for Azure resources)

**Phase 6: Documentation**
- Write architecture.md
- Write flows.md with diagrams
- Write api.md
- Write deployment and development guides

---

## ❓ Clarifying Questions

Before proceeding with code generation, please confirm:

1. **APIM State Management**: Should the backend track subscription state in a local database, or always query ARM/APIM for the current state?

2. **Frontend Authentication**: Should the MVC frontend have user authentication, or is it open for demo purposes?

3. **Payment Validation**: The payment service will be stubbed (always successful). Is this acceptable, or should we add basic validation logic (e.g., check card number format)?

4. **Deployment Targets**: 
   - Should we target **Azure Web Apps** or **Azure Container Apps** for deployment?
   - Do you have preferred naming conventions for Azure resources?

5. **Database**: Do we need a database to store purchase history, or is this purely stateless (each purchase creates a new APIM subscription)?

6. **Error Handling**: How should the system behave if ARM API calls fail (e.g., APIM is down, product doesn't exist)? Return error to user, retry, or log and continue?

7. **Key Display**: Should subscription keys be shown directly in the UI after purchase, or sent via email (stubbed)?

8. **Subscription Naming**: How should APIM subscription IDs be generated? (e.g., `{tier}-{timestamp}-{guid}`)

9. **Rate Limiting**: Should the Backend API have rate limiting, or rely on APIM/Azure for that?

10. **Logging**: Should we use Application Insights, or stick with console logging for simplicity?

---

## ✅ Next Steps

Once you approve this structure and answer the clarifying questions, I will:

1. Create the .NET solution and projects
2. Generate initial code for each component
3. Create GitHub Actions workflows
4. Write documentation
5. Open a PR for review

Please review and provide feedback! 🚀
