// ── Client Dashboard ─────────────────────────────────────────

export interface ClientProjectSummaryDto {
  projectId: string
  projectTitle: string
  platformName: string
  sharePermille: number   // 千分比，如 35 = 3.5%
  totalDividendReceived: number
  currency: string
  contractStatus: string
}

export interface ClientRecentDividendDto {
  id: string
  projectTitle: string
  dividendAmount: number
  currency: string
  statusLabel: string
  calculatedAt: string
}

export interface ClientDashboardDto {
  walletBalance: number
  frozenAmount: number
  currency: string
  totalDividendReceived: number
  projects: ClientProjectSummaryDto[]
  recentDividends: ClientRecentDividendDto[]
}

// ── Dividends ────────────────────────────────────────────────

export interface ClientDividendDto {
  id: string
  projectId: string
  projectTitle: string
  platformName: string
  revenueAmount: number
  sharePermille: number
  dividendAmount: number
  currency: string
  status: string
  statusLabel: string
  calculatedAt: string
}

// ── Contracts ────────────────────────────────────────────────

export interface ClientContractDto {
  id: string
  projectId: string
  projectTitle: string
  platformName: string
  sharePermille: number
  status: string
  statusLabel: string
  signedAt: string | null
  hasPdf: boolean
}
