import http from '../http'
import type { ApiResponse } from '../../types/auth'
import type { ClientDashboardDto, ClientDividendDto, ClientContractDto } from '../../types/client'
import type { PagedResult } from '../admin/wallet'

// ── Dashboard ────────────────────────────────────────────────
export function getClientDashboard() {
  return http.get<ApiResponse<ClientDashboardDto>>('/v1/client/dashboard')
    .then(r => r.data.data)
}

// ── Dividends ────────────────────────────────────────────────
export function getClientDividends(page = 1, pageSize = 20) {
  return http.get<ApiResponse<PagedResult<ClientDividendDto>>>('/v1/client/dividends', { params: { page, pageSize } })
    .then(r => r.data.data)
}

// ── Contracts ────────────────────────────────────────────────
export function getClientContracts() {
  return http.get<ApiResponse<ClientContractDto[]>>('/v1/client/contracts')
    .then(r => r.data.data)
}

export function getContractPdfPath(contractId: string) {
  return http.get<ApiResponse<{ path: string | null }>>(`/v1/client/contracts/${contractId}/pdf-path`)
    .then(r => r.data.data)
}

export function getContractPdfPreviewBlob(contractId: string) {
  return http.get<Blob>(`/v1/client/contracts/${contractId}/pdf-preview`, { responseType: 'blob' })
    .then(r => r.data)
}
