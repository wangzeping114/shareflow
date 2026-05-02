<script setup lang="ts">
import { h, computed } from 'vue'
import { RouterView, useRoute, useRouter } from 'vue-router'
import {
  NLayout, NLayoutSider, NLayoutHeader, NLayoutContent,
  NMenu, NFlex, NText, NButton, NAvatar,
  NMessageProvider, NDialogProvider, NNotificationProvider, NConfigProvider,
  type MenuOption,
} from 'naive-ui'
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const activeKey = computed(() => route.name as string | null)

const menuOptions: MenuOption[] = [
  {
    label: () => h('span', '工作台'),
    key: 'sales-dashboard',
    icon: () => h('span', { style: 'font-size:16px' }, '📊'),
  },
  {
    label: () => h('span', '客户管理'),
    key: 'sales-leads',
    icon: () => h('span', { style: 'font-size:16px' }, '🎯'),
  },
  {
    label: () => h('span', '项目中心'),
    key: 'sales-projects',
    icon: () => h('span', { style: 'font-size:16px' }, '📁'),
  },
  {
    label: () => h('span', '合同管理'),
    key: 'sales-contracts',
    icon: () => h('span', { style: 'font-size:16px' }, '📝'),
  },
]

const routeMap: Record<string, string> = {
  'sales-dashboard': '/sales/dashboard',
  'sales-leads':     '/sales/leads',
  'sales-projects':  '/sales/projects',
  'sales-contracts': '/sales/contracts',
}

function handleMenuSelect(key: string) {
  const path = routeMap[key]
  if (path) router.push(path)
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
          <n-layout has-sider style="min-height: 100vh; background: #ffffff">
            <!-- 侧边栏 -->
            <n-layout-sider
              bordered
              collapse-mode="width"
              :collapsed-width="64"
              :width="200"
              show-trigger
              style="background: #fff; min-height: 100vh"
            >
              <div class="brand">
                <n-text strong style="color: #3b82f6; font-size: 16px">ShareFlow</n-text>
                <n-text depth="3" style="font-size: 11px; display: block">销售工作台</n-text>
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
                    {{ authStore.user?.displayName?.charAt(0) ?? 'S' }}
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
