export const LeadStatus = {
  New: 'New',
  Contacted: 'Contacted',
  Interested: 'Interested',
  Converted: 'Converted',
  Lost: 'Lost',
} as const

export type LeadStatus = (typeof LeadStatus)[keyof typeof LeadStatus]

export interface LeadDto {
  id: string
  name: string
  contactInfo: string
  email?: string
  status: LeadStatus
  notes?: string
  createdAt: string
  updatedAt: string
}

export interface CreateLeadRequest {
  name: string
  contactInfo: string
  email?: string
  notes?: string
}

export interface UpdateLeadRequest {
  name: string
  contactInfo: string
  email?: string
  notes?: string
  status: LeadStatus
}

export interface SalesProjectSlotDto {
  id: string
  sharePermille: number
}

export interface SalesProjectDto {
  id: string
  title: string
  platformName: string
  availableSlots: number
  slots: SalesProjectSlotDto[]
  createdAt: string
}

export interface SalesClientDto {
  id: string
  name: string
  email: string
  hasAccount: boolean
}

export interface InitiateContractRequest {
  projectId: string
  slotId: string
  investorUserId: string
}

export interface InitiateContractResult {
  contractId: string
  signUrl: string
  expiresAt: string
  /** 仅首次创建账号时存在 */
  clientUsername?: string
  /** 仅首次创建账号时存在，明文临时密码 */
  tempPassword?: string
}

export interface SalesContractDetailDto {
  id: string
  projectTitle: string
  clientName: string
  status: string
  sharePermille: number
  contractSnapshot?: string
  signatureDataUrl?: string
  signedAt?: string
  createdAt: string
  clientUsername?: string
  clientInitialPassword?: string
  signUrl?: string
  signTokenExpiresAt?: string
}

export type SalesContractStatus =
  | 'Draft'
  | 'Sent'
  | 'Signed'
  | 'Executed'
  | 'Expired'
  | 'PendingRenew'
  | 'Renewing'
  | 'Superseded'

export interface SalesContractDto {
  id: string
  projectTitle: string
  slotId: string
  sharePermille: number
  clientName: string
  status: SalesContractStatus
  signedAt?: string
  expiresAt?: string
  createdAt: string
  updatedAt: string
}

export interface SalesPerformanceDto {
  totalLeads: number
  convertedLeads: number
  conversionRate: number
  contractsSent: number
  contractsSigned: number
}

export interface LeadQueryParams {
  status?: LeadStatus
  page?: number
  pageSize?: number
}
