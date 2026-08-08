import { NavLink, Outlet } from 'react-router-dom'

export function TeacherLayout() {
  return (
    <div className="flex">
      <nav className="w-48 border-r p-4 text-sm">
        <NavLink to="/teacher" end>
          Dashboard
        </NavLink>
      </nav>
      <main className="flex-1 p-4">
        <Outlet />
      </main>
    </div>
  )
}
