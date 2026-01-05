
# APIM Billing Integration Demo

A working reference implementation showing how to integrate an **external billing system** with **Azure API Management (APIM)** to provision, update, and deactivate API subscriptions based on successful purchases.

This repo provides a clean, repeatable pattern for organizations implementing **API monetization**, **external billing workflows**, or **custom subscription lifecycle automation**.

---

## 🎯 What This Demo Shows

This sample demonstrates the full **purchase → provision → consume** flow:

1. **Customer purchases a product** (Bronze / Silver / Gold tier) using an external billing system  
   _(demo payment step is stubbed for simplicity)_

2. **Billing system validates the payment**, then calls the **Azure Resource Manager (ARM) API** to:
   - Create a new APIM subscription  
   - Assign it to the purchased product  
   - Generate subscription keys  
   - Return those keys to the customer/app  

3. **Customer consumes APIs using their issued keys**, and APIM enforces:
   - Product-level access  
   - Rate limits / quotas  
   - Key rotation  
   - Deactivation if billing fails later

This pattern aligns with guidance discussed in customer scenarios:  
**Use ARM APIs — not the deprecated Direct Management API — to manage subscriptions.**

---

## 🏗 Architecture Overview
