import { useState } from 'react';
import {
  Paper, Table, TableHead, TableRow, TableCell, TableBody,
  TextField, IconButton, Pagination, Box, FormControl, InputLabel,
  Select, MenuItem,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { useNavigate, useSearchParams } from 'react-router-dom';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import ConfirmDialog from '../components/common/ConfirmDialog';
import EmptyState from '../components/common/EmptyState';
import { useTimeEntries, useDeleteTimeEntry } from '../hooks/useTimeEntries';
import { useUsers } from '../hooks/useUsers';
import type { TimeEntryDto } from '../types/timeEntry.types';

export default function TimeEntryListPage() {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const page = Number(searchParams.get('page') ?? '1');
  const dateFrom = searchParams.get('dateFrom') ?? '';
  const dateTo = searchParams.get('dateTo') ?? '';
  const userFilter = searchParams.get('userId') ?? '';
  const taskFilter = searchParams.get('taskId') ?? '';

  const { data, isLoading } = useTimeEntries({
    page,
    pageSize: 20,
    dateFrom: dateFrom || undefined,
    dateTo: dateTo || undefined,
    userId: userFilter || undefined,
    taskId: taskFilter || undefined,
  });
  const { data: usersData } = useUsers(1, 200);
  const deleteEntry = useDeleteTimeEntry();

  const [deleteTarget, setDeleteTarget] = useState<TimeEntryDto | null>(null);

  const onDelete = async () => {
    if (deleteTarget) {
      await deleteEntry.mutateAsync(deleteTarget.id);
      setDeleteTarget(null);
    }
  };

  const updateParam = (key: string, value: string) => {
    const params = new URLSearchParams(searchParams);
    if (value) params.set(key, value); else params.delete(key);
    if (key !== 'page') params.set('page', '1');
    setSearchParams(params);
  };

  return (
    <>
      <PageHeader title="Time Entries" onAdd={() => navigate('/time-entries/new')} addLabel="New Entry" />

      <Box sx={{ display: 'flex', gap: 2, mb: 2, flexWrap: 'wrap' }}>
        <TextField
          size="small"
          label="Date From"
          type="date"
          value={dateFrom}
          onChange={(e) => updateParam('dateFrom', e.target.value)}
          InputLabelProps={{ shrink: true }}
        />
        <TextField
          size="small"
          label="Date To"
          type="date"
          value={dateTo}
          onChange={(e) => updateParam('dateTo', e.target.value)}
          InputLabelProps={{ shrink: true }}
        />
        <FormControl size="small" sx={{ minWidth: 200 }}>
          <InputLabel>Filter by User</InputLabel>
          <Select
            value={userFilter}
            label="Filter by User"
            onChange={(e) => updateParam('userId', e.target.value)}
          >
            <MenuItem value="">All Users</MenuItem>
            {usersData?.items.map((u) => (
              <MenuItem key={u.id} value={u.id}>{u.firstName} {u.lastName}</MenuItem>
            ))}
          </Select>
        </FormControl>
      </Box>

      <LoadingOverlay open={isLoading} />

      {!isLoading && (!data || data.items.length === 0) ? (
        <EmptyState message="No time entries found" />
      ) : (
        <Paper>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Date</TableCell>
                <TableCell>Project</TableCell>
                <TableCell>Task</TableCell>
                <TableCell>User</TableCell>
                <TableCell align="right">Hours</TableCell>
                <TableCell>Description</TableCell>
                <TableCell align="right">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.items.map((entry) => (
                <TableRow key={entry.id} hover>
                  <TableCell>{new Date(entry.date).toLocaleDateString()}</TableCell>
                  <TableCell>{entry.projectName ?? '—'}</TableCell>
                  <TableCell>{entry.taskName ?? '—'}</TableCell>
                  <TableCell>{entry.userName ?? '—'}</TableCell>
                  <TableCell align="right">{entry.hours.toFixed(2)}</TableCell>
                  <TableCell>{entry.description}</TableCell>
                  <TableCell align="right">
                    <IconButton size="small" onClick={() => navigate(`/time-entries/${entry.id}/edit`)}>
                      <EditIcon fontSize="small" />
                    </IconButton>
                    <IconButton size="small" color="error" onClick={() => setDeleteTarget(entry)}>
                      <DeleteIcon fontSize="small" />
                    </IconButton>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>
      )}

      {data && data.totalPages > 1 && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 2 }}>
          <Pagination count={data.totalPages} page={page} onChange={(_, p) => updateParam('page', String(p))} />
        </Box>
      )}

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Time Entry"
        message="Are you sure you want to permanently delete this time entry?"
        onConfirm={onDelete}
        onCancel={() => setDeleteTarget(null)}
      />
    </>
  );
}
