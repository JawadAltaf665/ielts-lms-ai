import { Link } from 'react-router-dom'

import { CenteredAuthShell } from '../components/auth-shell'
import { ClockIcon } from '../components/icons'

export function SessionExpiredPage() {
  return (
    <CenteredAuthShell>
      <div style={{ textAlign: 'center' }}>
        <div className="verify-icon" style={{ marginInline: 'auto' }}>
          <ClockIcon />
        </div>
        <h1>Your session has expired</h1>
        <p className="lede">For your security, you've been signed out. Please sign in again to continue.</p>
        <Link to="/login" className="btn btn-primary btn-block">
          Sign in again
        </Link>
      </div>
    </CenteredAuthShell>
  )
}
