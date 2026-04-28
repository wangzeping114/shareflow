export type RevenueImportSource = 'Manual' | 'Csv' | 'AiScreenshot'
export type RevenueStatus = 'Pending' | 'NeedsVerification' | 'Approved' | 'Rejected'

export interface RevenueDto {
  id: string
  projectId: string
  projectTitle: string
  platformName: string
  amount: number
  currency: string
  revenueDate: string
  importSource: RevenueImportSource
  status: RevenueStatus
  aiConfidence: number
  rejectReason?: string
  createdAt: string
}

export interface RevenueQueryRequest {
  projectId?: string
  status?: RevenueStatus
  platformName?: string
  page?: number
  pageSize?: number
}

export interface AddManualRevenueRequest {
  projectId: string
  platformName: string
  amount: number
  currency: string
  revenueDate: string
}

export interface ConfirmAiImportRequest {
  overrideAmount?: number
}

export interface RejectRevenueRequest {
  reason: string
}

export interface BatchImportResult {
  imported: number
  failed: number
  errors: string[]
}

export interface AiImportPreviewDto {
  revenueId: string
  amount: number
  currency: string
  revenueDate: string
  confidence: number
  needsVerification: boolean
  aiRawResult?: string
}
