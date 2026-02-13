import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { customersApi } from '../api/customers.api';
import type { CreateCustomerRequest, UpdateCustomerRequest } from '../types/customer.types';

export function useCustomers(page = 1, pageSize = 20, search?: string, unitId?: string) {
  return useQuery({
    queryKey: ['customers', page, pageSize, search, unitId],
    queryFn: () => customersApi.getAll(page, pageSize, search, unitId),
  });
}

export function useCustomer(id: string) {
  return useQuery({
    queryKey: ['customers', id],
    queryFn: () => customersApi.getById(id),
    enabled: !!id,
  });
}

export function useCreateCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateCustomerRequest) => customersApi.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['customers'] }),
  });
}

export function useUpdateCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCustomerRequest }) => customersApi.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['customers'] }),
  });
}

export function useArchiveCustomer() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => customersApi.archive(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['customers'] }),
  });
}
