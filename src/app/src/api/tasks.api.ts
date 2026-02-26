import apiClient from './client';
import type { PagedResult } from '../types/common.types';
import type { TaskDto, CreateTaskRequest, UpdateTaskRequest } from '../types/task.types';
import type { UserDto } from '../types/user.types';

export const tasksApi = {
  getAll: (page = 1, pageSize = 20, search?: string, projectId?: string) =>
    apiClient.get<PagedResult<TaskDto>>('/tasks', { params: { page, pageSize, search, projectId } }).then(r => r.data),
  getById: (id: string) =>
    apiClient.get<TaskDto>(`/tasks/${id}`).then(r => r.data),
  getTaskUsers: (id: string) =>
    apiClient.get<UserDto[]>(`/tasks/${id}/users`).then(r => r.data),
  create: (data: CreateTaskRequest) =>
    apiClient.post<string>('/tasks', data).then(r => r.data),
  update: (id: string, data: UpdateTaskRequest) =>
    apiClient.put(`/tasks/${id}`, { id, ...data }),
  archive: (id: string) =>
    apiClient.delete(`/tasks/${id}`),
  assignUser: (taskId: string, userId: string) =>
    apiClient.post(`/tasks/${taskId}/users`, { userId }),
  removeUser: (taskId: string, userId: string) =>
    apiClient.delete(`/tasks/${taskId}/users/${userId}`),
};
