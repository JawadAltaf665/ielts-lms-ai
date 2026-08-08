// Public, build-time config only (doc 16 §19) — never put secrets behind
// VITE_*, Vite inlines them into the client bundle.
export const env = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5000',
  appName: import.meta.env.VITE_APP_NAME ?? 'IELTS LMS AI',
  // Dev-only fallback for pre-auth tenant resolution (see features/auth/auth.constants.ts) — unset
  // in any real deployment, where the Host header itself resolves the tenant (Multi-Tenant
  // Architecture §5.1, HostTenantResolutionStrategy).
  devTenantId: import.meta.env.VITE_DEV_TENANT_ID as string | undefined,
} as const
