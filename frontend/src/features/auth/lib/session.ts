import { markHadSession, useAuthStore } from '@/shared/lib/auth-store'
import { mapBackendRoles, type AuthUser } from '@/shared/types/auth'

import { authApi } from '../api/auth-api'
import { SYSTEM_TENANT_ID } from '../auth.constants'
import type { AccessTokenResponse, CurrentUserResponse } from '../types'

function mapUser(response: CurrentUserResponse): AuthUser {
  return {
    id: response.userId,
    email: response.email,
    displayName: response.displayName,
    isEmailVerified: response.isEmailVerified,
    tenantId: response.tenantId,
    roles: mapBackendRoles(response.roles),
    permissions: response.permissions,
  }
}

/**
 * Turns a fresh token pair (from login or refresh) into a populated auth session: stash the
 * access token, fetch the caller's profile, and put both in the store. Shared by login, register
 * auto-login-equivalent (none — register doesn't log in, see RegisterCommandHandler), and the
 * silent-refresh boot check.
 */
export async function establishSession(tokens: AccessTokenResponse): Promise<AuthUser> {
  useAuthStore.getState().setSession(
    {
      id: '',
      email: '',
      displayName: '',
      isEmailVerified: false,
      tenantId: SYSTEM_TENANT_ID,
      roles: [],
      permissions: [],
    },
    tokens.accessToken,
    tokens.sessionId,
  )
  const user = mapUser(await authApi.getCurrentUser())
  useAuthStore.getState().setSession(user, tokens.accessToken, tokens.sessionId)
  markHadSession()
  return user
}
