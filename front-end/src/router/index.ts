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
          name: 'sales-home',
          component: () => import('../views/sales/SalesHomeView.vue'),
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
          name: 'client-home',
          component: () => import('../views/client/ClientHomeView.vue'),
        },
      ],
    },
  ],
})

setupRouterGuards(router)

export default router
