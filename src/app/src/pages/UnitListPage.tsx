import { useState, useEffect } from 'react';
import {
  Paper, Table, TableHead, TableRow, TableCell, TableBody,
  TextField, Chip, IconButton, Pagination, Dialog, DialogTitle,
  DialogContent, DialogActions, Button, Box,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import ArchiveIcon from '@mui/icons-material/Archive';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import ConfirmDialog from '../components/common/ConfirmDialog';
import EmptyState from '../components/common/EmptyState';
import { useUnits, useCreateUnit, useUpdateUnit, useArchiveUnit } from '../hooks/useUnits';
import type { UnitDto } from '../types/unit.types';

const unitSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  description: z.string().optional(),
});
type UnitFormData = z.infer<typeof unitSchema>;

export default function UnitListPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const page = Number(searchParams.get('page') ?? '1');
  const search = searchParams.get('search') ?? '';

  const { data, isLoading } = useUnits(page, 20, search || undefined);
  const createUnit = useCreateUnit();
  const updateUnit = useUpdateUnit();
  const archiveUnit = useArchiveUnit();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<UnitDto | null>(null);
  const [archiveTarget, setArchiveTarget] = useState<UnitDto | null>(null);

  const { register, handleSubmit, reset, formState: { errors } } = useForm<UnitFormData>({
    resolver: zodResolver(unitSchema),
  });

  useEffect(() => {
    if (editing) {
      reset({ name: editing.name, description: editing.description ?? '' });
    } else {
      reset({ name: '', description: '' });
    }
  }, [editing, reset]);

  const openAdd = () => { setEditing(null); setDialogOpen(true); };
  const openEdit = (u: UnitDto) => { setEditing(u); setDialogOpen(true); };
  const closeDialog = () => { setDialogOpen(false); setEditing(null); };

  const onSubmit = async (data: UnitFormData) => {
    if (editing) {
      await updateUnit.mutateAsync({ id: editing.id, data: { ...data, isActive: editing.isActive } });
    } else {
      await createUnit.mutateAsync(data);
    }
    closeDialog();
  };

  const onArchive = async () => {
    if (archiveTarget) {
      await archiveUnit.mutateAsync(archiveTarget.id);
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

  return (
    <>
      <PageHeader title="Units" onAdd={openAdd} addLabel="New Unit" />

      <Box sx={{ mb: 2 }}>
        <TextField
          size="small"
          placeholder="Search units…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </Box>

      <LoadingOverlay open={isLoading} />

      {!isLoading && (!data || data.items.length === 0) ? (
        <EmptyState message="No units found" />
      ) : (
        <Paper>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Description</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.items.map((unit) => (
                <TableRow
                  key={unit.id}
                  hover
                  sx={{ cursor: 'pointer' }}
                  onClick={() => navigate(`/units/${unit.id}`)}
                >
                  <TableCell>{unit.name}</TableCell>
                  <TableCell>{unit.description ?? '—'}</TableCell>
                  <TableCell>
                    <Chip
                      label={unit.isActive ? 'Active' : 'Archived'}
                      color={unit.isActive ? 'success' : 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell align="right" onClick={(e) => e.stopPropagation()}>
                    <IconButton size="small" onClick={() => openEdit(unit)}>
                      <EditIcon fontSize="small" />
                    </IconButton>
                    {unit.isActive && (
                      <IconButton size="small" onClick={() => setArchiveTarget(unit)}>
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
          <DialogTitle>{editing ? 'Edit Unit' : 'Add Unit'}</DialogTitle>
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
              rows={3}
              fullWidth
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={closeDialog}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={createUnit.isPending || updateUnit.isPending}>
              {editing ? 'Save' : 'Create'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>

      {/* Archive Confirm */}
      <ConfirmDialog
        open={!!archiveTarget}
        title="Archive Unit"
        message={`Are you sure you want to archive "${archiveTarget?.name}"?`}
        onConfirm={onArchive}
        onCancel={() => setArchiveTarget(null)}
      />
    </>
  );
}
