export { LoginPage } from './pages/login-page'
export { RegisterPage } from './pages/register-page'
export { ForgotPasswordPage } from './pages/forgot-password-page'
export { ResetPasswordPage } from './pages/reset-password-page'
export { VerifyEmailPage } from './pages/verify-email-page'
export { SessionExpiredPage } from './pages/session-expired-page'
export { SessionsPage } from './pages/sessions-page'

// Composition-root surface (app/providers, app/layouts wire these — doc 16 §22 barrel rule).
export { authApi } from './api/auth-api'
export { establishSession } from './lib/session'
export { useLogout } from './hooks/use-logout'
