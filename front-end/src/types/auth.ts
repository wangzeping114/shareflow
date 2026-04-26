export type UserRole = 'SuperAdmin' | 'BackendCustom' | 'Sales' | 'Client'

export interface UserInfo {
  id: string
  email: string
  displayName: string
  role: UserRole
  roleName: string
  permissions: string[]
}

export interface LoginRequest {
  username: string
  password: string
}

export interface RefreshTokenRequest {
  refreshToken: string
}

export interface TokenResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: UserInfo
}

export interface ApiResponse<T> {
  code: number
  message: string
  data: T
  traceId: string
}
