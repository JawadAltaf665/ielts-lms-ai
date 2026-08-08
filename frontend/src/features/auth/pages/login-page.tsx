import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useLocation, useNavigate } from 'react-router-dom'

import { SplitAuthShell } from '../components/auth-shell'
import { BrandPane } from '../components/brand-pane'
import { Field } from '../components/field'
import { FormAlert } from '../components/form-alert'
import {
  ArrowRightIcon,
  AwardIcon,
  GraduationCapIcon,
  LayoutGridIcon,
  LockClosedAlertIcon,
  MailIcon,
} from '../components/icons'
import { PasswordField } from '../components/password-field'
import { useLoginMutation } from '../hooks/use-auth-mutations'
import { establishSession } from '../lib/session'
import { applyApiError } from '../lib/map-api-error'
import { loginSchema, type LoginFormValues } from '../schemas/auth.schemas'

const features = [
  { icon: <GraduationCapIcon />, text: 'AI evaluation aligned to official band descriptors' },
  { icon: <LayoutGridIcon />, text: 'Computer-based mock exams that mirror the real exam' },
  { icon: <AwardIcon />, text: 'Progress tracking, band estimation & certificates' },
]

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) })
  const loginMutation = useLoginMutation()
  const [alert, setAlert] = useState<{ message: string; locked: boolean } | null>(null)

  async function onSubmit(values: LoginFormValues) {
    setAlert(null)
    try {
      const tokens = await loginMutation.mutateAsync(values)
      await establishSession(tokens)
      // "/" is HomeRedirect — routes by role (Student/Teacher/admin roles/fallback), so login
      // doesn't need to duplicate that logic.
      navigate(from ?? '/', { replace: true })
    } catch (error) {
      const { message, code, hasFieldErrors } = applyApiError<LoginFormValues>(error, setError)
      if (!hasFieldErrors) {
        setAlert({ message, locked: code === 'AUTH_ACCOUNT_LOCKED' })
      }
    }
  }

  return (
    <SplitAuthShell>
      <BrandPane
        heading="Prepare with confidence. Score with proof."
        subtitle="AI-evaluated Writing & Speaking, realistic CB-IELTS mock exams, and a learning path built around your target band."
        features={features}
        footer="Trusted by individual learners, institutes and universities worldwide."
      />
      <div className="formpane">
        <div className="authcard">
          <h1>Log in to your account</h1>
          <p className="lede">Continue your IELTS preparation.</p>

          {alert ? (
            <FormAlert icon={alert.locked ? <LockClosedAlertIcon /> : undefined}>{alert.message}</FormAlert>
          ) : null}

          <form onSubmit={handleSubmit(onSubmit)} noValidate>
            <Field
              id="login-email"
              label="Email"
              icon={<MailIcon />}
              type="email"
              placeholder="you@institute.edu"
              autoComplete="email"
              error={errors.email?.message}
              {...register('email')}
            />
            <PasswordField
              id="login-password"
              label="Password"
              placeholder="••••••••••••"
              autoComplete="current-password"
              error={errors.password?.message}
              {...register('password')}
            />
            <div className="rowline">
              <Link to="/forgot-password" className="link-accent">
                Forgot password?
              </Link>
            </div>
            <button type="submit" className="btn btn-primary btn-block" disabled={isSubmitting}>
              {isSubmitting ? 'Signing in…' : 'Log in'}
              <ArrowRightIcon />
            </button>
          </form>

          <div className="foot-row">
            New to IELTS LMS AI?{' '}
            <Link to="/register" className="link-accent">
              Create an account
            </Link>
          </div>
        </div>
      </div>
    </SplitAuthShell>
  )
}
