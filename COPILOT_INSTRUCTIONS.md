
# GitHub Copilot Project Bootstrap Instructions

You (Copilot) will generate a complete working demo showing how an external billing system can provision Azure API Management (APIM) subscriptions using Azure Resource Manager (ARM) APIs.

Important:

- Do NOT create or deploy the APIM instance.
- Assume the user will provide APIM name, resource group, subscription ID, and product IDs through environment variables.
- All secrets must come from environment variables. No secrets in code.

---

## 🧱 Project Summary

This demo simulates the following flow:

1. Customer purchases a product (Bronze / Silver / Gold).
2. Billing App validates payment (stubbed).
3. Billing App calls ARM APIs:
   - Create APIM subscription
   - Assign to product
   - Retrieve subscription keys
   - Rotate keys
   - Deactivate subscription
4. Billing App returns subscription keys to the caller.

This demonstrates how companies integrate external billing systems with APIM for monetized APIs.

---

## 📦 Required Deliverables (Copilot MUST generate)

### 1. **Billing App (core code)**

A minimal REST API with endpoints:

- `POST /purchase`
- `GET /products`
- `POST /subscriptions/{id}/rotate`
- `DELETE /subscriptions/{id}`

Supported languages (choose one cleanly and stick to it):

- TypeScript (Node / Express)
- C# (.NET Minimal API)
- Python (FastAPI)

Use real, idiomatic patterns for the chosen language.

---

### 2. **ARM Client Module**

A clean, reusable module/class that wraps:
PUT   /subscriptions/{id}
POST  /subscriptions/{id}/listSecrets
POST  /subscriptions/{id}/regeneratePrimaryKey
POST  /subscriptions/{id}/regenerateSecondaryKey
Use API version: `2024-05-01`

Authentication:

- Managed Identity OR Service Principal  
- All values come from environment variables.

---

### 3. **Environment Configuration System**

Copilot must generate:

- A `.env.example` file
- A config loader
- Validation that fails at startup if required variables are missing

Required environment variables:

APIM_NAME=
APIM_RESOURCE_GROUP=
AZURE_SUBSCRIPTION_ID=
AZURE_TENANT_ID=
AZURE_CLIENT_ID=
AZURE_CLIENT_SECRET=
PRODUCT_BRONZE=
PRODUCT_SILVER=
PRODUCT_GOLD=
---

### 4. **Tests**

Minimum:

- Unit tests for subscription service logic
- Mocked ARM client tests

---

### 5. **GitHub Actions Pipeline**

Workflow must:

- Install dependencies
- Run tests
- Build the app
- Deploy to Azure Web App or Container Apps (use GitHub secrets)

---

### 6. **Documentation**

Copilot must generate the `/docs` folder containing:

- `architecture.md`  
- `flows.md` (purchase → provision → use)  
- `api.md` (Billing App API reference)  

Include Mermaid diagrams, for example:

```mermaid
flowchart LR
    Client -->|POST /purchase| BillingApp
    BillingApp -->|ARM calls| ARM
    ARM --> APIM
    APIM --> BillingApp
    BillingApp --> Client
    🗂 Required Repo Structure
```

Copilot must produce:
/
├── src/
│   ├── frontend/ (ASP.NET MVC)
│   └── backend/ (Minimal API)
├── tests/
│   ├── backend.tests/
│   └── armclient.tests/
├── docs/
├── .github/workflows/
├── .env.example
├── README.md
└── COPILOT_INSTRUCTIONS.md

### 7. 🔐 Security Expectations

Copilot must:

Use environment variables for secrets
Avoid hard‑coding credentials
Use idiomatic secure HTTP client code
Never generate custom RBAC roles unless asked

### 8. 🧭 Copilot Workflow Instructions

Copilot must:

Propose a project structure plan as its first response.
Wait for approval.
Generate code via PRs, not giant drop‑in files.
Follow normal SDLC patterns.