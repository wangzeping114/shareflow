import { createI18n } from 'vue-i18n'
import { useRegion } from '../composables/use-region'

const messages = {
  'zh-CN': {
    app: {
      brand: 'ShareFlow',
      welcome: '欢迎回来',
      logout: '退出登录',
      login: '登录',
    },
    auth: {
      title: '账号登录',
      subtitle: '使用 ShareFlow 账号访问管理后台或客户端门户。',
      username: '用户名',
      password: '密码',
      usernamePlaceholder: '请输入用户名',
      passwordPlaceholder: '请输入密码',
      loginFailed: '登录失败，请检查账号或密码。',
    },
    dashboard: {
      admin: '管理员工作台',
      sales: '销售工作台',
      client: '投资人门户',
      adminDesc: '你已登录后台管理区域。',
      salesDesc: '你已登录销售工作区域。',
      clientDesc: '你已登录投资人客户端。',
    },
  },
  'en-US': {
    app: {
      brand: 'ShareFlow',
      welcome: 'Welcome back',
      logout: 'Logout',
      login: 'Login',
    },
    auth: {
      title: 'Account Login',
      subtitle: 'Use your ShareFlow account to access the admin or client portal.',
      username: 'Username',
      password: 'Password',
      usernamePlaceholder: 'Enter your username',
      passwordPlaceholder: 'Enter your password',
      loginFailed: 'Login failed. Please verify your credentials.',
    },
    dashboard: {
      admin: 'Admin Workspace',
      sales: 'Sales Workspace',
      client: 'Client Portal',
      adminDesc: 'You are signed in to the administration area.',
      salesDesc: 'You are signed in to the sales workspace.',
      clientDesc: 'You are signed in to the client portal.',
    },
  },
}

const { locale } = useRegion()

export const i18n = createI18n({
  legacy: false,
  locale,
  fallbackLocale: 'en-US',
  messages,
})
