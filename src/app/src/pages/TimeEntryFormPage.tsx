import { useEffect } from 'react';
import {
  Paper, TextField, Button, Box, FormControl, InputLabel,
  Select, MenuItem, Typography,
} from '@mui/material';
import { useParams, useNavigate } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import { useTimeEntry, useCreateTimeEntry, useUpdateTimeEntry } from '../hooks/useTimeEntries';
import { useProjects } from '../hooks/useProjects';
import { useTasks } from '../hooks/useTasks';

const timeEntrySchema = z.object({
  projectId: z.string().min(1, 'Project is required'),
  taskId: z.string().min(1, 'Task is required'),
  date: z.string().min(1, 'Date is required'),
  hours: z.coerce
    .number()
    .min(0.25, 'Minimum 0.25 hours')
    .max(24, 'Maximum 24 hours'),
  description: z.string().optional(),
});
type TimeEntryFormData = z.infer<typeof timeEntrySchema>;

export default function TimeEntryFormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = Boolean(id);

  const { data: entry, isLoading: entryLoading } = useTimeEntry(id ?? '');
  const createEntry = useCreateTimeEntry();
  const updateEntry = useUpdateTimeEntry();
  const { data: projects } = useProjects(1, 100);

  const { register, handleSubmit, control, watch, reset, setValue, formState: { errors } } = useForm<TimeEntryFormData>({
    resolver: zodResolver(timeEntrySchema),
    defaultValues: { projectId: '', taskId: '', date: '', hours: 0, description: '' },
  });

  const watchedProjectId = watch('projectId');
  const { data: tasks } = useTasks(1, 100, undefined, watchedProjectId || undefined);

  useEffect(() => {
    if (isEdit && entry) {
      // Find the projectId from the entry's task
      const matchingTask = tasks?.items.find((t) => t.id === entry.taskId);
      reset({
        projectId: matchingTask?.projectId ?? '',
        taskId: entry.taskId,
        date: entry.date.split('T')[0],
        hours: entry.hours,
        description: entry.description,
      });
    }
  }, [isEdit, entry, tasks, reset]);

  const onSubmit = async (data: TimeEntryFormData) => {
    if (isEdit && id) {
      await updateEntry.mutateAsync({
        id,
        data: {
          taskId: data.taskId,
          date: data.date,
          hours: data.hours,
          description: data.description || undefined,
        },
      });
    } else {
      await createEntry.mutateAsync({
        taskId: data.taskId,
        date: data.date,
        hours: data.hours,
        description: data.description || undefined,
      });
    }
    navigate('/time-entries');
  };

  if (isEdit && entryLoading) return <LoadingOverlay />;

  return (
    <>
      <PageHeader title={isEdit ? 'Edit Time Entry' : 'New Time Entry'} />

      <Paper sx={{ p: 3, maxWidth: 600 }}>
        <form onSubmit={handleSubmit(onSubmit)}>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
            <Controller
              name="projectId"
              control={control}
              render={({ field }) => (
                <FormControl fullWidth error={!!errors.projectId}>
                  <InputLabel>Project</InputLabel>
                  <Select
                    {...field}
                    label="Project"
                    onChange={(e) => {
                      field.onChange(e);
                      setValue('taskId', '');
                    }}
                  >
                    {projects?.items.map((p) => (
                      <MenuItem key={p.id} value={p.id}>{p.name} ({p.code})</MenuItem>
                    ))}
                  </Select>
                  {errors.projectId && (
                    <Typography variant="caption" color="error">{errors.projectId.message}</Typography>
                  )}
                </FormControl>
              )}
            />

            <Controller
              name="taskId"
              control={control}
              render={({ field }) => (
                <FormControl fullWidth error={!!errors.taskId} disabled={!watchedProjectId}>
                  <InputLabel>Task</InputLabel>
                  <Select {...field} label="Task">
                    {tasks?.items.map((t) => (
                      <MenuItem key={t.id} value={t.id}>{t.name} ({t.code})</MenuItem>
                    ))}
                  </Select>
                  {errors.taskId && (
                    <Typography variant="caption" color="error">{errors.taskId.message}</Typography>
                  )}
                </FormControl>
              )}
            />

            <TextField
              label="Date"
              type="date"
              {...register('date')}
              error={!!errors.date}
              helperText={errors.date?.message}
              InputLabelProps={{ shrink: true }}
              fullWidth
            />

            <TextField
              label="Hours"
              type="number"
              inputProps={{ step: 0.25, min: 0.25, max: 24 }}
              {...register('hours')}
              error={!!errors.hours}
              helperText={errors.hours?.message}
              fullWidth
            />

            <TextField
              label="Description"
              {...register('description')}
              multiline
              rows={3}
              fullWidth
            />

            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end' }}>
              <Button variant="outlined" onClick={() => navigate('/time-entries')}>
                Cancel
              </Button>
              <Button
                type="submit"
                variant="contained"
                disabled={createEntry.isPending || updateEntry.isPending}
              >
                {isEdit ? 'Save' : 'Create'}
              </Button>
            </Box>
          </Box>
        </form>
      </Paper>
    </>
  );
}
