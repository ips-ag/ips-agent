import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { unitsApi } from '../api/units.api';
import type { CreateUnitRequest, UpdateUnitRequest } from '../types/unit.types';

export function useUnits(page = 1, pageSize = 20, search?: string) {
  return useQuery({
    queryKey: ['units', page, pageSize, search],
    queryFn: () => unitsApi.getAll(page, pageSize, search),
  });
}

export function useUnit(id: string) {
  return useQuery({
    queryKey: ['units', id],
    queryFn: () => unitsApi.getById(id),
    enabled: !!id,
  });
}

export function useCreateUnit() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateUnitRequest) => unitsApi.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['units'] }),
  });
}

export function useUpdateUnit() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateUnitRequest }) => unitsApi.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['units'] }),
  });
}

export function useArchiveUnit() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => unitsApi.archive(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['units'] }),
  });
}
