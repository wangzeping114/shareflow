import http from '../http'
import type { ApiResponse } from '../../types/auth'

export interface AdminKpiDto {
  totalRevenue: number
  totalDividends: number
  activeContracts: number
  pendingWithdrawals: number
  totalClients: number
  currency: string
}

export interface MonthlyTrendDto {
  month: string
  revenue: number
  dividend: number
}

export interface NameValueDto {
  name: string
  value: number
}

export interface AdminDashboardDto {
  kpi: AdminKpiDto
  revenueByMonth: MonthlyTrendDto[]
  revenueByPlatform: NameValueDto[]
  dividendByProject: NameValueDto[]
  contractStatusDist: NameValueDto[]
}

export function getAdminDashboard() {
  return http
    .get<ApiResponse<AdminDashboardDto>>('/v1/admin/dashboard')
    .then((r) => r.data.data)
}
