import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'

import { SplitAuthShell } from '../components/auth-shell'
import { BrandPane } from '../components/brand-pane'
import { Field } from '../components/field'
import { FormAlert } from '../components/form-alert'
import { BookIcon, GraduationCapIcon, MailIcon, SearchIcon, UserIcon, UsersIcon } from '../components/icons'
import { PasswordField } from '../components/password-field'
import { PasswordRequirements } from '../components/password-requirements'
import { useRegisterMutation } from '../hooks/use-auth-mutations'
import { applyApiError } from '../lib/map-api-error'
import { registerSchema, type RegisterFormValues } from '../schemas/auth.schemas'

const features = [
  { icon: <SearchIcon />, text: 'Placement-based learning path from day one' },
  { icon: <BookIcon />, text: 'Grammar, vocabulary & sentence-builder foundations' },
  { icon: <UsersIcon />, text: 'Academic & General Training tracks supported' },
]

export function RegisterPage() {
  const navigate = useNavigate()
  const {
    register,
    handleSubmit,
    setError,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<RegisterFormValues>({ resolver: zodResolver(registerSchema) })
  const registerMutation = useRegisterMutation()
  const [alert, setAlert] = useState<string | null>(null)
  const password = watch('password') ?? ''

  async function onSubmit(values: RegisterFormValues) {
    setAlert(null)
    try {
      // 202 Accepted, no tokens — RegisterCommandHandler never logs the caller in; it only sends
      // a verification email. The account can't sign in until it's verified.
      await registerMutation.mutateAsync(values)
      navigate('/verify-email', { state: { email: values.email }, replace: true })
    } catch (error) {
      const { message, hasFieldErrors } = applyApiError<RegisterFormValues>(error, setError)
      if (!hasFieldErrors) {
        setAlert(message)
      }
    }
  }

  return (
    <SplitAuthShell>
      <BrandPane
        heading="Start on the Free plan. Upgrade whenever you're ready."
        subtitle="Personalized learning path, daily practice, and your first AI-evaluated submissions — no card required."
        features={features}
        footer="Individual, institute and enterprise plans available."
      />
      <div className="formpane">
        <div className="authcard">
          <h1>Create your account</h1>
          <p className="lede">Start your personalized path to your target band.</p>

          {alert ? <FormAlert>{alert}</FormAlert> : null}

          <form onSubmit={handleSubmit(onSubmit)} noValidate>
            <Field
              id="register-name"
              label="Display name"
              icon={<UserIcon />}
              type="text"
              placeholder="Alex Morgan"
              autoComplete="name"
              error={errors.displayName?.message}
              {...register('displayName')}
            />
            <Field
              id="register-email"
              label="Email"
              icon={<MailIcon />}
              type="email"
              placeholder="you@email.com"
              autoComplete="email"
              error={errors.email?.message}
              {...register('email')}
            />
            <div>
              <PasswordField
                id="register-password"
                label="Password"
                placeholder="••••••••••••"
                autoComplete="new-password"
                error={errors.password?.message}
                {...register('password')}
              />
              <PasswordRequirements password={password} />
            </div>
            <button type="submit" className="btn btn-primary btn-block" disabled={isSubmitting}>
              {isSubmitting ? 'Creating account…' : 'Create account'}
            </button>
          </form>

          <div className="note-card">
            <GraduationCapIcon />
            <span>
              You'll start on the <strong>Free plan</strong> — upgrade to Starter or Premium anytime.
            </span>
          </div>
          <div className="foot-row" style={{ marginTop: 0 }}>
            Joining an institute? Ask your admin for an invite link instead.
          </div>
          <div className="foot-row">
            Already have an account?{' '}
            <Link to="/login" className="link-accent">
              Log in
            </Link>
          </div>
        </div>
      </div>
    </SplitAuthShell>
  )
}
