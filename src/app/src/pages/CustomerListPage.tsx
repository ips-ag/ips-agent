import { useState, useEffect } from 'react';
import {
  Paper, Table, TableHead, TableRow, TableCell, TableBody,
  TextField, Chip, IconButton, Pagination, Dialog, DialogTitle,
  DialogContent, DialogActions, Button, Box, FormControl, InputLabel,
  Select, MenuItem,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import ArchiveIcon from '@mui/icons-material/Archive';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import ConfirmDialog from '../components/common/ConfirmDialog';
import EmptyState from '../components/common/EmptyState';
import { useCustomers, useCreateCustomer, useUpdateCustomer, useArchiveCustomer } from '../hooks/useCustomers';
import { useUnits } from '../hooks/useUnits';
import type { CustomerDto } from '../types/customer.types';

const customerSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  description: z.string().optional(),
  unitId: z.string().min(1, 'Unit is required'),
  contactEmail: z.string().email('Invalid email').optional().or(z.literal('')),
  contactPhone: z.string().optional(),
});
type CustomerFormData = z.infer<typeof customerSchema>;

export default function CustomerListPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const page = Number(searchParams.get('page') ?? '1');
  const search = searchParams.get('search') ?? '';
  const unitFilter = searchParams.get('unitId') ?? '';

  const { data, isLoading } = useCustomers(page, 20, search || undefined, unitFilter || undefined);
  const { data: unitsData } = useUnits(1, 100);
  const createCustomer = useCreateCustomer();
  const updateCustomer = useUpdateCustomer();
  const archiveCustomer = useArchiveCustomer();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<CustomerDto | null>(null);
  const [archiveTarget, setArchiveTarget] = useState<CustomerDto | null>(null);

  const { register, handleSubmit, reset, control, formState: { errors } } = useForm<CustomerFormData>({
    resolver: zodResolver(customerSchema),
  });

  useEffect(() => {
    if (editing) {
      reset({
        name: editing.name,
        description: editing.description ?? '',
        unitId: editing.unitId,
        contactEmail: editing.contactEmail ?? '',
        contactPhone: editing.contactPhone ?? '',
      });
    } else {
      reset({ name: '', description: '', unitId: '', contactEmail: '', contactPhone: '' });
    }
  }, [editing, reset]);

  const openAdd = () => { setEditing(null); setDialogOpen(true); };
  const openEdit = (c: CustomerDto) => { setEditing(c); setDialogOpen(true); };
  const closeDialog = () => { setDialogOpen(false); setEditing(null); };

  const onSubmit = async (formData: CustomerFormData) => {
    if (editing) {
      await updateCustomer.mutateAsync({
        id: editing.id,
        data: {
          name: formData.name,
          description: formData.description,
          contactEmail: formData.contactEmail,
          contactPhone: formData.contactPhone,
          isActive: editing.isActive,
        },
      });
    } else {
      await createCustomer.mutateAsync({
        unitId: formData.unitId,
        name: formData.name,
        description: formData.description,
        contactEmail: formData.contactEmail,
        contactPhone: formData.contactPhone,
      });
    }
    closeDialog();
  };

  const onArchive = async () => {
    if (archiveTarget) {
      await archiveCustomer.mutateAsync(archiveTarget.id);
      setArchiveTarget(null);
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

  const setUnitFilter = (uid: string) => {
    const params = new URLSearchParams(searchParams);
    if (uid) params.set('unitId', uid); else params.delete('unitId');
    params.set('page', '1');
    setSearchParams(params);
  };

  return (
    <>
      <PageHeader title="Customers" onAdd={openAdd} addLabel="New Customer" />

      <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
        <TextField
          size="small"
          placeholder="Search customers…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel>Filter by Unit</InputLabel>
          <Select
            value={unitFilter}
            label="Filter by Unit"
            onChange={(e) => setUnitFilter(e.target.value)}
          >
            <MenuItem value="">All Units</MenuItem>
            {unitsData?.items.map((u) => (
              <MenuItem key={u.id} value={u.id}>{u.name}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      <LoadingOverlay open={isLoading} />

      {!isLoading && (!data || data.items.length === 0) ? (
        <EmptyState message="No customers found" />
      ) : (
        <Paper>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Unit</TableCell>
                <TableCell>Contact Email</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.items.map((customer) => (
                <TableRow
                  key={customer.id}
                  hover
                  sx={{ cursor: 'pointer' }}
                  onClick={() => navigate(`/customers/${customer.id}`)}
                >
                  <TableCell>{customer.name}</TableCell>
                  <TableCell>{customer.unitName ?? '—'}</TableCell>
                  <TableCell>{customer.contactEmail ?? '—'}</TableCell>
                  <TableCell>
                    <Chip
                      label={customer.isActive ? 'Active' : 'Archived'}
                      color={customer.isActive ? 'success' : 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell align="right" onClick={(e) => e.stopPropagation()}>
                    <IconButton size="small" onClick={() => openEdit(customer)}>
                      <EditIcon fontSize="small" />
                    </IconButton>
                    {customer.isActive && (
                      <IconButton size="small" onClick={() => setArchiveTarget(customer)}>
                        <ArchiveIcon fontSize="small" />
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

      {/* Add / Edit Dialog */}
      <Dialog open={dialogOpen} onClose={closeDialog} fullWidth maxWidth="sm">
        <form onSubmit={handleSubmit(onSubmit)}>
          <DialogTitle>{editing ? 'Edit Customer' : 'Add Customer'}</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: '8px !important' }}>
            <TextField
              label="Name"
              {...register('name')}
              error={!!errors.name}
              helperText={errors.name?.message}
              fullWidth
            />
            <TextField
              label="Description"
              {...register('description')}
              multiline
              rows={2}
              fullWidth
            />
            <Controller
              name="unitId"
              control={control}
              render={({ field }) => (
                <FormControl fullWidth error={!!errors.unitId} disabled={!!editing}>
                  <InputLabel>Unit</InputLabel>
                  <Select {...field} label="Unit">
                    {unitsData?.items.map((u) => (
                      <MenuItem key={u.id} value={u.id}>{u.name}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              )}
            />
            <TextField
              label="Contact Email"
              {...register('contactEmail')}
              error={!!errors.contactEmail}
              helperText={errors.contactEmail?.message}
              fullWidth
            />
            <TextField
              label="Contact Phone"
              {...register('contactPhone')}
              fullWidth
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={closeDialog}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={createCustomer.isPending || updateCustomer.isPending}>
              {editing ? 'Save' : 'Create'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>

      {/* Archive Confirm */}
      <ConfirmDialog
        open={!!archiveTarget}
        title="Archive Customer"
        message={`Are you sure you want to archive "${archiveTarget?.name}"?`}
        onConfirm={onArchive}
        onCancel={() => setArchiveTarget(null)}
      />
    </>
  );
}
