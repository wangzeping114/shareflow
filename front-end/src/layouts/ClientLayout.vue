<script setup lang="ts">
import { h, computed, onMounted } from 'vue'
import { RouterView, useRoute, useRouter } from 'vue-router'
import {
  NLayout, NLayoutSider, NLayoutHeader, NLayoutContent,
  NMenu, NFlex, NText, NButton, NAvatar,
  NMessageProvider, NDialogProvider, NNotificationProvider, NConfigProvider,
  enUS, dateEnUS, zhCN, dateZhCN,
  type MenuOption,
} from 'naive-ui'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '../stores/auth'
import { useRegion } from '../composables/use-region'

const { locale } = useI18n()
const { clientLocale } = useRegion()
onMounted(() => { locale.value = clientLocale })

const authStore = useAuthStore()
const router = useRouter()
const route = useRoute()

const activeKey = computed(() => route.name as string | null)
const isCN = computed(() => locale.value === 'zh-CN')
const naiveLocale = computed(() => isCN.value ? zhCN : enUS)
const naiveDateLocale = computed(() => isCN.value ? dateZhCN : dateEnUS)

const menuOptions: MenuOption[] = [
  {
    label: () => h('span', 'Dashboard'),
    key: 'client-dashboard',
    icon: () => h('span', { style: 'font-size:16px' }, '📊'),
  },
  {
    label: () => h('span', isCN.value ? '分红记录' : 'Dividends'),
    key: 'client-dividends',
    icon: () => h('span', { style: 'font-size:16px' }, '💰'),
  },
  {
    label: () => h('span', isCN.value ? '我的合同' : 'Contracts'),
    key: 'client-contracts',
    icon: () => h('span', { style: 'font-size:16px' }, '📄'),
  },
  {
    label: () => h('span', isCN.value ? '钱包' : 'Wallet'),
    key: 'client-wallet',
    icon: () => h('span', { style: 'font-size:16px' }, '💳'),
  },
]

const routeMap: Record<string, string> = {
  'client-dashboard':  '/client/dashboard',
  'client-dividends':  '/client/dividends',
  'client-contracts':  '/client/contracts',
  'client-wallet':     '/client/wallet',
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
  <n-config-provider :locale="naiveLocale" :date-locale="naiveDateLocale">
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
                <n-text depth="3" style="font-size: 11px; display: block">
                  {{ isCN ? '持股门户' : 'Investor Portal' }}
                </n-text>
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
                    {{ authStore.user?.displayName?.charAt(0) ?? 'C' }}
                  </n-avatar>
                  <n-button text @click="handleLogout" style="color: #999">
                    {{ isCN ? '退出登录' : 'Logout' }}
                  </n-button>
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

