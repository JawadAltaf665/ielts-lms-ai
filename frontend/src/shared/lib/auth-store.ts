import { create } from 'zustand'

import { setAccessToken } from '@/shared/lib/api-client'
import type { AuthUser } from '@/shared/types/auth'

// Auth is boot-time async (silent refresh against the httpOnly cookie), so 'idle' — not yet
// checked — is distinct from 'unauthenticated' — checked, no session. Guards (guards.tsx) treat
// 'idle' as "wait", everything else as a definite answer.
export type AuthStatus = 'idle' | 'authenticated' | 'unauthenticated'

interface AuthState {
  status: AuthStatus
  user: AuthUser | null
  accessToken: string | null
  /** The `sid` claim of the current access token (`AccessTokenResponse.sessionId`) — lets the
   * sessions page mark "this device" without the API exposing an `isCurrent` flag. */
  sessionId: string | null
  isAuthenticated: boolean
  setSession: (user: AuthUser, accessToken: string, sessionId: string) => void
  clear: (status?: AuthStatus) => void
}

// Session lives in a Zustand store, not React Context — doc 16 §8 treats auth/tenant as client
// UI state. Lives in shared/, not app/providers/, so features/* can update it from mutations
// (login, register) without importing "up" into app/ (doc 16 §22 import rule).
export const useAuthStore = create<AuthState>((set) => ({
  status: 'idle',
  user: null,
  accessToken: null,
  sessionId: null,
  isAuthenticated: false,
  setSession: (user, accessToken, sessionId) => {
    setAccessToken(accessToken)
    set({ status: 'authenticated', user, accessToken, sessionId, isAuthenticated: true })
  },
  clear: (status = 'unauthenticated') => {
    setAccessToken(null)
    set({ status, user: null, accessToken: null, sessionId: null, isAuthenticated: false })
  },
}))

export function useAuth() {
  return useAuthStore()
}

/** Was there ever an authenticated session this page load? Distinguishes "expired" from "never logged in". */
export let hadSession = false
export function markHadSession(): void {
  hadSession = true
}
