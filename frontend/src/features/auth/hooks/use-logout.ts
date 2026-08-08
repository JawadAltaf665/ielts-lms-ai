import { useNavigate } from 'react-router-dom'

import { useAuthStore } from '@/shared/lib/auth-store'

import { authApi } from '../api/auth-api'

/** Ends the session: revokes the refresh-token family server-side (LogoutCommand), then clears local state. */
export function useLogout() {
  const navigate = useNavigate()

  return async function logout() {
    try {
      await authApi.logout()
    } finally {
      useAuthStore.getState().clear()
      navigate('/login', { replace: true })
    }
  }
}
