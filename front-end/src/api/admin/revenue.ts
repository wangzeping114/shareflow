import http from '../http'
import type { ApiResponse } from '../../types/auth'
import type {
  RevenueDto,
  RevenueQueryRequest,
  AddManualRevenueRequest,
  ConfirmAiImportRequest,
  RejectRevenueRequest,
  BatchImportResult,
  AiImportPreviewDto,
} from '../../types/revenue'

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export function getRevenueList(query: RevenueQueryRequest = {}) {
  return http
    .get<ApiResponse<PagedResult<RevenueDto>>>('/v1/admin/revenues', { params: query })
    .then((r) => r.data.data)
}

export function addManualRevenue(data: AddManualRevenueRequest) {
  return http
    .post<ApiResponse<{ id: string }>>('/v1/admin/revenues/manual', data)
    .then((r) => r.data.data)
}

export function importRevenueCsv(projectId: string, platform: string, file: File) {
  const form = new FormData()
  form.append('file', file)
  return http
    .post<ApiResponse<BatchImportResult>>(
      `/v1/admin/revenues/csv?projectId=${projectId}&platform=${encodeURIComponent(platform)}`,
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } },
    )
    .then((r) => r.data.data)
}

export function previewRevenueScreenshot(projectId: string, platform: string, file: File) {
  const form = new FormData()
  form.append('file', file)
  return http
    .post<ApiResponse<AiImportPreviewDto>>(
      `/v1/admin/revenues/screenshot-preview?projectId=${projectId}&platform=${encodeURIComponent(platform)}`,
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } },
    )
    .then((r) => r.data.data)
}

export function confirmAiImport(id: string, data: ConfirmAiImportRequest) {
  return http
    .post<ApiResponse<object>>(`/v1/admin/revenues/${id}/confirm-ai`, data)
    .then((r) => r.data.data)
}

export function approveRevenue(id: string) {
  return http
    .post<ApiResponse<object>>(`/v1/admin/revenues/${id}/approve`)
    .then((r) => r.data.data)
}

export function rejectRevenue(id: string, data: RejectRevenueRequest) {
  return http
    .post<ApiResponse<object>>(`/v1/admin/revenues/${id}/reject`, data)
    .then((r) => r.data.data)
}
