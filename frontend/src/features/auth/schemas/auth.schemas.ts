import { z } from 'zod'

import { PASSWORD_MIN_LENGTH } from '../auth.constants'

// Mirrors ConfigurablePasswordPolicy defaults (PasswordPolicyOptions): min 12, upper, lower,
// digit, symbol. Client-side is UX only — the backend remains authoritative (doc 16 §9).
const passwordPolicy = z
  .string()
  .min(PASSWORD_MIN_LENGTH, `Password must be at least ${PASSWORD_MIN_LENGTH} characters long.`)
  .regex(/[A-Z]/, 'Password must contain an uppercase letter.')
  .regex(/[a-z]/, 'Password must contain a lowercase letter.')
  .regex(/[0-9]/, 'Password must contain a digit.')
  .regex(/[^A-Za-z0-9]/, 'Password must contain a symbol.')

export const loginSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email address.'),
  password: z.string().min(1, 'Password is required.'),
})
export type LoginFormValues = z.infer<typeof loginSchema>

export const registerSchema = z.object({
  displayName: z.string().min(1, 'Display name is required.').max(256, 'Display name is too long.'),
  email: z.string().min(1, 'Email is required.').email('Enter a valid email address.'),
  password: passwordPolicy,
})
export type RegisterFormValues = z.infer<typeof registerSchema>

export const forgotPasswordSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email address.'),
})
export type ForgotPasswordFormValues = z.infer<typeof forgotPasswordSchema>

export const resetPasswordSchema = z.object({
  newPassword: passwordPolicy,
})
export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>

export const resendVerificationSchema = z.object({
  email: z.string().min(1, 'Email is required.').email('Enter a valid email address.'),
})
export type ResendVerificationFormValues = z.infer<typeof resendVerificationSchema>
