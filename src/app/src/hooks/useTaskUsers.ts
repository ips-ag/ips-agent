import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { tasksApi } from '../api/tasks.api';

export function useTaskUsers(taskId: string) {
  return useQuery({
    queryKey: ['tasks', taskId, 'users'],
    queryFn: () => tasksApi.getTaskUsers(taskId),
    enabled: !!taskId,
  });
}

export function useAssignUserToTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ taskId, userId }: { taskId: string; userId: string }) =>
      tasksApi.assignUser(taskId, userId),
    onSuccess: (_data, { taskId }) =>
      qc.invalidateQueries({ queryKey: ['tasks', taskId, 'users'] }),
  });
}

export function useRemoveUserFromTask() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ taskId, userId }: { taskId: string; userId: string }) =>
      tasksApi.removeUser(taskId, userId),
    onSuccess: (_data, { taskId }) =>
      qc.invalidateQueries({ queryKey: ['tasks', taskId, 'users'] }),
  });
}
