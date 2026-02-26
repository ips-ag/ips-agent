import apiClient from './client';
import type { PagedResult } from '../types/common.types';
import type { UnitDto, CreateUnitRequest, UpdateUnitRequest } from '../types/unit.types';

export const unitsApi = {
  getAll: (page = 1, pageSize = 20, search?: string) =>
    apiClient.get<PagedResult<UnitDto>>('/units', { params: { page, pageSize, search } }).then(r => r.data),
  getById: (id: string) =>
    apiClient.get<UnitDto>(`/units/${id}`).then(r => r.data),
  create: (data: CreateUnitRequest) =>
    apiClient.post<string>('/units', data).then(r => r.data),
  update: (id: string, data: UpdateUnitRequest) =>
    apiClient.put(`/units/${id}`, { id, ...data }),
  archive: (id: string) =>
    apiClient.delete(`/units/${id}`),
};
