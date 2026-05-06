import http from '../http'
import type { ApiResponse } from '../../types/auth'

export interface BackendRoleDto {
  id: string
  name: string
  permissions: string[]
}

export interface CreateBackendRoleRequest {
  name: string
  permissions: string[]
}

export interface UpdateBackendRolePermissionsRequest {
  permissions: string[]
}

export interface InternalUserDto {
  id: string
  username: string
  email: string
  displayName: string
  role: string
  backendRoleId: string | null
  backendRoleName: string | null
  status: string
  createdAt: string
}

export interface CreateInternalUserRequest {
  username: string
  email: string
  displayName: string
  password: string
  role: string
  backendRoleId?: string
}

export interface AssignBackendRoleRequest {
  backendRoleId: string | null
}

export const ALL_PERMISSIONS: { value: string; label: string; group: string }[] = [
  { value: 'dashboard.read',     label: '仪表盘查看', group: '仪表盘' },
  { value: 'project.read',       label: '项目查看',   group: '项目管理' },
  { value: 'project.write',      label: '项目编辑',   group: '项目管理' },
  { value: 'contract.read',      label: '合同查看',   group: '合同管理' },
  { value: 'contract.renew',     label: '合同续签',   group: '合同管理' },
  { value: 'revenue.read',       label: '收益查看',   group: '收益管理' },
  { value: 'revenue.write',      label: '收益录入',   group: '收益管理' },
  { value: 'revenue.verify',     label: '收益审核',   group: '收益管理' },
  { value: 'dividend.read',      label: '分红查看',   group: '分红管理' },
  { value: 'dividend.write',     label: '分红发放',   group: '分红管理' },
  { value: 'withdrawal.approve', label: '提现审批',   group: '钱包管理' },
  { value: 'user.manage',        label: '用户管理',   group: '权限管理' },
  { value: 'role.manage',        label: '角色管理',   group: '权限管理' },
  { value: 'report.read',        label: '报表查看',   group: '报表' },
]

export const roleApi = {
  getAll: () => http.get<ApiResponse<BackendRoleDto[]>>('/v1/admin/roles'),
  create: (data: CreateBackendRoleRequest) => http.post<ApiResponse<BackendRoleDto>>('/v1/admin/roles', data),
  updatePermissions: (id: string, data: UpdateBackendRolePermissionsRequest) =>
    http.put<ApiResponse<BackendRoleDto>>(`/v1/admin/roles/${id}/permissions`, data),
  delete: (id: string) => http.delete<ApiResponse<object>>(`/v1/admin/roles/${id}`),
  getInternalUsers: () => http.get<ApiResponse<InternalUserDto[]>>('/v1/admin/users/internal'),
  createInternalUser: (data: CreateInternalUserRequest) =>
    http.post<ApiResponse<InternalUserDto>>('/v1/admin/users/internal', data),
  assignRole: (userId: string, data: AssignBackendRoleRequest) =>
    http.post<ApiResponse<object>>(`/v1/admin/users/${userId}/role`, data),
}