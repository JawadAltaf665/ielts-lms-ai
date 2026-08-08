// Mirrors the role hierarchy in docs/11_Authentication_Authorization.md §11.2. The backend remains
// the authorization authority (doc 16 §7); these gate UI only — permission checks are enforced
// server-side (AuthorizationPipelineBehavior), never trusted here as a security boundary.
export type Role =
  | 'PlatformAdmin' // Platform Super Admin — cross-tenant, audited
  | 'TenantAdmin'
  | 'InstituteAdmin'
  | 'ContentManager'
  | 'Teacher'
  | 'Student'
  | 'SupportAgent'

// Backend role names (RbacCatalog.cs — spaced display names) → the PascalCase tokens above.
// "PlatformAdmin" backend-side has no space (a pre-existing inconsistency preserved from the
// codebase's own Billing RequiredRoles checks) so it round-trips unchanged.
const BACKEND_ROLE_TO_TOKEN: Record<string, Role> = {
  PlatformAdmin: 'PlatformAdmin',
  'Tenant Admin': 'TenantAdmin',
  'Institute Admin': 'InstituteAdmin',
  'Content Manager': 'ContentManager',
  Teacher: 'Teacher',
  Student: 'Student',
  'Support Agent': 'SupportAgent',
}

/** Maps `/auth/me` and JWT role-name strings (e.g. "Institute Admin") to the frontend's `Role` tokens. */
export function mapBackendRoles(roleNames: string[]): Role[] {
  return roleNames
    .map((name) => BACKEND_ROLE_TO_TOKEN[name])
    .filter((role): role is Role => role !== undefined)
}

export interface AuthUser {
  id: string
  email: string
  displayName: string
  isEmailVerified: boolean
  tenantId: string
  roles: Role[]
  /** Permission keys the user's roles grant (Auth §9.2) — UI convenience only, e.g. hiding an action
   * the backend would reject anyway. Never the actual authorization boundary. */
  permissions: string[]
}

export function hasRole(user: AuthUser | null, ...allowed: Role[]): boolean {
  if (!user) return false
  return user.roles.some((role) => allowed.includes(role))
}

export function hasPermission(user: AuthUser | null, permission: string): boolean {
  if (!user) return false
  return user.permissions.includes(permission)
}
