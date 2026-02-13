import { useState, useEffect } from 'react';
import {
  Paper, Table, TableHead, TableRow, TableCell, TableBody,
  TextField, Chip, IconButton, Pagination, Dialog, DialogTitle,
  DialogContent, DialogActions, Button, Box, FormControl, InputLabel,
  Select, MenuItem,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import BlockIcon from '@mui/icons-material/Block';
import { useSearchParams } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import ConfirmDialog from '../components/common/ConfirmDialog';
import EmptyState from '../components/common/EmptyState';
import { useUsers, useUpdateUser, useDeactivateUser } from '../hooks/useUsers';
import type { UserDto } from '../types/user.types';

const roleSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  role: z.enum(['Admin', 'Manager', 'Employee']),
});
type RoleFormData = z.infer<typeof roleSchema>;

const roleColors: Record<string, 'error' | 'warning' | 'info'> = {
  Admin: 'error',
  Manager: 'warning',
  Employee: 'info',
};

export default function UserListPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const page = Number(searchParams.get('page') ?? '1');
  const search = searchParams.get('search') ?? '';

  const { data, isLoading } = useUsers(page, 20, search || undefined);
  const updateUser = useUpdateUser();
  const deactivateUser = useDeactivateUser();

  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [editing, setEditing] = useState<UserDto | null>(null);
  const [deactivateTarget, setDeactivateTarget] = useState<UserDto | null>(null);

  const { register, handleSubmit, reset, control, formState: { errors } } = useForm<RoleFormData>({
    resolver: zodResolver(roleSchema),
  });

  useEffect(() => {
    if (editing) {
      reset({ firstName: editing.firstName, lastName: editing.lastName, role: editing.role });
    }
  }, [editing, reset]);

  const openEdit = (u: UserDto) => { setEditing(u); setEditDialogOpen(true); };
  const closeDialog = () => { setEditDialogOpen(false); setEditing(null); };

  const onSubmit = async (formData: RoleFormData) => {
    if (editing) {
      await updateUser.mutateAsync({
        id: editing.id,
        data: {
          firstName: formData.firstName,
          lastName: formData.lastName,
          role: formData.role,
          isActive: editing.isActive,
        },
      });
    }
    closeDialog();
  };

  const onDeactivate = async () => {
    if (deactivateTarget) {
      await deactivateUser.mutateAsync(deactivateTarget.id);
      setDeactivateTarget(null);
    }
  };

  const setPage = (p: number) => {
    const params = new URLSearchParams(searchParams);
    params.set('page', String(p));
    setSearchParams(params);
  };

  const setSearch = (s: string) => {
    const params = new URLSearchParams(searchParams);
    if (s) params.set('search', s); else params.delete('search');
    params.set('page', '1');
    setSearchParams(params);
  };

  return (
    <>
      <PageHeader title="Users" />

      <Box sx={{ mb: 2 }}>
        <TextField
          size="small"
          placeholder="Search users…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </Box>

      <LoadingOverlay open={isLoading} />

      {!isLoading && (!data || data.items.length === 0) ? (
        <EmptyState message="No users found" />
      ) : (
        <Paper>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Email</TableCell>
                <TableCell>Role</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.items.map((user) => (
                <TableRow key={user.id} hover>
                  <TableCell>{user.firstName} {user.lastName}</TableCell>
                  <TableCell>{user.email}</TableCell>
                  <TableCell>
                    <Chip label={user.role} color={roleColors[user.role] ?? 'default'} size="small" />
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={user.isActive ? 'Active' : 'Inactive'}
                      color={user.isActive ? 'success' : 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => openEdit(user)}>
                      <EditIcon fontSize="small" />
                    </IconButton>
                    {user.isActive && (
                      <IconButton size="small" color="error" onClick={() => setDeactivateTarget(user)}>
                        <BlockIcon fontSize="small" />
                      </IconButton>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>
      )}

      {data && data.totalPages > 1 && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
          <Pagination count={data.totalPages} page={page} onChange={(_, p) => setPage(p)} />
        </Box>
      )}

      {/* Edit User Dialog */}
      <Dialog open={editDialogOpen} onClose={closeDialog} fullWidth maxWidth="sm">
        <form onSubmit={handleSubmit(onSubmit)}>
          <DialogTitle>Edit User</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: '8px !important' }}>
            <TextField
              label="First Name"
              {...register('firstName')}
              error={!!errors.firstName}
              helperText={errors.firstName?.message}
              fullWidth
            />
            <TextField
              label="Last Name"
              {...register('lastName')}
              error={!!errors.lastName}
              helperText={errors.lastName?.message}
              fullWidth
            />
            <Controller
              name="role"
              control={control}
              render={({ field }) => (
                <FormControl fullWidth>
                  <InputLabel>Role</InputLabel>
                  <Select {...field} label="Role">
                    <MenuItem value="Admin">Admin</MenuItem>
                    <MenuItem value="Manager">Manager</MenuItem>
                    <MenuItem value="Employee">Employee</MenuItem>
                  </Select>
                </FormControl>
              )}
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={closeDialog}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={updateUser.isPending}>Save</Button>
          </DialogActions>
        </form>
      </Dialog>

      {/* Deactivate Confirm */}
      <ConfirmDialog
        open={!!deactivateTarget}
        title="Deactivate User"
        message={`Are you sure you want to deactivate ${deactivateTarget?.firstName} ${deactivateTarget?.lastName}?`}
        onConfirm={onDeactivate}
        onCancel={() => setDeactivateTarget(null)}
      />
    </>
  );
}
