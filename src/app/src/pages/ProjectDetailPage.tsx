import { useState } from 'react';
import {
  Paper, Table, TableHead, TableRow, TableCell, TableBody,
  Card, CardContent, Typography, Chip, Box, Button, Dialog,
  DialogTitle, DialogContent, DialogActions, TextField, FormControl,
  InputLabel, Select, MenuItem, IconButton,
} from '@mui/material';
import Tab from '@mui/material/Tab';
import TabContext from '@mui/lab/TabContext';
import TabList from '@mui/lab/TabList';
import TabPanel from '@mui/lab/TabPanel';
import DeleteIcon from '@mui/icons-material/Delete';
import PersonAddIcon from '@mui/icons-material/PersonAdd';
import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import EmptyState from '../components/common/EmptyState';
import ConfirmDialog from '../components/common/ConfirmDialog';
import { useProject, useProjects } from '../hooks/useProjects';
import { useTasks, useCreateTask } from '../hooks/useTasks';
import { useProjectUsers, useAssignUserToProject, useRemoveUserFromProject } from '../hooks/useProjectUsers';
import { useUsers } from '../hooks/useUsers';

const taskSchema = z.object({
  name: z.string().min(1, 'Name is required'),
  code: z.string().min(1, 'Code is required'),
  description: z.string().optional(),
  startDate: z.string().optional(),
  endDate: z.string().optional(),
});
type TaskFormData = z.infer<typeof taskSchema>;

