# 🏗️ Architecture Diagram (Proposed)

## System Overview

```mermaid
flowchart TB
    subgraph "Customer Experience"
        Customer[👤 Customer]
    end
    
    subgraph "Frontend Layer"
        MVC[ASP.NET Core MVC<br/>Product Catalog & Purchase UI]
    end
    
    subgraph "Backend Layer"
        API[.NET 9 Minimal API<br/>Business Logic]
        PaymentSvc[Payment Service<br/>Stubbed]
        SubSvc[Subscription Service]
    end
    
    subgraph "Integration Layer"
        ARMClient[ARM Client Library<br/>APIM Operations]
    end
    
    subgraph "Azure Cloud"
        ARM[Azure Resource Manager<br/>Management API]
        APIM[Azure APIM<br/>API Gateway]
    end
    
    Customer -->|Browse & Purchase| MVC
    MVC -->|HTTP REST Calls| API
    API --> PaymentSvc
    API --> SubSvc
    SubSvc --> ARMClient
    ARMClient -->|ARM REST API<br/>2024-05-01| ARM
    ARM -->|Manage Subscriptions| APIM
    APIM -.->|Subscription Keys| ARMClient
    ARMClient -.->|Keys & Status| SubSvc
    SubSvc -.->|Purchase Result| API
    API -.->|JSON Response| MVC
    MVC -.->|Display Keys| Customer
    
    style Customer fill:#e1f5ff
    style MVC fill:#b3e5fc
    style API fill:#81d4fa
    style ARMClient fill:#4fc3f7
    style ARM fill:#29b6f6
    style APIM fill:#039be5
```

---

## Component Interaction - Purchase Flow

```mermaid
sequenceDiagram
    autonumber
    actor Customer
    participant MVC as Frontend<br/>(MVC)
    participant API as Backend<br/>(Minimal API)
    participant Payment as Payment<br/>Service
    participant Sub as Subscription<br/>Service
    participant ARM as ARM<br/>Client
    participant Azure as Azure<br/>APIM

    Customer->>MVC: Select "Gold Tier"
    Customer->>MVC: Click "Purchase"
    
    MVC->>API: POST /api/purchase<br/>{productId: "gold", customer}
    
    API->>Payment: ValidatePayment()
    Payment-->>API: ✓ Payment OK (stubbed)
    
    API->>Sub: CreateSubscription(productId, customer)
    
    Sub->>ARM: CreateApimSubscription()<br/>{scope, productId}
    ARM->>Azure: PUT /subscriptions/{id}<br/>ARM API 2024-05-01
    Azure-->>ARM: Subscription Created
    
    ARM->>Azure: POST /listSecrets
    Azure-->>ARM: {primaryKey, secondaryKey}
    
    ARM-->>Sub: {subscriptionId, keys}
    Sub-->>API: {subscriptionId, keys, status}
    
    API-->>MVC: 200 OK<br/>{subscriptionId, primaryKey, secondaryKey}
    
    MVC-->>Customer: 🎉 Purchase Successful!<br/>Display Keys
```

---

## Component Interaction - Key Rotation Flow

```mermaid
sequenceDiagram
    autonumber
    actor Admin
    participant API as Backend<br/>(Minimal API)
    participant Sub as Subscription<br/>Service
    participant ARM as ARM<br/>Client
    participant Azure as Azure<br/>APIM

    Admin->>API: POST /api/subscriptions/{id}/rotate<br/>{keyType: "primary"}
    
    API->>Sub: RotateKey(subscriptionId, keyType)
    
    Sub->>ARM: RegeneratePrimaryKey(subscriptionId)
    ARM->>Azure: POST /subscriptions/{id}/regeneratePrimaryKey<br/>ARM API 2024-05-01
    Azure-->>ARM: Key Regenerated
    
    ARM->>Azure: POST /listSecrets
    Azure-->>ARM: {primaryKey, secondaryKey}
    
    ARM-->>Sub: {newPrimaryKey, secondaryKey}
    Sub-->>API: {keys, status}
    
    API-->>Admin: 200 OK<br/>{newPrimaryKey, secondaryKey}
```

---

## Component Interaction - Deactivation Flow

