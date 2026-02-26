import apiClient from './client';
import type { PagedResult } from '../types/common.types';
import type { CustomerDto, CreateCustomerRequest, UpdateCustomerRequest } from '../types/customer.types';

export const customersApi = {
  getAll: (page = 1, pageSize = 20, search?: string, unitId?: string) =>
    apiClient.get<PagedResult<CustomerDto>>('/customers', { params: { page, pageSize, search, unitId } }).then(r => r.data),
  getById: (id: string) =>
    apiClient.get<CustomerDto>(`/customers/${id}`).then(r => r.data),
  create: (data: CreateCustomerRequest) =>
    apiClient.post<string>('/customers', data).then(r => r.data),
  update: (id: string, data: UpdateCustomerRequest) =>
    apiClient.put(`/customers/${id}`, { id, ...data }),
  archive: (id: string) =>
    apiClient.delete(`/customers/${id}`),
};
