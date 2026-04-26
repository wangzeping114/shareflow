import http from './http'
import type { ApiResponse, LoginRequest, RefreshTokenRequest, TokenResponse } from '../types/auth'

export function login(payload: LoginRequest) {
  return http.post<ApiResponse<TokenResponse>>('/v1/auth/login', payload).then((response) => response.data.data)
}

export function refreshToken(payload: RefreshTokenRequest) {
  return http.post<ApiResponse<TokenResponse>>('/v1/auth/refresh', payload).then((response) => response.data.data)
}

export function logout(payload: RefreshTokenRequest) {
  return http.post('/v1/auth/logout', payload).then((response) => response.data)
}
