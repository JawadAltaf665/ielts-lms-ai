# Subscription & Billing

Full architecture — plans, feature matrix, lifecycle, billing cycles, proration, invoices/payments,
tax/currency, payment-provider ACL, webhooks, dunning, entitlement enforcement — lives in
[13_Subscription_Billing.md](../13_Subscription_Billing.md).

The concrete Stripe-native entity model (Product/Plan/Price catalog, Customer/PaymentMethod,
Subscription/SubscriptionItem, Invoice/InvoiceLine, Feature/Quota entitlement framework) that
Phase 5 Sprint 1 implements is defined in
[09_Database_Design_ERD.md §6.5.1](../09_Database_Design_ERD.md#651-stripe-native-billing-extension-adr-031),
with the rationale recorded in
[ADR-031](../28_ADR_Architecture_Decision_Records.md#adr-031--stripe-native-billing-entity-model).
