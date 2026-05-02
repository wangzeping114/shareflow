import http from '../http'
import type { ApiResponse, PagedResult } from '../../types/common'
import type {
  LeadDto,
  CreateLeadRequest,
  UpdateLeadRequest,
  SalesProjectDto,
  SalesClientDto,
  SalesContractDto,
  SalesContractDetailDto,
  InitiateContractRequest,
  InitiateContractResult,
  SalesPerformanceDto,
  LeadQueryParams,
} from '../../types/sales'

export function getLeads(params: LeadQueryParams) {
  return http.get<ApiResponse<PagedResult<LeadDto>>>('/v1/sales/leads', { params })
    .then(r => r.data.data)
}

export function createLead(data: CreateLeadRequest) {
  return http.post<ApiResponse<LeadDto>>('/v1/sales/leads', data)
    .then(r => r.data.data)
}

export function updateLead(id: string, data: UpdateLeadRequest) {
  return http.put<ApiResponse<LeadDto>>(`/v1/sales/leads/${id}`, data)
    .then(r => r.data.data)
}

export function getSalesProjects() {
  return http.get<ApiResponse<SalesProjectDto[]>>('/v1/sales/projects')
    .then(r => r.data.data)
}

export function getSalesClients() {
  return http.get<ApiResponse<SalesClientDto[]>>('/v1/sales/clients')
    .then(r => r.data.data)
}

export function getSalesContracts() {
  return http.get<ApiResponse<SalesContractDto[]>>('/v1/sales/contracts')
    .then(r => r.data.data)
}

export function getSalesContractDetail(id: string) {
  return http.get<ApiResponse<SalesContractDetailDto>>(`/v1/sales/contracts/${id}`)
    .then(r => r.data.data)
}

export function initiateContract(data: InitiateContractRequest) {
  return http.post<ApiResponse<InitiateContractResult>>('/v1/sales/contracts', data)
    .then(r => r.data.data)
}

export function getSalesPerformance() {
  return http.get<ApiResponse<SalesPerformanceDto>>('/v1/sales/performance')
    .then(r => r.data.data)
}
