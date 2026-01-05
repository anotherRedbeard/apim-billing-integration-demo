# 📖 Project Proposal Navigation Guide

Welcome! This directory contains the complete project structure proposal for the **APIM Billing Integration Demo** using **.NET 9**.

---

## 🚀 Quick Start - Where to Begin?

### 1️⃣ **New to the project?**
Start here: **[PROPOSAL_SUMMARY.md](./PROPOSAL_SUMMARY.md)**
- Quick overview
- Technology stack
- Key flows diagram
- Implementation plan

### 2️⃣ **Want detailed specifications?**
Read this: **[PROJECT_STRUCTURE_PROPOSAL.md](./PROJECT_STRUCTURE_PROPOSAL.md)**
- Complete directory structure
- Component breakdown
- Testing strategy
- GitHub Actions workflows
- Clarifying questions

### 3️⃣ **Visual learner?**
Check out: **[ARCHITECTURE_DIAGRAMS.md](./ARCHITECTURE_DIAGRAMS.md)**
- System overview diagram
- Purchase flow sequence
- Key rotation flow
- Deactivation flow
- Deployment architecture

### 4️⃣ **Understanding requirements?**
Reference: **[COPILOT_INSTRUCTIONS.md](./COPILOT_INSTRUCTIONS.md)**
- Original project requirements
- Deliverables checklist
- Security expectations
- Workflow instructions

---

## 📂 Proposal Documents

| Document | Purpose | Audience |
|----------|---------|----------|
| **PROPOSAL_SUMMARY.md** | High-level overview and quick reference | Everyone - start here! |
| **PROJECT_STRUCTURE_PROPOSAL.md** | Detailed technical specification | Developers & Architects |
| **ARCHITECTURE_DIAGRAMS.md** | Visual system architecture | Visual learners & Stakeholders |
| **COPILOT_INSTRUCTIONS.md** | Original requirements and guidelines | Project context |
| **README.md** | Project introduction (existing) | New contributors |

---

## 🎯 Current Status

**Stage**: ✋ **Proposal Phase - Awaiting Approval**

**What's Next**:
1. Review all proposal documents
2. Answer clarifying questions in PROJECT_STRUCTURE_PROPOSAL.md
3. Approve or request changes to the proposed structure
4. Once approved → Code generation begins

---

## ❓ Key Questions Requiring Answers

Before code generation can begin, please answer these questions from [PROJECT_STRUCTURE_PROPOSAL.md](./PROJECT_STRUCTURE_PROPOSAL.md#-clarifying-questions):

1. **Database**: Do we need to persist purchase history?
2. **Frontend Authentication**: Should the MVC frontend require user login?
3. **Deployment Targets**: Azure Web Apps or Container Apps?
4. **Logging**: Application Insights or console logging?
5. **Error Handling**: Retry logic for ARM API failures?
6. **Key Display**: Show keys in UI or send via email (stubbed)?
7. **Subscription Naming**: How to generate APIM subscription IDs?
8. **Rate Limiting**: Should Backend API have rate limiting?
9. **APIM State**: Track state locally or always query ARM?
10. **Payment Validation**: Stubbed only, or add basic validation?

---

## 📋 Proposed Solution Structure

```
apim-billing-integration-demo/
├── src/
│   ├── ApimBilling.Frontend/        # ASP.NET Core MVC (.NET 9)
│   ├── ApimBilling.Backend/         # Minimal API (.NET 9)
│   ├── ApimBilling.ArmClient/       # ARM Client Library
│   └── ApimBilling.Shared/          # Shared Models
├── tests/
│   ├── ApimBilling.Frontend.Tests/
│   ├── ApimBilling.Backend.Tests/
│   └── ApimBilling.ArmClient.Tests/
├── docs/
│   ├── architecture.md
│   ├── flows.md
│   ├── api.md
│   ├── deployment.md
│   └── development.md
├── .github/workflows/
│   ├── build-and-test.yml
│   ├── deploy-backend.yml
│   └── deploy-frontend.yml
└── .env.example
```

---

## 🔄 Implementation Phases (After Approval)

1. ✅ **Proposal** ← You are here
2. ⏳ **Approval & Q&A**
3. 🏗️ **Project Setup** - Create solution & projects
4. 🔐 **ARM Client** - Implement Azure integration
5. 🔗 **Backend API** - Implement Minimal API
6. 🎨 **Frontend MVC** - Implement web UI
7. ✅ **Testing** - Add unit & integration tests
8. 🔄 **CI/CD** - Create GitHub Actions
9. 📝 **Documentation** - Write comprehensive docs

---

## 💡 How to Provide Feedback

To approve or request changes:
1. Review all three proposal documents
2. Comment on any sections that need clarification
3. Answer the clarifying questions
4. Give explicit approval to proceed with code generation

---

## 🛠️ Technology Choices

- **Frontend**: ASP.NET Core MVC 9, Razor, Bootstrap 5
- **Backend**: .NET 9 Minimal API, Swagger/OpenAPI
- **ARM Integration**: Azure.Identity, HttpClient
- **Testing**: xUnit, Moq, FluentAssertions
- **CI/CD**: GitHub Actions
- **Documentation**: Markdown + Mermaid diagrams

---

**Ready to proceed?** Please review the documents and provide approval! 🚀
