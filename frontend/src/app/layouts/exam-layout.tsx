import { Outlet } from 'react-router-dom'

// Distraction-free exam shell (doc 16 §14.2) — no nav, no chrome besides a
// timer slot. Timer/navigator/autosave land with the exam engine (M5.1).
export function ExamLayout() {
  return (
    <div className="min-h-screen">
      <Outlet />
    </div>
  )
}
