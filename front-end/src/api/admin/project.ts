import http from '../http'
import type {
  Project,
  ProjectDetail,
  ProjectSlot,
  ProjectQueryRequest,
  CreateProjectRequest,
  UpdateProjectRequest,
  AddSlotRequest,
  ProjectStatus,
} from '../../types/project'
import type { ApiResponse, PagedResult } from '../../types/common'

export const projectApi = {
  list(params: ProjectQueryRequest) {
    return http.get<ApiResponse<PagedResult<Project>>>('/v1/admin/projects', { params })
      .then(r => r.data.data)
  },

  getById(id: string) {
    return http.get<ApiResponse<ProjectDetail>>(`/v1/admin/projects/${id}`)
      .then(r => r.data.data)
  },

  create(data: CreateProjectRequest) {
    return http.post<ApiResponse<{ id: string }>>('/v1/admin/projects', data)
      .then(r => r.data.data)
  },

  update(id: string, data: UpdateProjectRequest) {
    return http.put<ApiResponse<object>>(`/v1/admin/projects/${id}`, data)
      .then(r => r.data)
  },

  changeStatus(id: string, newStatus: ProjectStatus) {
    return http.post<ApiResponse<object>>(`/v1/admin/projects/${id}/status`, { newStatus })
      .then(r => r.data)
  },

  getSlots(projectId: string) {
    return http.get<ApiResponse<ProjectSlot[]>>(`/v1/admin/projects/${projectId}/slots`)
      .then(r => r.data.data)
  },

  addSlot(projectId: string, data: AddSlotRequest) {
    return http.post<ApiResponse<ProjectSlot>>(`/v1/admin/projects/${projectId}/slots`, data)
      .then(r => r.data.data)
  },
}
