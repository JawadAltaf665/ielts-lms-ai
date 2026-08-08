/**
 * The seeded "system" tenant (`SystemTenantSeeder`, `IdentityDefaults.TenantId`) — the tenant for
 * direct/individual sign-up (as opposed to an institute reached via its own subdomain). Used only as
 * the placeholder in the auth store before `/auth/me` returns the real `tenantId` (session.ts), and
 * as the value of the dev-only `VITE_DEV_TENANT_ID` fallback header — see auth-api.ts. Real browser
 * traffic resolves its tenant from the Host header (`HostTenantResolutionStrategy`, Multi-Tenant
 * Architecture §5.1); this constant is not sent to the backend by default.
 */
export const SYSTEM_TENANT_ID = '00000000-0000-0000-0000-000000000001'

export const TENANT_HEADER = 'X-Tenant-Id'

/** Mirrors `ConfigurablePasswordPolicy` defaults (`PasswordPolicyOptions`) — min 12, mixed classes. */
export const PASSWORD_MIN_LENGTH = 12
