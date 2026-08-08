import { apiFetch } from '@/shared/lib/api-client'
import { env } from '@/shared/config/env'

import { TENANT_HEADER } from '../auth.constants'
import type {
  AccessTokenResponse,
  CurrentUserResponse,
  ForgotPasswordRequest,
  LoginRequest,
  RegisterRequest,
  ResendVerificationRequest,
  ResetPasswordRequest,
  SessionSummary,
  VerifyEmailRequest,
} from '../types'

// Real browser traffic resolves its tenant from the Host header server-side
// (HostTenantResolutionStrategy) — nothing to send. VITE_DEV_TENANT_ID is a dev-only escape hatch
// for exercising the system tenant on a bare host (localhost) that resolves no tenant by itself;
// unset in any real deployment. Sending it alongside a host that DOES resolve a (different) tenant
// would trip the backend's spoofing check (TenantResolutionMiddleware rejects disagreeing signals),
// so this must stay opt-in, never a default.
const tenantHeader: Record<string, string> = env.devTenantId ? { [TENANT_HEADER]: env.devTenantId } : {}

// Thin, one-call-per-endpoint wrapper — matches the real `/api/v1/auth/*` contract exactly
// (AuthEndpoints.cs). No endpoint here that doesn't exist on the backend.
export const authApi = {
  register: (body: RegisterRequest) =>
    apiFetch<void>('/auth/register', {
      method: 'POST',
      body: JSON.stringify(body),
      headers: tenantHeader,
      skipAuth: true,
    }),

  login: (body: LoginRequest) =>
    apiFetch<AccessTokenResponse>('/auth/login', {
      method: 'POST',
      body: JSON.stringify(body),
      headers: tenantHeader,
      skipAuth: true,
    }),

  /** Cookie-based; the httpOnly refresh cookie travels automatically via `credentials: 'include'`. */
  refresh: () =>
    apiFetch<AccessTokenResponse>('/auth/refresh', {
      method: 'POST',
      headers: tenantHeader,
      skipAuth: true,
    }),

  logout: () => apiFetch<void>('/auth/logout', { method: 'POST' }),

  forgotPassword: (body: ForgotPasswordRequest) =>
    apiFetch<void>('/auth/forgot-password', {
      method: 'POST',
      body: JSON.stringify(body),
      headers: tenantHeader,
      skipAuth: true,
    }),

  resetPassword: (body: ResetPasswordRequest) =>
    apiFetch<void>('/auth/reset-password', {
      method: 'POST',
      body: JSON.stringify(body),
      headers: tenantHeader,
      skipAuth: true,
    }),

  verifyEmail: (body: VerifyEmailRequest) =>
    apiFetch<void>('/auth/verify-email', {
      method: 'POST',
      body: JSON.stringify(body),
      headers: tenantHeader,
      skipAuth: true,
    }),

  resendVerification: (body: ResendVerificationRequest) =>
    apiFetch<void>('/auth/resend-verification', {
      method: 'POST',
      body: JSON.stringify(body),
      headers: tenantHeader,
      skipAuth: true,
    }),

  getCurrentUser: () => apiFetch<CurrentUserResponse>('/auth/me'),

  listSessions: () => apiFetch<SessionSummary[]>('/auth/sessions'),

  revokeSession: (sessionId: string) =>
    apiFetch<void>(`/auth/sessions/${sessionId}`, { method: 'DELETE' }),
}
