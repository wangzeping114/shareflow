import http from '../http'
import type { ApiResponse } from '../../types/auth'
import type { WalletDto, WalletTransactionDto, WithdrawalRequestDto } from '../../types/wallet'

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export function getMyWallet() {
  return http.get<ApiResponse<WalletDto>>('/v1/client/wallet').then(r => r.data.data)
}

export function getMyTransactions(page = 1, pageSize = 20) {
  return http.get<ApiResponse<PagedResult<WalletTransactionDto>>>('/v1/client/wallet/transactions', { params: { page, pageSize } })
    .then(r => r.data.data)
}

export function requestWithdrawal(amount: number, paymentMethod?: string) {
  return http.post<ApiResponse<{ id: string }>>('/v1/client/wallet/withdrawals', { amount, paymentMethod })
    .then(r => r.data.data)
}

export function getMyWithdrawals(page = 1, pageSize = 20) {
  return http.get<ApiResponse<PagedResult<WithdrawalRequestDto>>>('/v1/client/wallet/withdrawals', { params: { page, pageSize } })
    .then(r => r.data.data)
}
