import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { authApi } from '../api/auth-api'

const sessionsQueryKey = ['auth', 'sessions'] as const

export function useSessionsQuery() {
  return useQuery({ queryKey: sessionsQueryKey, queryFn: authApi.listSessions })
}

export function useRevokeSessionMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: authApi.revokeSession,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: sessionsQueryKey }),
  })
}
