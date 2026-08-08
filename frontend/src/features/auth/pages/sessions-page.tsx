import { useState } from 'react'

import { useAuthStore } from '@/shared/lib/auth-store'

import { LogOutIcon, MonitorIcon } from '../components/icons'
import { useRevokeSessionMutation, useSessionsQuery } from '../hooks/use-sessions'
import { useLogout } from '../hooks/use-logout'

function formatDate(iso: string): string {
  return new Date(iso).toLocaleString(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  })
}

/**
 * "Your devices" — lists active sessions (`GET /auth/sessions`) and lets the caller revoke any of
 * them (`DELETE /auth/sessions/{id}`). No approved design screen covers this; built with the same
 * tokens/typography/component classes as the six auth screens (auth.css — see its "Sessions" section).
 */
export function SessionsPage() {
  const currentSessionId = useAuthStore((s) => s.sessionId)
  const sessionsQuery = useSessionsQuery()
  const revokeMutation = useRevokeSessionMutation()
  const logout = useLogout()
  const [revokingId, setRevokingId] = useState<string | null>(null)

  async function handleRevoke(sessionId: string) {
    setRevokingId(sessionId)
    try {
      await revokeMutation.mutateAsync(sessionId)
      // The session's refresh-token family is now dead server-side; revoking our own session
      // means this device is effectively logged out too.
      if (sessionId === currentSessionId) {
        await logout()
      }
    } finally {
      setRevokingId(null)
    }
  }

  return (
    <div className="auth-shell">
      <div className="session-page">
        <h1>Your devices</h1>
        <p className="lede">
          These are the devices currently signed in to your account. Revoke any you don't recognize.
        </p>

        {sessionsQuery.isPending ? (
          <p className="session-empty">Loading…</p>
        ) : sessionsQuery.isError ? (
          <p className="session-empty">Couldn't load your sessions. Try refreshing the page.</p>
        ) : sessionsQuery.data.length === 0 ? (
          <p className="session-empty">No active sessions.</p>
        ) : (
          <ul className="session-list">
            {sessionsQuery.data.map((session) => {
              const isCurrent = session.sessionId === currentSessionId
              return (
                <li key={session.sessionId} className="session-row">
                  <div className="session-row-info">
                    <span className="session-device">
                      <MonitorIcon />
                      {session.deviceLabel ?? 'Unknown device'}
                      {isCurrent ? <span className="session-badge">This device</span> : null}
                    </span>
                    <span className="session-meta">
                      {session.ipAddress ?? 'IP unknown'} · last active {formatDate(session.lastSeenAtUtc)}
                    </span>
                  </div>
                  <div className="session-actions">
                    <button
                      type="button"
                      className="btn btn-secondary btn-danger-text"
                      onClick={() => handleRevoke(session.sessionId)}
                      disabled={revokingId === session.sessionId}
                    >
                      <LogOutIcon />
                      {isCurrent ? 'Log out' : revokingId === session.sessionId ? 'Revoking…' : 'Revoke'}
                    </button>
                  </div>
                </li>
              )
            })}
          </ul>
        )}
      </div>
    </div>
  )
}
