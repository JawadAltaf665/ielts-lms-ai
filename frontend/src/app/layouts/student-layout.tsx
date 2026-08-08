import { NavLink, Outlet } from 'react-router-dom'

export function StudentLayout() {
  return (
    <div className="flex">
      <nav className="w-48 border-r p-4 text-sm">
        <NavLink to="/student" end>
          Dashboard
        </NavLink>
      </nav>
      <main className="flex-1 p-4">
        <Outlet />
      </main>
    </div>
  )
}
