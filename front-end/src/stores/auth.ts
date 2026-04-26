import { computed, ref } from 'vue'
import { defineStore } from 'pinia'
import { login as loginRequest, logout as logoutRequest, refreshToken as refreshTokenRequest } from '../api/auth'
import type { TokenResponse, UserInfo, UserRole } from '../types/auth'

function resolveHomePath(role?: UserRole | null | number) {
  // 兼容后端返回数字枚举的情况（0=SuperAdmin,1=BackendCustom,2=Sales,3=Client）
  const roleStr = typeof role === 'number'
    ? (['SuperAdmin', 'BackendCustom', 'Sales', 'Client'][role] ?? null)
    : role
  switch (roleStr) {
    case 'Sales':
      return '/sales'
    case 'Client':
      return '/client'
    case 'SuperAdmin':
    case 'BackendCustom':
      return '/admin'
    default:
      return null
  }
}

export const useAuthStore = defineStore(
  'auth',
  () => {
    const accessToken = ref<string | null>(null)
    const refreshTokenValue = ref<string | null>(null)
    const expiresAt = ref<string | null>(null)
    const user = ref<UserInfo | null>(null)
    const permissions = ref<string[]>([])

    const isAuthenticated = computed(() => Boolean(accessToken.value && user.value))
    const homePath = computed(() => resolveHomePath(user.value?.role ?? null))

    function applyTokenPayload(payload: TokenResponse) {
      accessToken.value = payload.accessToken
      refreshTokenValue.value = payload.refreshToken
      expiresAt.value = payload.expiresAt
      user.value = payload.user
      permissions.value = payload.user.permissions ?? []
    }

    function hasPermission(key: string): boolean {
      if (user.value?.role === 'SuperAdmin') {
        return true
      }

      return permissions.value.includes(key)
    }

    async function login(username: string, password: string) {
      const payload = await loginRequest({ username, password })
      applyTokenPayload(payload)
      return payload
    }

    async function refreshSession() {
      if (!refreshTokenValue.value) {
        throw new Error('No refresh token')
      }

      const payload = await refreshTokenRequest({ refreshToken: refreshTokenValue.value })
      applyTokenPayload(payload)
      return payload
    }

    async function logout() {
      const currentRefreshToken = refreshTokenValue.value

      clearSession()

      if (currentRefreshToken) {
        await logoutRequest({ refreshToken: currentRefreshToken }).catch(() => undefined)
      }
    }

    function clearSession() {
      accessToken.value = null
      refreshTokenValue.value = null
      expiresAt.value = null
      user.value = null
      permissions.value = []
    }

    return {
      accessToken,
      refreshTokenValue,
      expiresAt,
      user,
      permissions,
      isAuthenticated,
      homePath,
      hasPermission,
      login,
      refreshSession,
      logout,
      clearSession,
    }
  },
  {
    persist: true,
  },
)
