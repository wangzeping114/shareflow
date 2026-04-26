import type { Router } from 'vue-router'
import { pinia } from '../stores'
import { useAuthStore } from '../stores/auth'

export function setupRouterGuards(router: Router) {
  router.beforeEach((to) => {
    const authStore = useAuthStore(pinia)
    const requiresAuth = to.meta.requiresAuth === true

    // 未认证访问需要权限的页面 → 去登录
    if (requiresAuth && !authStore.isAuthenticated) {
      return '/auth/login'
    }

    // 已认证访问登录页 → 去首页（防止 homePath 本身是 /auth/login 时死循环）
    if (to.path === '/auth/login' && authStore.isAuthenticated) {
      const home = authStore.homePath
      if (home) {
        return home
      }
      // role 异常，清除 session 重新登录
      authStore.clearSession()
      return true
    }

    if (to.path === '/') {
      if (authStore.isAuthenticated) {
        const home = authStore.homePath
        if (home) return home
        authStore.clearSession()
      }
      return '/auth/login'
    }

    return true
  })
}
