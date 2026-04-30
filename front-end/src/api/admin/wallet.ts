import http from '../http'
import type { ApiResponse } from '../../types/auth'
import type { WalletDto, WalletTransactionDto, WithdrawalRequestDto } from '../../types/wallet'

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

// ── 管理端钱包 ────────────────────────────────────────────────

export function getWalletList(page = 1, pageSize = 20, keyword?: string) {
  return http.get<ApiResponse<PagedResult<WalletDto>>>('/v1/admin/wallets', { params: { page, pageSize, keyword: keyword || undefined } })
    .then(r => r.data.data)
}

export function getWalletByInvestor(investorId: string) {
  return http.get<ApiResponse<WalletDto>>(`/v1/admin/wallets/${investorId}`)
    .then(r => r.data.data)
}

export function getWalletTransactions(investorId: string, page = 1, pageSize = 20) {
  return http.get<ApiResponse<PagedResult<WalletTransactionDto>>>(`/v1/admin/wallets/${investorId}/transactions`, { params: { page, pageSize } })
    .then(r => r.data.data)
}

export function adminCredit(investorId: string, amount: number, remark: string) {
  return http.post<ApiResponse<object>>(`/v1/admin/wallets/${investorId}/credit`, { amount, remark })
    .then(r => r.data)
}

// ── 管理端提现审批 ────────────────────────────────────────────

export function getWithdrawalList(status?: string, page = 1, pageSize = 20, keyword?: string) {
  return http.get<ApiResponse<PagedResult<WithdrawalRequestDto>>>('/v1/admin/withdrawals', { params: { status, page, pageSize, keyword: keyword || undefined } })
    .then(r => r.data.data)
}

export function approveWithdrawal(id: string) {
  return http.post<ApiResponse<object>>(`/v1/admin/withdrawals/${id}/approve`).then(r => r.data)
}

export function completeWithdrawal(id: string) {
  return http.post<ApiResponse<object>>(`/v1/admin/withdrawals/${id}/complete`).then(r => r.data)
}

export function rejectWithdrawal(id: string, reason: string) {
  return http.post<ApiResponse<object>>(`/v1/admin/withdrawals/${id}/reject`, { reason }).then(r => r.data)
}