export default function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: project, isLoading } = useProject(id!);
  const { data: children } = useProjects(1, 50, undefined, undefined);
  const { data: tasks } = useTasks(1, 50, undefined, id);
  const { data: projectUsers } = useProjectUsers(id!);
  const { data: allUsers } = useUsers(1, 200);
  const createTask = useCreateTask();
  const assignUser = useAssignUserToProject();
  const removeUser = useRemoveUserFromProject();

  const [tab, setTab] = useState('info');
  const [taskDialogOpen, setTaskDialogOpen] = useState(false);
  const [assignDialogOpen, setAssignDialogOpen] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState('');
  const [removeTarget, setRemoveTarget] = useState<{ userId: string; name: string } | null>(null);

  const { register, handleSubmit, reset, formState: { errors } } = useForm<TaskFormData>({
    resolver: zodResolver(taskSchema),
  });

  if (isLoading) return <LoadingOverlay />;
  if (!project) return <EmptyState message="Project not found" />;

  const childProjects = children?.items.filter((p) => p.parentId === id) ?? [];

  const onCreateTask = async (data: TaskFormData) => {
    await createTask.mutateAsync({
      projectId: id!,
      name: data.name,
      code: data.code,
      description: data.description,
      startDate: data.startDate || undefined,
      endDate: data.endDate || undefined,
    });
    setTaskDialogOpen(false);
    reset();
  };

  const onAssignUser = async () => {
    if (selectedUserId) {
      await assignUser.mutateAsync({ projectId: id!, userId: selectedUserId });
      setAssignDialogOpen(false);
      setSelectedUserId('');
    }
  };

  const onRemoveUser = async () => {
    if (removeTarget) {
      await removeUser.mutateAsync({ projectId: id!, userId: removeTarget.userId });
      setRemoveTarget(null);
    }
  };

  const assignedUserIds = new Set((projectUsers as any[])?.map((u: any) => u.id ?? u.userId) ?? []);

  return (
    <>
      <PageHeader title={project.name} />

      <TabContext value={tab}>
        <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
          <TabList onChange={(_: React.SyntheticEvent, v: string) => setTab(v)}>
            <Tab label="Info" value="info" />
            <Tab label="Child Projects" value="children" />
            <Tab label="Tasks" value="tasks" />
            <Tab label="Users" value="users" />
          </TabList>
        </Box>

        {/* Info Tab */}
        <TabPanel value="info" sx={{ p: 0 }}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
                <Box>
                  <Typography variant="subtitle2" color="text.secondary">Name</Typography>
                  <Typography>{project.name}</Typography>
                </Box>
                <Box>
                  <Typography variant="subtitle2" color="text.secondary">Code</Typography>
                  <Typography>{project.code}</Typography>
                </Box>
                <Box>
                  <Typography variant="subtitle2" color="text.secondary">Customer</Typography>
                  <Typography>{project.customerName ?? '—'}</Typography>
                </Box>
                <Box>
                  <Typography variant="subtitle2" color="text.secondary">Status</Typography>
                  <Chip
                    label={project.isActive ? 'Active' : 'Archived'}
                    color={project.isActive ? 'success' : 'default'}
                    size="small"
                  />
                </Box>
                <Box>
                  <Typography variant="subtitle2" color="text.secondary">Start Date</Typography>
                  <Typography>{new Date(project.startDate).toLocaleDateString()}</Typography>
                </Box>
                <Box>
                  <Typography variant="subtitle2" color="text.secondary">End Date</Typography>
                  <Typography>{project.endDate ? new Date(project.endDate).toLocaleDateString() : '—'}</Typography>
                </Box>
                <Box sx={{ gridColumn: '1 / -1' }}>
                  <Typography variant="subtitle2" color="text.secondary">Description</Typography>
                  <Typography>{project.description || '—'}</Typography>
                </Box>
              </Box>
            </CardContent>
          </Card>
        </TabPanel>

        {/* Child Projects Tab */}
        <TabPanel value="children" sx={{ p: 0 }}>
          {childProjects.length === 0 ? (
            <EmptyState message="No child projects" />
          ) : (
            <Paper>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Code</TableCell>
                    <TableCell>Status</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {childProjects.map((cp) => (
                    <TableRow
                      key={cp.id}
                      hover
                      sx={{ cursor: 'pointer' }}
                      onClick={() => navigate(`/projects/${cp.id}`)}
                    >
                      <TableCell>{cp.name}</TableCell>
                      <TableCell>{cp.code}</TableCell>
                      <TableCell>
                        <Chip
                          label={cp.isActive ? 'Active' : 'Archived'}
                          color={cp.isActive ? 'success' : 'default'}
                          size="small"
                        />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Paper>
          )}
        </TabPanel>

        {/* Tasks Tab */}
        <TabPanel value="tasks" sx={{ p: 0 }}>
          <Box sx={{ mb: 2 }}>
            <Button variant="contained" onClick={() => { reset(); setTaskDialogOpen(true); }}>
              Add Task
            </Button>
          </Box>
          {!tasks || tasks.items.length === 0 ? (
            <EmptyState message="No tasks for this project" />
          ) : (
            <Paper>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Code</TableCell>
                    <TableCell>Status</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {tasks.items.map((t) => (
                    <TableRow
                      key={t.id}
                      hover
                      sx={{ cursor: 'pointer' }}
                      onClick={() => navigate(`/tasks/${t.id}`)}
                    >
                      <TableCell>{t.name}</TableCell>
                      <TableCell>{t.code}</TableCell>
                      <TableCell>
                        <Chip
                          label={t.isActive ? 'Active' : 'Archived'}
                          color={t.isActive ? 'success' : 'default'}
                          size="small"
                        />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Paper>
          )}
        </TabPanel>

        {/* Users Tab */}
        <TabPanel value="users" sx={{ p: 0 }}>
          <Box sx={{ mb: 2 }}>
            <Button variant="contained" startIcon={<PersonAddIcon />} onClick={() => setAssignDialogOpen(true)}>
              Assign User
            </Button>
          </Box>
          {!projectUsers || (projectUsers as any[]).length === 0 ? (
            <EmptyState message="No users assigned" />
          ) : (
            <Paper>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Name</TableCell>
                    <TableCell>Email</TableCell>
                    <TableCell>Role</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {(projectUsers as any[]).map((u: any) => (
                    <TableRow key={u.id ?? u.userId}>
                      <TableCell>{u.firstName ?? u.name} {u.lastName ?? ''}</TableCell>
                      <TableCell>{u.email ?? '—'}</TableCell>
                      <TableCell><Chip label={u.role ?? 'Member'} size="small" /></TableCell>
                      <TableCell align="right">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => setRemoveTarget({ userId: u.id ?? u.userId, name: `${u.firstName ?? u.name} ${u.lastName ?? ''}` })}
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Paper>
          )}
        </TabPanel>
      </TabContext>

      <Box sx={{ mt: 2 }}>
        <Button variant="outlined" onClick={() => navigate('/projects')}>Back to Projects</Button>
      </Box>

      {/* Add Task Dialog */}
      <Dialog open={taskDialogOpen} onClose={() => setTaskDialogOpen(false)} fullWidth maxWidth="sm">
        <form onSubmit={handleSubmit(onCreateTask)}>
          <DialogTitle>Add Task</DialogTitle>
          <DialogContent sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: '8px !important' }}>
            <TextField label="Name" {...register('name')} error={!!errors.name} helperText={errors.name?.message} fullWidth />
            <TextField label="Code" {...register('code')} error={!!errors.code} helperText={errors.code?.message} fullWidth />
            <TextField label="Description" {...register('description')} multiline rows={2} fullWidth />
            <TextField label="Start Date" type="date" {...register('startDate')} InputLabelProps={{ shrink: true }} fullWidth />
            <TextField label="End Date" type="date" {...register('endDate')} InputLabelProps={{ shrink: true }} fullWidth />
          </DialogContent>
          <DialogActions>
            <Button onClick={() => setTaskDialogOpen(false)}>Cancel</Button>
            <Button type="submit" variant="contained" disabled={createTask.isPending}>Create</Button>
          </DialogActions>
        </form>
      </Dialog>

      {/* Assign User Dialog */}
      <Dialog open={assignDialogOpen} onClose={() => setAssignDialogOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Assign User to Project</DialogTitle>
        <DialogContent>
          <FormControl fullWidth sx={{ mt: 1 }}>
            <InputLabel>User</InputLabel>
            <Select value={selectedUserId} label="User" onChange={(e) => setSelectedUserId(e.target.value)}>
              {allUsers?.items
                .filter((u) => !assignedUserIds.has(u.id))
                .map((u) => (
                  <MenuItem key={u.id} value={u.id}>{u.firstName} {u.lastName} ({u.email})</MenuItem>
                ))}
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAssignDialogOpen(false)}>Cancel</Button>
          <Button variant="contained" onClick={onAssignUser} disabled={!selectedUserId || assignUser.isPending}>
            Assign
          </Button>
        </DialogActions>
      </Dialog>

      {/* Remove User Confirm */}
      <ConfirmDialog
        open={!!removeTarget}
        title="Remove User"
        message={`Remove ${removeTarget?.name} from this project?`}
        onConfirm={onRemoveUser}
        onCancel={() => setRemoveTarget(null)}
      />
    </>
  );
}
