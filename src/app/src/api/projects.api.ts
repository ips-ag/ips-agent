import apiClient from './client';
import type { PagedResult } from '../types/common.types';
import type { ProjectDto, CreateProjectRequest, UpdateProjectRequest, ProjectTreeDto } from '../types/project.types';
import type { UserDto } from '../types/user.types';

export const projectsApi = {
  getAll: (page = 1, pageSize = 20, search?: string, customerId?: string) =>
    apiClient.get<PagedResult<ProjectDto>>('/projects', { params: { page, pageSize, search, customerId } }).then(r => r.data),
  getById: (id: string) =>
    apiClient.get<ProjectDto>(`/projects/${id}`).then(r => r.data),
  getHierarchy: (id: string) =>
    apiClient.get<ProjectTreeDto>(`/projects/${id}/hierarchy`).then(r => r.data),
  getProjectUsers: (id: string) =>
    apiClient.get<UserDto[]>(`/projects/${id}/users`).then(r => r.data),
  create: (data: CreateProjectRequest) =>
    apiClient.post<string>('/projects', data).then(r => r.data),
  update: (id: string, data: UpdateProjectRequest) =>
    apiClient.put(`/projects/${id}`, { id, ...data }),
  archive: (id: string) =>
    apiClient.delete(`/projects/${id}`),
  assignUser: (projectId: string, userId: string) =>
    apiClient.post(`/projects/${projectId}/users`, { userId }),
  removeUser: (projectId: string, userId: string) =>
    apiClient.delete(`/projects/${projectId}/users/${userId}`),
};
