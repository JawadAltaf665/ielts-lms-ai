import { useMutation } from '@tanstack/react-query'

import { authApi } from '../api/auth-api'

// Thin useMutation wrappers, one per endpoint — pages own loading/error/success UI (doc 16 §9:
// server errors are mapped per-form via applyApiError, not centralized here).
export const useLoginMutation = () => useMutation({ mutationFn: authApi.login })
export const useRegisterMutation = () => useMutation({ mutationFn: authApi.register })
export const useForgotPasswordMutation = () => useMutation({ mutationFn: authApi.forgotPassword })
export const useResetPasswordMutation = () => useMutation({ mutationFn: authApi.resetPassword })
export const useVerifyEmailMutation = () => useMutation({ mutationFn: authApi.verifyEmail })
export const useResendVerificationMutation = () => useMutation({ mutationFn: authApi.resendVerification })
