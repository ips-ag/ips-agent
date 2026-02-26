import { useQuery } from '@tanstack/react-query';
import { usersApi } from '../api/users.api';

export function useUserTasks(userId: string) {
  return useQuery({
    queryKey: ['users', userId, 'tasks'],
    queryFn: () => usersApi.getUserTasks(userId),
    enabled: !!userId,
  });
}
