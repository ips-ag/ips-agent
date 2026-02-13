import {
  Paper, Table, TableHead, TableRow, TableCell, TableBody,
  Card, CardContent, Typography, Chip, Box, Button,
} from '@mui/material';
import { useParams, useNavigate } from 'react-router-dom';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import EmptyState from '../components/common/EmptyState';
import { useTask } from '../hooks/useTasks';
import { useTimeEntries } from '../hooks/useTimeEntries';

export default function TaskDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: task, isLoading: taskLoading } = useTask(id!);
  const { data: timeEntries, isLoading: entriesLoading } = useTimeEntries({ taskId: id });

  if (taskLoading) return <LoadingOverlay />;
  if (!task) return <EmptyState message="Task not found" />;

  return (
    <>
      <PageHeader title={task.name} />

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Name</Typography>
              <Typography>{task.name}</Typography>
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Code</Typography>
              <Typography>{task.code}</Typography>
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Project</Typography>
              <Typography>{task.projectName ?? '—'}</Typography>
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Status</Typography>
              <Chip
                label={task.isActive ? 'Active' : 'Archived'}
                color={task.isActive ? 'success' : 'default'}
                size="small"
              />
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Start Date</Typography>
              <Typography>{task.startDate ? new Date(task.startDate).toLocaleDateString() : '—'}</Typography>
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">End Date</Typography>
              <Typography>{task.endDate ? new Date(task.endDate).toLocaleDateString() : '—'}</Typography>
            </Box>
            <Box sx={{ gridColumn: '1 / -1' }}>
              <Typography variant="subtitle2" color="text.secondary">Description</Typography>
              <Typography>{task.description || '—'}</Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h6">Time Entries</Typography>
        <Button variant="contained" onClick={() => navigate('/time-entries/new')}>
          Add Time Entry
        </Button>
      </Box>

      {entriesLoading ? (
        <LoadingOverlay />
      ) : !timeEntries || timeEntries.items.length === 0 ? (
        <EmptyState message="No time entries for this task" />
      ) : (
        <Paper>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>User</TableCell>
                <TableCell>Hours</TableCell>
                <TableCell>Description</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {timeEntries.items.map((te) => (
                <TableRow
                  key={te.id}
                  hover
                  sx={{ cursor: 'pointer' }}
                  onClick={() => navigate(`/time-entries/${te.id}/edit`)}
                >
                  <TableCell>{new Date(te.date).toLocaleDateString()}</TableCell>
                  <TableCell>{te.userName ?? '—'}</TableCell>
                  <TableCell>{te.hours.toFixed(2)}</TableCell>
                  <TableCell>{te.description}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>
      )}

      <Box sx={{ mt: 2 }}>
        <Button variant="outlined" onClick={() => navigate(-1 as any)}>Back</Button>
      </Box>
    </>
  );
}
