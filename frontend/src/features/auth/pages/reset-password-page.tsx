import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useSearchParams } from 'react-router-dom'

import { CenteredAuthShell } from '../components/auth-shell'
import { AlertCircleIcon, CheckIcon } from '../components/icons'
import { FormAlert } from '../components/form-alert'
import { PasswordField } from '../components/password-field'
import { PasswordRequirements } from '../components/password-requirements'
import { useResetPasswordMutation } from '../hooks/use-auth-mutations'
import { applyApiError } from '../lib/map-api-error'
import { resetPasswordSchema, type ResetPasswordFormValues } from '../schemas/auth.schemas'

type ViewState = 'idle' | 'invalid' | 'success'

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')

  const {
    register,
    handleSubmit,
    watch,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordFormValues>({ resolver: zodResolver(resetPasswordSchema) })
  const resetPasswordMutation = useResetPasswordMutation()
  const [state, setState] = useState<ViewState>(token ? 'idle' : 'invalid')
  const [alert, setAlert] = useState<string | null>(null)
  const newPassword = watch('newPassword') ?? ''

  async function onSubmit(values: ResetPasswordFormValues) {
    if (!token) return
    setAlert(null)
    try {
      await resetPasswordMutation.mutateAsync({ token, newPassword: values.newPassword })
      setState('success')
    } catch (error) {
      const { message, code, hasFieldErrors } = applyApiError<ResetPasswordFormValues>(error, setError)
      if (code === 'AUTH_INVALID_TOKEN') {
        setState('invalid')
      } else if (!hasFieldErrors) {
        setAlert(message)
      }
    }
  }

  if (state === 'invalid') {
    return (
      <CenteredAuthShell>
        <FormAlert>The token is invalid or has expired.</FormAlert>
        <h1>This reset link no longer works</h1>
        <p className="lede">Reset links are single-use and expire after 1 hour. Request a new one to continue.</p>
        <Link to="/forgot-password" className="btn btn-primary btn-block">
          Request a new link
        </Link>
      </CenteredAuthShell>
    )
  }

  if (state === 'success') {
    return (
      <CenteredAuthShell>
        <div className="verify-icon">
          <CheckIcon width={26} height={26} />
        </div>
        <h1>Password reset</h1>
        <p className="lede">
          Your password has been changed and every other session has been signed out. Please sign in again.
        </p>
        <Link to="/login" className="btn btn-primary btn-block">
          Go to log in
        </Link>
      </CenteredAuthShell>
    )
  }

  return (
    <CenteredAuthShell>
      <h1>Choose a new password</h1>
      <p className="lede">Enter a new password for your account.</p>
      {alert ? (
        <FormAlert icon={<AlertCircleIcon />}>{alert}</FormAlert>
      ) : null}
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <div>
          <PasswordField
            id="reset-password"
            label="New password"
            placeholder="••••••••••••"
            autoComplete="new-password"
            error={errors.newPassword?.message}
            {...register('newPassword')}
          />
          <PasswordRequirements password={newPassword} />
        </div>
        <button type="submit" className="btn btn-primary btn-block" disabled={isSubmitting}>
          {isSubmitting ? 'Resetting…' : 'Reset password'}
        </button>
      </form>
      <p className="field-help" style={{ marginTop: 14, textAlign: 'center' }}>
        This will sign you out of every other device.
      </p>
    </CenteredAuthShell>
  )
}
