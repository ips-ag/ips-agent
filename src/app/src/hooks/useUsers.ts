import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { usersApi } from '../api/users.api';
import type { UpdateUserRequest } from '../types/user.types';

export function useUsers(page = 1, pageSize = 20, search?: string) {
  return useQuery({
    queryKey: ['users', page, pageSize, search],
    queryFn: () => usersApi.getAll(page, pageSize, search),
  });
}

export function useUser(id: string) {
  return useQuery({
    queryKey: ['users', id],
    queryFn: () => usersApi.getById(id),
    enabled: !!id,
  });
}

export function useSyncUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ email, firstName, lastName }: { email: string; firstName: string; lastName: string }) =>
      usersApi.sync(email, firstName, lastName),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  });
}

export function useUpdateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateUserRequest }) => usersApi.update(id, data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  });
}

export function useDeactivateUser() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => usersApi.deactivate(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['users'] }),
  });
}
