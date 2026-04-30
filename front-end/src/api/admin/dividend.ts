import http from '../http'
import type { ApiResponse } from '../../types/auth'
import type {
  DividendDto,
  DividendQueryRequest,
  DividendProjectSummaryDto,
  BatchDividendRequest,
} from '../../types/dividend'

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export function getDividendList(query: DividendQueryRequest = {}) {
  // ASP.NET Core 接受重复键的数组：projectIds=a&projectIds=b
  const params = new URLSearchParams()
  if (query.page) params.append('page', String(query.page))
  if (query.pageSize) params.append('pageSize', String(query.pageSize))
  if (query.investorUserId) params.append('investorUserId', query.investorUserId)
  query.projectIds?.forEach((id) => params.append('projectIds', id))
  query.statuses?.forEach((s) => params.append('statuses', s))

  return http
    .get<ApiResponse<PagedResult<DividendDto>>>('/v1/admin/dividends', { params })
    .then((r) => r.data.data)
}

export function getDividendProjectSummary(projectId: string) {
  return http
    .get<ApiResponse<DividendProjectSummaryDto>>('/v1/admin/dividends/summary', {
      params: { projectId },
    })
    .then((r) => r.data.data)
}

export function confirmDividends(dividendIds: string[]) {
  const data: BatchDividendRequest = { dividendIds }
  return http
    .post<ApiResponse<object>>('/v1/admin/dividends/confirm', data)
    .then((r) => r.data)
}

export function distributeDividends(dividendIds: string[]) {
  const data: BatchDividendRequest = { dividendIds }
  return http
    .post<ApiResponse<object>>('/v1/admin/dividends/distribute', data)
    .then((r) => r.data)
}

/**
 * 修复历史数据：将所有已签署合同的槽位强制设置为 Occupied
 * 用于处理存量数据 ContractSignedEventHandler 未正确更新槽位的情况
 */
export function repairSlots() {
  return http
    .post<ApiResponse<{ repairedCount: number; message: string }>>('/v1/admin/dividends/repair-slots')
    .then((r) => r.data.data)
}

/**
 * 手动补算某条已审核收益的分红记录
 * 适用于：事件处理失败、存量数据首次补算等场景
 */
export function recalculateDividends(revenueId: string) {
  return http
    .post<ApiResponse<object>>(`/v1/admin/dividends/${revenueId}/recalculate`)
    .then((r) => r.data)
}
