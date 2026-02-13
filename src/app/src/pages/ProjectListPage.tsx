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
import { useProjects, useCreateProject, useUpdateProject, useArchiveProject } from '../hooks/useProjects';
import { useCustomers } from '../hooks/useCustomers';
import type { ProjectDto } from '../types/project.types';

const projectSchema = z.object({
  customerId: z.string().min(1, 'Customer is required'),
  parentId: z.string().optional(),
  name: z.string().min(1, 'Name is required'),
  code: z.string().min(1, 'Code is required'),
  description: z.string().optional(),
  startDate: z.string().min(1, 'Start date is required'),
  endDate: z.string().optional(),
});
type ProjectFormData = z.infer<typeof projectSchema>;

export default function ProjectListPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const page = Number(searchParams.get('page') ?? '1');
  const search = searchParams.get('search') ?? '';
  const customerFilter = searchParams.get('customerId') ?? '';

  const { data, isLoading } = useProjects(page, 20, search || undefined, customerFilter || undefined);
  const { data: customersData } = useCustomers(1, 100);
  const { data: projectsForParent } = useProjects(1, 100, undefined, customerFilter || undefined);
  const createProject = useCreateProject();
  const updateProject = useUpdateProject();
  const archiveProject = useArchiveProject();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<ProjectDto | null>(null);
  const [archiveTarget, setArchiveTarget] = useState<ProjectDto | null>(null);

  const { register, handleSubmit, reset, control, formState: { errors } } = useForm<ProjectFormData>({
    resolver: zodResolver(projectSchema),
  });

  useEffect(() => {
    if (editing) {
      reset({
        customerId: editing.customerId,
        parentId: editing.parentId ?? '',
        name: editing.name,
        code: editing.code,
        description: editing.description ?? '',
        startDate: editing.startDate.split('T')[0],
        endDate: editing.endDate?.split('T')[0] ?? '',
      });
    } else {
      reset({ customerId: '', parentId: '', name: '', code: '', description: '', startDate: '', endDate: '' });
    }
  }, [editing, reset]);

  const openAdd = () => { setEditing(null); setDialogOpen(true); };
  const openEdit = (p: ProjectDto) => { setEditing(p); setDialogOpen(true); };
  const closeDialog = () => { setDialogOpen(false); setEditing(null); };

  const onSubmit = async (formData: ProjectFormData) => {
    if (editing) {
      await updateProject.mutateAsync({
        id: editing.id,
        data: {
          name: formData.name,
          code: formData.code,
          description: formData.description,
          isActive: editing.isActive,
          startDate: formData.startDate,
          endDate: formData.endDate,
        },
      });
    } else {
      await createProject.mutateAsync({
        customerId: formData.customerId,
        parentId: formData.parentId || undefined,
        name: formData.name,
        code: formData.code,
        description: formData.description,
        startDate: formData.startDate,
        endDate: formData.endDate,
      });
    }
    closeDialog();
  };

  const onArchive = async () => {
    if (archiveTarget) {
      await archiveProject.mutateAsync(archiveTarget.id);
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

  const setCustomerFilter = (cid: string) => {
    const params = new URLSearchParams(searchParams);
    if (cid) params.set('customerId', cid); else params.delete('customerId');
    params.set('page', '1');
    setSearchParams(params);
  };

  return (
    <>
      <PageHeader title="Projects" onAdd={openAdd} addLabel="New Project" />

      <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
        <TextField
          size="small"
          placeholder="Search projects…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel>Filter by Customer</InputLabel>
          <Select
            value={customerFilter}
            label="Filter by Customer"
            onChange={(e) => setCustomerFilter(e.target.value)}
          >
            <MenuItem value="">All Customers</MenuItem>
            {customersData?.items.map((c) => (
              <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      <LoadingOverlay open={isLoading} />

      {!isLoading && (!data || data.items.length === 0) ? (
        <EmptyState message="No projects found" />
      ) : (
        <Paper>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Code</TableCell>
                <TableCell>Customer</TableCell>
                <TableCell>Status</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.items.map((project) => (
                <TableRow
                  key={project.id}
                  hover
                  sx={{ cursor: 'pointer' }}
                  onClick={() => navigate(`/projects/${project.id}`)}
                >
                  <TableCell>{project.name}</TableCell>
                  <TableCell>{project.code}</TableCell>
                  <TableCell>{project.customerName ?? '—'}</TableCell>
                  <TableCell>
                    <Chip
                      label={project.isActive ? 'Active' : 'Archived'}
                      color={project.isActive ? 'success' : 'default'}
                      size="small"
                    />
                  </TableCell>
                  <TableCell align="right" onClick={(e) => e.stopPropagation()}>
                    <IconButton size="small" onClick={() => openEdit(project)}>
                      <EditIcon fontSize="small" />
                    </IconButton>
                    {project.isActive && (
                      <IconButton size="small" onClick={() => setArchiveTarget(project)}>
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
          <DialogTitle>{editing ? 'Edit Project' : 'Add Project'}</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: '8px !important' }}>
            <Controller
              name="customerId"
              control={control}
              render={({ field }) => (
                <FormControl fullWidth error={!!errors.customerId} disabled={!!editing}>
                  <InputLabel>Customer</InputLabel>
                  <Select {...field} label="Customer">
                    {customersData?.items.map((c) => (
                      <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>
                    ))}
                  </Select>
                </FormControl>
              )}
            />
            <Controller
              name="parentId"
              control={control}
              render={({ field }) => (
                <FormControl fullWidth disabled={!!editing}>
                  <InputLabel>Parent Project (optional)</InputLabel>
                  <Select {...field} label="Parent Project (optional)">
                    <MenuItem value="">None</MenuItem>
                    {projectsForParent?.items
                      .filter((p) => p.id !== editing?.id)
                      .map((p) => (
                        <MenuItem key={p.id} value={p.id}>{p.name}</MenuItem>
                      ))}
                  </Select>
                </FormControl>
              )}
            />
            <TextField
              label="Name"
              {...register('name')}
              error={!!errors.name}
              helperText={errors.name?.message}
              fullWidth
            />
            <TextField
              label="Code"
              {...register('code')}
              error={!!errors.code}
              helperText={errors.code?.message}
              fullWidth
            />
            <TextField
              label="Description"
              {...register('description')}
              multiline
              rows={2}
              fullWidth
            />
            <TextField
              label="Start Date"
              type="date"
              {...register('startDate')}
              error={!!errors.startDate}
              helperText={errors.startDate?.message}
              InputLabelProps={{ shrink: true }}
              fullWidth
            />
            <TextField
              label="End Date"
              type="date"
              {...register('endDate')}
              InputLabelProps={{ shrink: true }}
              fullWidth
            />
          </DialogContent>
          <DialogActions>
            <Button onClick={closeDialog}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={createProject.isPending || updateProject.isPending}>
              {editing ? 'Save' : 'Create'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>

      {/* Archive Confirm */}
      <ConfirmDialog
        open={!!archiveTarget}
        title="Archive Project"
        message={`Are you sure you want to archive "${archiveTarget?.name}"?`}
        onConfirm={onArchive}
        onCancel={() => setArchiveTarget(null)}
      />
    </>
  );
}
