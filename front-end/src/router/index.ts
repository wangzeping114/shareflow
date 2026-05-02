import { createRouter, createWebHistory } from 'vue-router'
import { setupRouterGuards } from './guards'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      component: () => import('../layouts/PublicLayout.vue'),
      children: [],
    },
    {
      path: '/auth/login',
      component: () => import('../layouts/PublicLayout.vue'),
      children: [
        {
          path: '',
          name: 'login',
          component: () => import('../views/auth/LoginView.vue'),
        },
      ],
    },
    {
      path: '/admin',
      component: () => import('../layouts/AdminLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          name: 'admin-home',
          component: () => import('../views/admin/AdminHomeView.vue'),
        },
        {
          path: 'projects',
          name: 'admin-projects',
          component: () => import('../views/admin/projects/ProjectListView.vue'),
        },
        {
          path: 'projects/:id',
          name: 'admin-project-detail',
          component: () => import('../views/admin/projects/ProjectDetailView.vue'),
        },
        {
          path: 'contracts',
          name: 'admin-contracts',
          component: () => import('../views/admin/contracts/ContractListView.vue'),
        },
        {
          path: 'contracts/:id',
          name: 'admin-contract-detail',
          component: () => import('../views/admin/contracts/ContractDetailView.vue'),
        },
        {
          path: 'revenues',
          name: 'admin-revenues',
          component: () => import('../views/admin/revenues/RevenueListView.vue'),
        },
        {
          path: 'dividends',
          name: 'admin-dividends',
          component: () => import('../views/admin/dividends/DividendListView.vue'),
        },
        {
          path: 'wallets',
          name: 'admin-wallets',
          component: () => import('../views/admin/wallet/WalletListView.vue'),
        },
        {
          path: 'withdrawals',
          name: 'admin-withdrawals',
          component: () => import('../views/admin/wallet/WithdrawalRequestListView.vue'),
        },
      ],
    },
    {
      path: '/esign/:token',
      component: () => import('../layouts/PublicLayout.vue'),
      meta: { requiresAuth: false },
      children: [
        {
          path: '',
          name: 'esign',
          component: () => import('../views/esign/ESignView.vue'),
        },
      ],
    },
    {
      path: '/sales',
      component: () => import('../layouts/SalesLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          redirect: '/sales/dashboard',
        },
        {
          path: 'dashboard',
          name: 'sales-dashboard',
          component: () => import('../views/sales/dashboard/SalesDashboardView.vue'),
        },
        {
          path: 'leads',
          name: 'sales-leads',
          component: () => import('../views/sales/leads/LeadsView.vue'),
        },
        {
          path: 'projects',
          name: 'sales-projects',
          component: () => import('../views/sales/projects/SalesProjectsView.vue'),
        },
        {
          path: 'contracts',
          name: 'sales-contracts',
          component: () => import('../views/sales/contracts/SalesContractsView.vue'),
        },
      ],
    },
    {
      path: '/client',
      component: () => import('../layouts/ClientLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          redirect: '/client/dashboard',
        },
        {
          path: 'dashboard',
          name: 'client-dashboard',
          component: () => import('../views/client/dashboard/ClientDashboardView.vue'),
        },
        {
          path: 'dividends',
          name: 'client-dividends',
          component: () => import('../views/client/dividends/ClientDividendsView.vue'),
        },
        {
          path: 'contracts',
          name: 'client-contracts',
          component: () => import('../views/client/contracts/ClientContractsView.vue'),
        },
        {
          path: 'wallet',
          name: 'client-wallet',
          component: () => import('../views/client/wallet/WalletView.vue'),
        },
      ],
    },
  ],
})

setupRouterGuards(router)

export default router
