<script setup lang="ts">
import { computed, h } from 'vue'
import { RouterView, useRoute, useRouter } from 'vue-router'
import {
  NLayout,
  NLayoutSider,
  NLayoutHeader,
  NLayoutContent,
  NMenu,
  NFlex,
  NText,
  NButton,
  NAvatar,
  NMessageProvider,
  NDialogProvider,
  NNotificationProvider,
  NConfigProvider,
  type MenuOption,
} from 'naive-ui'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const activeKey = computed(() => route.name as string | null)

const menuOptions: MenuOption[] = [
  {
    label: () => h('span', '仪表盘'),
    key: 'admin-home',
    icon: () => h('span', { style: 'font-size:16px' }, '📊'),
  },
  {
    label: () => h('span', '项目管理'),
    key: 'admin-projects',
    icon: () => h('span', { style: 'font-size:16px' }, '🎬'),
  },
  {
    label: () => h('span', '合同管理'),
    key: 'admin-contracts',
    icon: () => h('span', { style: 'font-size:16px' }, '📄'),
  },
  {
    label: () => h('span', '收益管理'),
    key: 'admin-revenues',
    icon: () => h('span', { style: 'font-size:16px' }, '💰'),
  },
  {
    label: () => h('span', '分红管理'),
    key: 'admin-dividends',
    icon: () => h('span', { style: 'font-size:16px' }, '🎯'),
  },
  {
    label: () => h('span', '角色权限'),
    key: 'admin-roles',
    icon: () => h('span', { style: 'font-size:16px' }, '🔐'),
  },
]

function handleMenuSelect(key: string) {
  const routeMap: Record<string, string> = {
    'admin-home': '/admin',
    'admin-projects': '/admin/projects',
    'admin-contracts': '/admin/contracts',
    'admin-revenues': '/admin/revenues',
    'admin-dividends': '/admin/dividends',
    'admin-roles': '/admin/roles',
  }
  const target = routeMap[key]
  if (target) router.push(target)
}

async function handleLogout() {
  await authStore.logout()
  router.replace('/auth/login')
}
</script>

<template>
  <n-config-provider>
  <n-message-provider>
    <n-notification-provider>
      <n-dialog-provider>
  <n-layout has-sider style="min-height: 100vh; background: #f5f7fb">
    <!-- 侧边栏 -->
    <n-layout-sider
      bordered
      collapse-mode="width"
      :collapsed-width="64"
      :width="220"
      show-trigger
      style="background: #fff"
    >
      <div class="brand">
        <n-text strong style="color: #3b82f6; font-size: 18px">ShareFlow</n-text>
        <n-text depth="3" style="font-size: 11px; display: block">管理后台</n-text>
      </div>
      <n-menu
        :value="activeKey"
        :options="menuOptions"
        :indent="18"
        @update:value="handleMenuSelect"
      />
    </n-layout-sider>

    <!-- 主区域 -->
    <n-layout>
      <n-layout-header bordered style="padding: 0 24px; height: 56px; background: #fff">
        <n-flex align="center" justify="end" style="height: 100%">
          <n-text depth="2">{{ authStore.user?.displayName }}</n-text>
          <n-avatar round size="small" style="background: #3b82f6">
            {{ authStore.user?.displayName?.charAt(0) ?? 'A' }}
          </n-avatar>
          <n-button text @click="handleLogout" style="color: #999">退出登录</n-button>
        </n-flex>
      </n-layout-header>

      <n-layout-content style="padding: 24px">
        <RouterView />
      </n-layout-content>
    </n-layout>
  </n-layout>
      </n-dialog-provider>
    </n-notification-provider>
  </n-message-provider>
  </n-config-provider>
</template>

<style scoped>
.brand {
  padding: 20px 20px 12px;
  border-bottom: 1px solid #f0f0f0;
  margin-bottom: 8px;
}
</style>
