import { NavLink, Outlet } from 'react-router-dom'

import { useAuth } from '@/app/providers/auth-provider'
import { hasRole } from '@/shared/types/auth'

// Shared by Super Admin / Tenant Admin / Institute Admin (doc 11 role
// hierarchy) — one layout, nav + page content gated by role, not three apps.
export function AdminLayout() {
  const { user } = useAuth()

  return (
    <div className="flex">
      <nav className="w-48 border-r p-4 text-sm flex flex-col gap-2">
        <NavLink to="/admin" end>
          Overview
        </NavLink>
        {hasRole(user, 'PlatformAdmin') && <NavLink to="/admin/platform">Platform</NavLink>}
        {hasRole(user, 'PlatformAdmin', 'TenantAdmin') && <NavLink to="/admin/tenant">Tenant</NavLink>}
        {hasRole(user, 'PlatformAdmin', 'TenantAdmin', 'InstituteAdmin') && (
          <NavLink to="/admin/institute">Institute</NavLink>
        )}
      </nav>
      <main className="flex-1 p-4">
        <Outlet />
      </main>
    </div>
  )
}
