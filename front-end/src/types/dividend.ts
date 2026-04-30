export type DividendStatus = 'Calculated' | 'Confirmed' | 'Distributed'

export interface DividendDto {
  id: string
  projectId: string
  projectTitle: string
  slotId: string
  investorUserId: string
  investorName: string
  platformRevenueId: string
  revenueAmount: number
  sharePermille: number
  dividendAmount: number
  currency: string
  status: DividendStatus
  calculatedAt: string
}

export interface DividendQueryRequest {
  /** 多项目筛选，为空时返回全部 */
  projectIds?: string[]
  investorUserId?: string
  /** 多状态筛选，为空时返回全部 */
  statuses?: DividendStatus[]
  page?: number
  pageSize?: number
}

export interface DividendProjectSummaryDto {
  projectId: string
  totalCalculated: number
  totalConfirmed: number
  totalDistributed: number
  recordCount: number
  currency: string
}

export interface BatchDividendRequest {
  dividendIds: string[]
}
