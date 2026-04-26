import type { Router } from 'vue-router'
import { pinia } from '../stores'
import { useAuthStore } from '../stores/auth'

export function setupRouterGuards(router: Router) {
  router.beforeEach((to) => {
    const authStore = useAuthStore(pinia)
    const requiresAuth = to.meta.requiresAuth === true

    if (requiresAuth && !authStore.isAuthenticated) {
      return '/auth/login'
    }

    if (to.path === '/auth/login' && authStore.isAuthenticated) {
      return authStore.homePath
    }

    if (to.path === '/') {
      return authStore.isAuthenticated ? authStore.homePath : '/auth/login'
    }

    return true
  })
}
