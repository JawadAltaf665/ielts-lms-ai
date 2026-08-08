import { useEffect, useState } from 'react'
import { Link, useLocation, useSearchParams } from 'react-router-dom'

import { CenteredAuthShell } from '../components/auth-shell'
import { CheckIcon, MailIcon, RefreshIcon } from '../components/icons'
import { FormAlert } from '../components/form-alert'
import {
  useResendVerificationMutation,
  useVerifyEmailMutation,
} from '../hooks/use-auth-mutations'

type ViewState = 'pending' | 'verifying' | 'success' | 'invalid'

export function VerifyEmailPage() {
  const [searchParams] = useSearchParams()
  const location = useLocation()
  const token = searchParams.get('token')
  const knownEmail = (location.state as { email?: string } | null)?.email

  const verifyEmailMutation = useVerifyEmailMutation()
  const resendVerificationMutation = useResendVerificationMutation()
  const [state, setState] = useState<ViewState>(token ? 'verifying' : 'pending')
  const [resendEmail, setResendEmail] = useState(knownEmail ?? '')
  const [resent, setResent] = useState(false)

  useEffect(() => {
    if (!token) return
    verifyEmailMutation
      .mutateAsync({ token })
      .then(() => setState('success'))
      .catch(() => setState('invalid'))
    // Auto-verify runs once for the token in the URL, on mount.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token])

  async function handleResend() {
    if (!resendEmail) return
    setResent(false)
    // ResendVerificationCommandHandler responds uniformly whether or not the email exists/is
    // already verified (enumeration resistance, Auth §5).
    await resendVerificationMutation.mutateAsync({ email: resendEmail })
    setResent(true)
  }

  if (state === 'verifying') {
    return (
      <CenteredAuthShell>
        <div style={{ textAlign: 'center' }}>
          <div className="verify-icon" style={{ marginInline: 'auto' }}>
            <RefreshIcon />
          </div>
          <h1>Verifying your email…</h1>
          <p className="lede">Hold on while we confirm your verification link.</p>
        </div>
      </CenteredAuthShell>
    )
  }

  if (state === 'success') {
    return (
      <CenteredAuthShell>
        <div style={{ textAlign: 'center' }}>
          <div className="verify-icon" style={{ marginInline: 'auto' }}>
            <CheckIcon width={26} height={26} />
          </div>
          <h1>Email verified</h1>
          <p className="lede">Your account is active. You can now log in.</p>
          <Link to="/login" className="btn btn-primary btn-block">
            Go to log in
          </Link>
        </div>
      </CenteredAuthShell>
    )
  }

  if (state === 'invalid') {
    return (
      <CenteredAuthShell>
        <div style={{ textAlign: 'center' }}>
          <FormAlert>The token is invalid or has expired.</FormAlert>
          <h1>This link no longer works</h1>
          <p className="lede">Verification links are single-use and expire after 24 hours. Enter your email to get a new one.</p>
          <div className="resendmini">
            <input
              className="input"
              type="email"
              placeholder="you@email.com"
              value={resendEmail}
              onChange={(e) => setResendEmail(e.target.value)}
              aria-label="Email"
            />
            <button type="button" className="btn btn-primary" onClick={handleResend} disabled={!resendEmail}>
              Resend
            </button>
          </div>
          {resent ? <p className="field-help">If that account exists, a new link is on its way.</p> : null}
        </div>
      </CenteredAuthShell>
    )
  }

  return (
    <CenteredAuthShell>
      <div style={{ textAlign: 'center' }}>
        <div className="verify-icon" style={{ marginInline: 'auto' }}>
          <MailIcon width={26} height={26} className="" />
        </div>
        <h1>Verify your email</h1>
        {knownEmail ? (
          <p className="lede">
            We sent a verification link to <strong>{knownEmail}</strong>. Click the link to activate your
            account — you can't log in until it's verified.
          </p>
        ) : (
          <p className="lede">
            Check your inbox for the verification link we sent when you registered. Enter your email below to
            get a new one.
          </p>
        )}
        {!knownEmail ? (
          <div className="resendmini">
            <input
              className="input"
              type="email"
              placeholder="you@email.com"
              value={resendEmail}
              onChange={(e) => setResendEmail(e.target.value)}
              aria-label="Email"
            />
            <button type="button" className="btn btn-primary" onClick={handleResend} disabled={!resendEmail}>
              Resend
            </button>
          </div>
        ) : (
          <button type="button" className="btn btn-secondary btn-block" onClick={handleResend}>
            Resend email
          </button>
        )}
        {resent ? <p className="field-help">If that account exists, a new link is on its way.</p> : null}
        <p className="field-help" style={{ marginTop: 14 }}>
          Didn't get it?{' '}
          <Link to="/register" className="link-accent">
            Use a different email
          </Link>
          .
        </p>
        <div className="foot-row">
          Already verified?{' '}
          <Link to="/login" className="link-accent">
            Log in
          </Link>
        </div>
      </div>
    </CenteredAuthShell>
  )
}
