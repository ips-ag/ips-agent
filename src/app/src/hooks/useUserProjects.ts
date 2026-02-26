import { useQuery } from '@tanstack/react-query';
import { usersApi } from '../api/users.api';

export function useUserProjects(userId: string) {
  return useQuery({
    queryKey: ['users', userId, 'projects'],
    queryFn: () => usersApi.getUserProjects(userId),
    enabled: !!userId,
  });
}
