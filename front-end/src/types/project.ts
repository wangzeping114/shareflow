export type ProjectSlotMode = 'Fixed' | 'Flexible'
export type ProjectStatus = 'Draft' | 'Active' | 'Paused' | 'Closed'
export type SlotStatus = 'Available' | 'Reserved' | 'Occupied' | 'Released'

export interface Project {
  id: string
  title: string
  description: string
  platformName: string
  slotMode: ProjectSlotMode
  status: ProjectStatus
  totalSlots: number
  filledSlots: number
  reservedSlots: number
  availableSlots: number
  fundingProgressPct: number
  totalInvestment?: number
  createdAt: string
}

export interface ProjectSlot {
  id: string
  slotNumber: number
  alias?: string
  sharePct: number
  status: SlotStatus
  clientUserId?: string
  contractMonths: number
  templateType: string
}

export interface ProjectDetail extends Project {
  slots: ProjectSlot[]
}

export interface CreateProjectRequest {
  title: string
  description: string
  platformName: string
  slotMode: ProjectSlotMode
  totalSlots: number
  totalInvestment?: number
}

export interface UpdateProjectRequest {
  title: string
  description: string
  platformName: string
  totalSlots: number
  totalInvestment?: number
}

export interface ProjectQueryRequest {
  title?: string
  status?: ProjectStatus
  page?: number
  pageSize?: number
}

export interface AddSlotRequest {
  sharePct: number
}
