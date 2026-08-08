import type { ReactNode } from 'react'

import '../auth.css'

/** Split layout for login/register: brand panel + form, matching `.split-auth`. */
export function SplitAuthShell({ children }: { children: ReactNode }) {
  return (
    <div className="auth-shell">
      <div className="split-auth">{children}</div>
    </div>
  )
}

/** Centered layout for forgot/reset/verify/expired, matching `.centered-auth`. */
export function CenteredAuthShell({ children }: { children: ReactNode }) {
  return (
    <div className="auth-shell">
      <div className="centered-auth">
        <div className="centered-card">
          <div className="authcard">{children}</div>
        </div>
      </div>
    </div>
  )
}
