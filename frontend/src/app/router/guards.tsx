import { Navigate, Outlet, useLocation } from 'react-router-dom'

import { useAuth } from '@/app/providers/auth-provider'
import { hasRole, type Role } from '@/shared/types/auth'

// Doc 16 §7 + §6: route guards check auth, then role/permission, before
// rendering. The backend is still the authorization authority — this is UX,
// not the security boundary (doc 16 §18).
export function RequireAuth() {
  const { status, isAuthenticated } = useAuth()
  const location = useLocation()

  // 'idle' = the silent-refresh boot check (auth-provider.tsx) hasn't resolved yet — wait rather
  // than bouncing to /login and back for a session that's about to prove valid.
  if (status === 'idle') {
    return null
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }
  return <Outlet />
}

export function RequireGuest() {
  const { status, isAuthenticated } = useAuth()

  if (status === 'idle') {
    return null
  }

  if (isAuthenticated) {
    return <Navigate to="/" replace />
  }
  return <Outlet />
}

export function RequireRole({ allow }: { allow: Role[] }) {
  const { user } = useAuth()

  if (!hasRole(user, ...allow)) {
    return <Navigate to="/403" replace />
  }
  return <Outlet />
}
