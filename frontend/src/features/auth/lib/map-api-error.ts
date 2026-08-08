import type { UseFormSetError, FieldValues, Path } from 'react-hook-form'

import { ApiError } from '@/shared/lib/api-client'

/**
 * Applies a failed auth call's RFC 7807 problem to a react-hook-form instance: field errors
 * (422 `errors[]`, `PropertyName` → camelCase) go to their inputs via `setError`; anything else
 * is returned as a form-level message for a `FormAlert`. Backend `code` (AuthErrors.cs) is
 * returned too so pages can branch on specific states (e.g. `AUTH_ACCOUNT_LOCKED`).
 */
export function applyApiError<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
): { message: string; code?: string; hasFieldErrors: boolean } {
  if (!(error instanceof ApiError)) {
    return { message: 'Something went wrong. Please try again.', hasFieldErrors: false }
  }

  const hasFieldErrors = Boolean(error.problem.errors?.length)
  for (const fieldError of error.problem.errors ?? []) {
    const field = (fieldError.field.charAt(0).toLowerCase() + fieldError.field.slice(1)) as Path<T>
    setError(field, { type: fieldError.code, message: fieldError.message })
  }

  return {
    message: error.problem.detail ?? 'Something went wrong. Please try again.',
    code: error.code,
    hasFieldErrors,
  }
}
