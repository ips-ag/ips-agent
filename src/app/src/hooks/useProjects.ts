import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsApi } from '../api/projects.api';
import type { CreateProjectRequest, UpdateProjectRequest } from '../types/project.types';

export function useProjects(page = 1, pageSize = 20, search?: string, customerId?: string) {
  return useQuery({
    queryKey: ['projects', page, pageSize, search, customerId],
    queryFn: () => projectsApi.getAll(page, pageSize, search, customerId),
  });
}

export function useProject(id: string) {
  return useQuery({
    queryKey: ['projects', id],
    queryFn: () => projectsApi.getById(id),
    enabled: !!id,
  });
}

export function useProjectHierarchy(id: string) {
  return useQuery({
    queryKey: ['projects', id, 'hierarchy'],
    queryFn: () => projectsApi.getHierarchy(id),
    enabled: !!id,
  });
}

export function useCreateProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateProjectRequest) => projectsApi.create(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['projects'] }),
  });
}

export function useUpdateProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProjectRequest }) => projectsApi.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['projects'] }),
  });
}

export function useArchiveProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => projectsApi.archive(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['projects'] }),
  });
}
