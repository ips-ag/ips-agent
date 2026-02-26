import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { projectsApi } from '../api/projects.api';

export function useProjectUsers(projectId: string) {
  return useQuery({
    queryKey: ['projects', projectId, 'users'],
    queryFn: () => projectsApi.getProjectUsers(projectId),
    enabled: !!projectId,
  });
}

export function useAssignUserToProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, userId }: { projectId: string; userId: string }) =>
      projectsApi.assignUser(projectId, userId),
    onSuccess: (_data, { projectId }) =>
      qc.invalidateQueries({ queryKey: ['projects', projectId, 'users'] }),
  });
}

export function useRemoveUserFromProject() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ projectId, userId }: { projectId: string; userId: string }) =>
      projectsApi.removeUser(projectId, userId),
    onSuccess: (_data, { projectId }) =>
      qc.invalidateQueries({ queryKey: ['projects', projectId, 'users'] }),
  });
}
