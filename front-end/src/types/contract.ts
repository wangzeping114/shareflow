export type ContractStatus =
  | 'Draft'
  | 'Sent'
  | 'Signed'
  | 'Executed'
  | 'Expired'
  | 'PendingRenew'
  | 'Renewing'
  | 'Superseded'

export interface ContractDto {
  id: string
  contractNo: number
  projectId: string
  projectTitle: string
  slotId: string
  investorUserId: string
  investorName: string
  status: ContractStatus
  templateType: string
  signTokenExpiresAt?: string
  signedAt?: string
  hasPdf: boolean
  createdAt: string
  updatedAt: string
}

export interface ContractDetailDto extends ContractDto {
  contractSnapshot?: string
  signUrl?: string
}

export interface ContractPreviewDto {
  contractId: string
  projectTitle: string
  investorName: string
  sharePct: number
  templateType: string
  contractSnapshot?: string
  expiresAt: string
}

export interface GenerateSignLinkResult {
  contractNo: number
  signUrl: string
  expiresAt: string
}

export interface ContractQueryRequest {
  projectId?: string
  status?: ContractStatus
  projectTitle?: string
  page?: number
  pageSize?: number
}

export interface CreateContractRequest {
  projectId: string
  slotId: string
  investorUserId: string
}

export interface SubmitSignatureRequest {
  signatureDataUrl: string
}

export interface SignContractResult {
  clientUsername?: string
  initialPassword?: string
}
