import { zodResolver } from '@hookform/resolvers/zod'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link } from 'react-router-dom'

import { CenteredAuthShell } from '../components/auth-shell'
import { Field } from '../components/field'
import { ChevronLeftIcon, MailIcon } from '../components/icons'
import { useForgotPasswordMutation } from '../hooks/use-auth-mutations'
import { forgotPasswordSchema, type ForgotPasswordFormValues } from '../schemas/auth.schemas'

export function ForgotPasswordPage() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordFormValues>({ resolver: zodResolver(forgotPasswordSchema) })
  const forgotPasswordMutation = useForgotPasswordMutation()
  const [sent, setSent] = useState(false)

  async function onSubmit(values: ForgotPasswordFormValues) {
    // ForgotPasswordCommandHandler responds uniformly whether or not the email exists
    // (enumeration resistance, Auth §5) — there is no "email not found" state to handle.
    await forgotPasswordMutation.mutateAsync(values)
    setSent(true)
  }

  if (sent) {
    return (
      <CenteredAuthShell>
        <div className="verify-icon">
          <MailIcon width={26} height={26} className="" />
        </div>
        <h1>Check your email</h1>
        <p className="lede">
          If an account exists for that email, you'll receive a password-reset link shortly. The link
          expires in 1 hour.
        </p>
        <button type="button" className="btn btn-secondary btn-block" onClick={() => setSent(false)}>
          Use a different email
        </button>
      </CenteredAuthShell>
    )
  }

  return (
    <CenteredAuthShell>
      <Link to="/login" className="back-link">
        <ChevronLeftIcon />
        Back to log in
      </Link>
      <h1>Reset your password</h1>
      <p className="lede">Enter the email associated with your account and we'll send a reset link.</p>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <Field
          id="forgot-email"
          label="Email"
          icon={<MailIcon />}
          type="email"
          placeholder="you@email.com"
          autoComplete="email"
          error={errors.email?.message}
          {...register('email')}
        />
        <button type="submit" className="btn btn-primary btn-block" disabled={isSubmitting}>
          {isSubmitting ? 'Sending…' : 'Send reset link'}
        </button>
      </form>
    </CenteredAuthShell>
  )
}
