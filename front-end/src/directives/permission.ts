import type { App, DirectiveBinding } from 'vue'
import { pinia } from '../stores'
import { useAuthStore } from '../stores/auth'

function applyPermission(el: HTMLElement, binding: DirectiveBinding<string>) {
  const authStore = useAuthStore(pinia)
  if (!binding.value) {
    return
  }

  if (!authStore.hasPermission(binding.value)) {
    el.style.display = 'none'
  }
}

export function registerPermissionDirective(app: App) {
  app.directive('permission', {
    mounted: applyPermission,
    updated: applyPermission,
  })
}
