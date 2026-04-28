import http from '../http'
import type { ApiResponse } from '../../types/auth'
import type {
  ContractDto,
  ContractDetailDto,
  ContractQueryRequest,
  CreateContractRequest,
  GenerateSignLinkResult,
} from '../../types/contract'

export interface PagedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export function getContractList(query: ContractQueryRequest = {}) {
  return http
    .get<ApiResponse<PagedResult<ContractDto>>>('/v1/admin/contracts', { params: query })
    .then((r) => r.data.data)
}

export function getContractById(id: string) {
  return http
    .get<ApiResponse<ContractDetailDto>>(`/v1/admin/contracts/${id}`)
    .then((r) => r.data.data)
}

export function createContract(payload: CreateContractRequest) {
  return http
    .post<ApiResponse<{ id: string }>>('/v1/admin/contracts', payload)
    .then((r) => r.data.data)
}

export function generateSignLink(id: string) {
  return http
    .post<ApiResponse<GenerateSignLinkResult>>(`/v1/admin/contracts/${id}/send`)
    .then((r) => r.data.data)
}

export interface UserSummaryDto {
  id: string
  displayName: string
  email: string
}

export function getUserClients() {
  return http
    .get<ApiResponse<UserSummaryDto[]>>('/v1/admin/users/clients')
    .then((r) => r.data.data)
}

export function getTemplatePreview(type: 'OverseasEnglish' | 'DomesticChinese') {
  return http
    .get<ApiResponse<{ type: string; content: string }>>('/v1/admin/contracts/template/preview', {
      params: { type },
    })
    .then((r) => r.data.data)
}
