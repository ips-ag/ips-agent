import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { timeEntriesApi, type TimeEntryFilters } from '../api/timeEntries.api';
import type { CreateTimeEntryRequest, UpdateTimeEntryRequest } from '../types/timeEntry.types';

export function useTimeEntries(filters: TimeEntryFilters = {}) {
  return useQuery({
    queryKey: ['timeEntries', filters],
    queryFn: () => timeEntriesApi.getAll(filters),
  });
}

export function useTimeEntry(id: string) {
  return useQuery({
    queryKey: ['timeEntries', id],
    queryFn: () => timeEntriesApi.getById(id),
    enabled: !!id,
  });
}

export function useCreateTimeEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateTimeEntryRequest) => timeEntriesApi.create(data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['timeEntries'] });
      qc.invalidateQueries({ queryKey: ['timesheets'] });
    },
  });
}

export function useUpdateTimeEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateTimeEntryRequest }) => timeEntriesApi.update(id, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['timeEntries'] });
      qc.invalidateQueries({ queryKey: ['timesheets'] });
    },
  });
}

export function useDeleteTimeEntry() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => timeEntriesApi.remove(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['timeEntries'] });
      qc.invalidateQueries({ queryKey: ['timesheets'] });
    },
  });
}
