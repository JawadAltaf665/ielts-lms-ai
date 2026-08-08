import { type ReactNode, useEffect } from 'react'

import { setSessionHandlers } from '@/shared/lib/api-client'
import { hadSession, useAuthStore } from '@/shared/lib/auth-store'

import { authApi, establishSession } from '@/features/auth'

export { useAuth } from '@/shared/lib/auth-store'

/**
 * Wires the framework-agnostic API client to the auth store (doc 16 §16): a shared refresh
 * function for 401 retry-once, and a session-expired callback that sends the user to the
 * dedicated "session expired" screen only if they were actually logged in before. Also performs
 * the silent-refresh boot check against the httpOnly cookie (doc 16 §7 sequence).
 */
export function AuthProvider({ children }: { children: ReactNode }) {
  useEffect(() => {
    setSessionHandlers({
      refresh: async () => {
        const tokens = await authApi.refresh()
        await establishSession(tokens)
        return tokens.accessToken
      },
      onExpired: () => {
        useAuthStore.getState().clear()
        if (hadSession) {
          window.location.assign('/session-expired')
        }
      },
    })

    authApi
      .refresh()
      .then(establishSession)
      .catch(() => useAuthStore.getState().clear())
  }, [])

  return children
}
