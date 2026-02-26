import apiClient from './client';
import type { PagedResult } from '../types/common.types';
import type { TimeEntryDto, CreateTimeEntryRequest, UpdateTimeEntryRequest } from '../types/timeEntry.types';

export interface TimeEntryFilters {
  page?: number;
  pageSize?: number;
  userId?: string;
  taskId?: string;
  dateFrom?: string;
  dateTo?: string;
}

export const timeEntriesApi = {
  getAll: (filters: TimeEntryFilters = {}) => {
    const { page = 1, pageSize = 20, ...rest } = filters;
    return apiClient.get<PagedResult<TimeEntryDto>>('/timeentries', { params: { page, pageSize, ...rest } }).then(r => r.data);
  },
  getById: (id: string) =>
    apiClient.get<TimeEntryDto>(`/timeentries/${id}`).then(r => r.data),
  create: (data: CreateTimeEntryRequest) =>
    apiClient.post<string>('/timeentries', data).then(r => r.data),
  update: (id: string, data: UpdateTimeEntryRequest) =>
    apiClient.put(`/timeentries/${id}`, { id, ...data }),
  remove: (id: string) =>
    apiClient.delete(`/timeentries/${id}`),
};