```mermaid
sequenceDiagram
    autonumber
    actor BillingSystem as Billing<br/>System
    participant API as Backend<br/>(Minimal API)
    participant Sub as Subscription<br/>Service
    participant ARM as ARM<br/>Client
    participant Azure as Azure<br/>APIM

    BillingSystem->>API: DELETE /api/subscriptions/{id}<br/>(Payment failed/expired)
    
    API->>Sub: DeactivateSubscription(subscriptionId)
    
    Sub->>ARM: UpdateSubscriptionState(subscriptionId, "suspended")
    ARM->>Azure: PUT /subscriptions/{id}<br/>{state: "suspended"}
    Azure-->>ARM: Subscription Suspended
    
    ARM-->>Sub: {status: "suspended"}
    Sub-->>API: Deactivation Successful
    
    API-->>BillingSystem: 200 OK<br/>{status: "deactivated"}
    
    Note over Azure: Customer can no longer<br/>use API with this subscription
```

---

## Technology Stack Layers

```mermaid
flowchart LR
    subgraph "Presentation"
        A[ASP.NET Core MVC<br/>Razor Views<br/>Bootstrap 5]
    end
    
    subgraph "Application"
        B[.NET 9 Minimal API<br/>Swagger/OpenAPI]
    end
    
    subgraph "Business Logic"
        C[Subscription Service<br/>Payment Service<br/>Validation]
    end
    
    subgraph "Integration"
        D[ARM Client Library<br/>Azure.Identity<br/>HttpClient]
    end
    
    subgraph "Infrastructure"
        E[Azure APIM<br/>ARM API<br/>Managed Identity]
    end
    
    A --> B
    B --> C
    C --> D
    D --> E
```

---

## Deployment Architecture (Future State)

```mermaid
flowchart TB
    subgraph "GitHub"
        Code[Source Code]
        Actions[GitHub Actions<br/>CI/CD]
    end
    
    subgraph "Azure Cloud"
        subgraph "Frontend"
            WebApp1[Azure Web App<br/>MVC Frontend]
        end
        
        subgraph "Backend"
            WebApp2[Azure Web App<br/>Minimal API]
        end
        
        subgraph "Identity"
            MSI[Managed Identity<br/>or Service Principal]
        end
        
        subgraph "API Management"
            APIM2[Azure APIM<br/>Products & Subscriptions]
        end
        
        subgraph "Monitoring"
            AppInsights[Application Insights<br/>Logging & Telemetry]
        end
    end
    
    Code -->|Push/PR| Actions
    Actions -->|Deploy| WebApp1
    Actions -->|Deploy| WebApp2
    
    WebApp1 -->|API Calls| WebApp2
    WebApp2 -->|Authenticate| MSI
    WebApp2 -->|ARM API| APIM2
    
    WebApp1 -.->|Telemetry| AppInsights
    WebApp2 -.->|Telemetry| AppInsights
    
    style Code fill:#e8f5e9
    style Actions fill:#c8e6c9
    style WebApp1 fill:#a5d6a7
    style WebApp2 fill:#81c784
    style APIM2 fill:#66bb6a
```

---

## Project Structure (File System)

```mermaid
graph TD
    Root[apim-billing-integration-demo/]
    
    Root --> Src[src/]
    Root --> Tests[tests/]
    Root --> Docs[docs/]
    Root --> GitHub[.github/workflows/]
    Root --> Config[.env.example]
    
    Src --> Frontend[ApimBilling.Frontend/<br/>MVC App]
    Src --> Backend[ApimBilling.Backend/<br/>Minimal API]
    Src --> ArmClient[ApimBilling.ArmClient/<br/>ARM Library]
    Src --> Shared[ApimBilling.Shared/<br/>Common Models]
    
    Tests --> FrontendTests[ApimBilling.Frontend.Tests/]
    Tests --> BackendTests[ApimBilling.Backend.Tests/]
    Tests --> ArmTests[ApimBilling.ArmClient.Tests/]
    
    Docs --> ArchDoc[architecture.md]
    Docs --> FlowsDoc[flows.md]
    Docs --> ApiDoc[api.md]
    
    GitHub --> BuildWorkflow[build-and-test.yml]
    GitHub --> DeployBackend[deploy-backend.yml]
    GitHub --> DeployFrontend[deploy-frontend.yml]
    
    style Root fill:#fff3e0
    style Src fill:#ffe0b2
    style Tests fill:#ffcc80
    style Docs fill:#ffb74d
    style GitHub fill:#ffa726
```

---

**Note**: These diagrams represent the proposed architecture. They will be refined and included in the `docs/` folder once the structure is approved.
