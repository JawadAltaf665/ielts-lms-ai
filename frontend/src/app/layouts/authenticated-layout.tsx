import { Link, Outlet } from 'react-router-dom'

import { useAuth } from '@/app/providers/auth-provider'
import { useLogout } from '@/features/auth'

// Shared shell under RequireAuth (doc 16 §6) — persona layouts (student/
// teacher/admin) render inside via <Outlet/>. Branding/tenant chrome goes
// here once tenant theming (doc 12 §8) is wired.
export function AuthenticatedLayout() {
  const { user } = useAuth()
  const logout = useLogout()

  return (
    <div className="min-h-screen">
      <header className="flex items-center justify-between border-b px-4 py-3">
        <span className="font-semibold">IELTS LMS AI</span>
        <div className="flex items-center gap-3 text-sm">
          <span>{user?.email}</span>
          <Link to="/account/sessions" className="underline">
            Devices
          </Link>
          <button onClick={logout} className="underline">
            Log out
          </button>
        </div>
      </header>
      <Outlet />
    </div>
  )
}
