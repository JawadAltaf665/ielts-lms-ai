import { createBrowserRouter } from 'react-router-dom'

import { AdminLayout } from '@/app/layouts/admin-layout'
import { AuthenticatedLayout } from '@/app/layouts/authenticated-layout'
import { ExamLayout } from '@/app/layouts/exam-layout'
import { PublicLayout } from '@/app/layouts/public-layout'
import { StudentLayout } from '@/app/layouts/student-layout'
import { TeacherLayout } from '@/app/layouts/teacher-layout'
import {
  AdminOverviewPage,
  DashboardPage,
  ExamPlaceholderPage,
  ForbiddenPage,
  HomeRedirect,
  NotFoundPage,
  StudentDashboardPage,
  TeacherDashboardPage,
} from '@/app/router/placeholder-pages'
import { RequireAuth, RequireGuest, RequireRole } from '@/app/router/guards'

import {
  ForgotPasswordPage,
  LoginPage,
  RegisterPage,
  ResetPasswordPage,
  SessionExpiredPage,
  SessionsPage,
  VerifyEmailPage,
} from '@/features/auth'

// Mirrors doc 16 §6's route tree: public → authenticated shell → persona
// layout (student/teacher/admin), plus a separate distraction-free exam
// layout. RequireRole gates each persona branch; RequireAuth gates all of it.
export const router = createBrowserRouter([
  { path: '/', element: <HomeRedirect /> },
  {
    element: <PublicLayout />,
    children: [
      {
        element: <RequireGuest />,
        children: [
          { path: '/login', element: <LoginPage /> },
          { path: '/register', element: <RegisterPage /> },
        ],
      },
      // Not guest-gated: an already-authenticated user may legitimately reset a password,
      // re-verify, or land here after a refresh failure.
      { path: '/forgot-password', element: <ForgotPasswordPage /> },
      { path: '/reset-password', element: <ResetPasswordPage /> },
      { path: '/verify-email', element: <VerifyEmailPage /> },
      { path: '/session-expired', element: <SessionExpiredPage /> },
    ],
  },
  {
    element: <RequireAuth />,
    children: [
      {
        element: <AuthenticatedLayout />,
        children: [
          { path: '/dashboard', element: <DashboardPage /> },
          { path: '/account/sessions', element: <SessionsPage /> },
          {
            element: <RequireRole allow={['Student']} />,
            children: [
              {
                path: '/student',
                element: <StudentLayout />,
                children: [{ index: true, element: <StudentDashboardPage /> }],
              },
            ],
          },
          {
            element: <RequireRole allow={['Teacher']} />,
            children: [
              {
                path: '/teacher',
                element: <TeacherLayout />,
                children: [{ index: true, element: <TeacherDashboardPage /> }],
              },
            ],
          },
          {
            element: (
              <RequireRole
                allow={['PlatformAdmin', 'TenantAdmin', 'InstituteAdmin', 'ContentManager']}
              />
            ),
            children: [
              {
                path: '/admin',
                element: <AdminLayout />,
                children: [{ index: true, element: <AdminOverviewPage /> }],
              },
            ],
          },
        ],
      },
      {
        path: '/exam/:examId',
        element: <ExamLayout />,
        children: [{ index: true, element: <ExamPlaceholderPage /> }],
      },
    ],
  },
  { path: '/403', element: <ForbiddenPage /> },
  { path: '*', element: <NotFoundPage /> },
])
