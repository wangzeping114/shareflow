export interface WalletDto {
  id: string
  investorUserId: string
  investorName: string
  investorEmail: string
  balance: number
  frozenAmount: number
  currency: string
  updatedAt: string
}

export interface WalletTransactionDto {
  id: number
  type: string
  typeLabel: string
  direction: string
  directionLabel: string
  amount: number
  currency: string
  balanceBefore: number
  balanceAfter: number
  remark: string
  referenceId?: string
  createdAt: string
}

export interface WithdrawalRequestDto {
  id: string
  investorUserId: string
  investorName: string
  amount: number
  currency: string
  status: WithdrawalStatus
  statusLabel: string
  paymentMethod?: string
  rejectReason?: string
  createdAt: string
  processedAt?: string
}

export type WithdrawalStatus = 'Pending' | 'Approved' | 'Rejected' | 'Completed'
