<script setup lang="ts">
import { computed } from 'vue'
import { NButton } from 'naive-ui'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '../stores/auth'

interface Props {
  titleKey: string
}

const props = defineProps<Props>()
const { t } = useI18n()
const router = useRouter()
const authStore = useAuthStore()

const title = computed(() => t(props.titleKey))

async function handleLogout() {
  await authStore.logout()
  await router.replace('/auth/login')
}
</script>

<template>
  <div class="app-shell">
    <header class="app-header">
      <div>
        <div class="brand">{{ t('app.brand') }}</div>
        <h1>{{ title }}</h1>
      </div>
      <div class="user-panel">
        <span>{{ authStore.user?.displayName }}</span>
        <n-button tertiary type="primary" @click="handleLogout">{{ t('app.logout') }}</n-button>
      </div>
    </header>

    <main class="app-main">
      <router-view />
    </main>
  </div>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
  padding: 24px;
  background: linear-gradient(180deg, #f6fbff 0%, #edf4ff 100%);
}

.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 24px;
  border-radius: 20px;
  background: #ffffff;
  box-shadow: 0 16px 40px rgba(15, 23, 42, 0.08);
}

.brand {
  color: #3b82f6;
  font-weight: 700;
  margin-bottom: 8px;
}

.user-panel {
  display: flex;
  align-items: center;
  gap: 12px;
}

.app-main {
  margin-top: 24px;
}
</style>
