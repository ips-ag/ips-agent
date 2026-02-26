import apiClient from './client';
import type { PagedResult } from '../types/common.types';
import type { UserDto, UpdateUserRequest } from '../types/user.types';
import type { ProjectDto } from '../types/project.types';
import type { TaskDto } from '../types/task.types';

export const usersApi = {
  getAll: (page = 1, pageSize = 20, search?: string) =>
    apiClient.get<PagedResult<UserDto>>('/users', { params: { page, pageSize, search } }).then(r => r.data),
  getById: (id: string) =>
    apiClient.get<UserDto>(`/users/${id}`).then(r => r.data),
  getUserProjects: (id: string) =>
    apiClient.get<ProjectDto[]>(`/users/${id}/projects`).then(r => r.data),
  getUserTasks: (id: string) =>
    apiClient.get<TaskDto[]>(`/users/${id}/tasks`).then(r => r.data),
  sync: (email: string, firstName: string, lastName: string) =>
    apiClient.post<string>('/users', { email, firstName, lastName }).then(r => r.data),
  update: (id: string, data: UpdateUserRequest) =>
    apiClient.put(`/users/${id}`, { id, ...data }),
  deactivate: (id: string) =>
    apiClient.delete(`/users/${id}`),
};
