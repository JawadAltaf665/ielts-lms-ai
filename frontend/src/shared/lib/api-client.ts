import { env } from '@/shared/config/env'

/**
 * RFC 7807 problem+json shape returned by every failed API call (API Design
 * Standards §9). `code` is the stable machine error code (AUTH_*, VALIDATION_*…);
 * `errors` is populated only for 422 field-validation failures.
 */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  code?: string
  errors?: { field: string; code: string; message: string }[]
}

/** Thrown by {@link apiFetch} for any non-2xx response. */
export class ApiError extends Error {
  readonly status: number
  readonly problem: ProblemDetails

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail ?? problem.title ?? `API error ${status}`)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }

  /** The stable machine code (e.g. `AUTH_INVALID_CREDENTIALS`), if the API sent one. */
  get code(): string | undefined {
    return this.problem.code
  }
}

/** The envelope every successful response is wrapped in (API Design Standards §8.1). */
interface ApiEnvelope<T> {
  data: T
  meta?: unknown
}

// Access token is attached by the auth feature via this setter, avoiding a
// circular import between the framework-agnostic API client and the Zustand
// auth store (doc 16 §7 — token lives in memory, never localStorage).
let accessToken: string | null = null
export function setAccessToken(token: string | null): void {
  accessToken = token
}

// Registered once by AuthProvider (doc 16 §16 — silent refresh + retry-once on 401, single
// in-flight refresh so concurrent 401s don't each fire their own refresh request).
let refreshHandler: (() => Promise<string>) | null = null
let onSessionExpired: (() => void) | null = null
let inFlightRefresh: Promise<string> | null = null

export function setSessionHandlers(handlers: {
  refresh: () => Promise<string>
  onExpired: () => void
}): void {
  refreshHandler = handlers.refresh
  onSessionExpired = handlers.onExpired
}

interface ApiFetchOptions extends RequestInit {
  /** Skip the Authorization header and the 401 refresh-retry (login/register/refresh itself). */
  skipAuth?: boolean
  /** Internal: set on the retried call so a second 401 fails instead of looping. */
  _retried?: boolean
}

/**
 * Fetch wrapper for the IELTS LMS API: base URL + tenant/auth headers, the
 * `{data}` envelope unwrapped, and RFC 7807 failures raised as {@link ApiError}.
 * `credentials: 'include'` is required so the httpOnly refresh-token cookie
 * (Auth §3.3) is sent with every request.
 */
export async function apiFetch<T>(path: string, options: ApiFetchOptions = {}): Promise<T> {
  const { skipAuth, _retried, headers, ...init } = options

  const res = await fetch(`${env.apiBaseUrl}/api/v1${path}`, {
    ...init,
    credentials: 'include',
    headers: {
      'Content-Type': 'application/json',
      ...(!skipAuth && accessToken ? { Authorization: `Bearer ${accessToken}` } : {}),
      ...headers,
    },
  })

  if (res.status === 401 && !skipAuth && !_retried && refreshHandler) {
    try {
      inFlightRefresh ??= refreshHandler().finally(() => {
        inFlightRefresh = null
      })
      await inFlightRefresh
      return apiFetch<T>(path, { ...options, _retried: true })
    } catch {
      onSessionExpired?.()
      // Fall through to the original 401 handling below.
    }
  }

  if (res.status === 204) {
    return undefined as T
  }

  const body = await res.json().catch(() => null)

  if (!res.ok) {
    throw new ApiError(res.status, (body as ProblemDetails) ?? { status: res.status })
  }

  return (body as ApiEnvelope<T>).data
}
