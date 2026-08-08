// Scaffold-only placeholders so every route renders something. Each becomes
// a real page inside its feature module (doc 16 §3/§4) once that feature's
// work starts — delete the matching line here when it does.
import { Navigate } from 'react-router-dom'

import { useAuth } from '@/app/providers/auth-provider'
import { hasRole } from '@/shared/types/auth'

// "/" has no page of its own — send the visitor to login, or to their highest-ranked persona's home.
// A role-less authenticated user (e.g. Support Agent, or a fresh account ahead of the RBAC seeder
// completing) falls back to the neutral /dashboard rather than a 403 loop.
export function HomeRedirect() {
  const { isAuthenticated, user } = useAuth()
  if (!isAuthenticated) return <Navigate to="/login" replace />
  if (hasRole(user, 'PlatformAdmin', 'TenantAdmin', 'InstituteAdmin', 'ContentManager')) {
    return <Navigate to="/admin" replace />
  }
  if (hasRole(user, 'Teacher')) return <Navigate to="/teacher" replace />
  if (hasRole(user, 'Student')) return <Navigate to="/student" replace />
  return <Navigate to="/dashboard" replace />
}

export function DashboardPage() {
  const { user } = useAuth()
  return (
    <div className="p-8">
      <h1 className="text-xl font-semibold">Welcome{user?.displayName ? `, ${user.displayName}` : ''}</h1>
      <p className="text-sm text-slate-500">
        {user?.email}
        {user && !user.isEmailVerified ? ' — email not verified yet' : ''}
      </p>
    </div>
  )
}

export function StudentDashboardPage() {
  return <h1 className="text-xl font-semibold">Student Dashboard</h1>
}

export function TeacherDashboardPage() {
  return <h1 className="text-xl font-semibold">Teacher Portal</h1>
}

export function AdminOverviewPage() {
  return <h1 className="text-xl font-semibold">Admin Overview</h1>
}

export function ExamPlaceholderPage() {
  return <h1 className="p-8 text-xl font-semibold">Mock Exam</h1>
}

export function ForbiddenPage() {
  return <h1 className="p-8 text-xl font-semibold">403 — Forbidden</h1>
}

export function NotFoundPage() {
  return <h1 className="p-8 text-xl font-semibold">404 — Not Found</h1>
}
