// Mirrors backend/src/IELTSLms.Api/Contracts/AuthRequests.cs and the Identity
// application-layer commands/queries exactly — no invented fields.

export interface RegisterRequest {
  email: string
  password: string
  displayName: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface ForgotPasswordRequest {
  email: string
}

export interface ResetPasswordRequest {
  token: string
  newPassword: string
}

export interface VerifyEmailRequest {
  token: string
}

export interface ResendVerificationRequest {
  email: string
}

/** Response body for login/refresh (`AccessTokenResponse`). Refresh token is cookie-only. */
export interface AccessTokenResponse {
  accessToken: string
  tokenType: 'Bearer'
  accessTokenExpiresAtUtc: string
  sessionId: string
}

/** `CurrentUserResponse` — the caller's own profile from `GET /auth/me`, including their effective
 * roles and the permissions those roles resolve to (backend Gap 2 — previously omitted). */
export interface CurrentUserResponse {
  userId: string
  tenantId: string
  email: string
  displayName: string
  status: string
  isEmailVerified: boolean
  createdAtUtc: string
  lastLoginAtUtc: string | null
  roles: string[]
  permissions: string[]
}

/** `SessionSummary` — one active device session from `GET /auth/sessions`. No `isCurrent` flag;
 * the frontend compares `sessionId` against the store's own session id (from login/refresh). */
export interface SessionSummary {
  sessionId: string
  deviceLabel: string | null
  ipAddress: string | null
  lastSeenAtUtc: string
  createdAtUtc: string
  expiresAtUtc: string
}
