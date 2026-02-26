import { useQuery } from '@tanstack/react-query';
import { reportsApi } from '../api/reports.api';

export function useProjectReport(id: string) {
  return useQuery({
    queryKey: ['reports', 'project', id],
    queryFn: () => reportsApi.getProjectReport(id),
    enabled: !!id,
  });
}

export function useUserReport(id: string) {
  return useQuery({
    queryKey: ['reports', 'user', id],
    queryFn: () => reportsApi.getUserReport(id),
    enabled: !!id,
  });
}

export function useOverallReport() {
  return useQuery({
    queryKey: ['reports', 'overall'],
    queryFn: () => reportsApi.getOverallReport(),
  });
}
