import { useMemo } from 'react';
import {
  Paper, Table, TableHead, TableRow, TableCell, TableBody,
  Typography, Box, Button, IconButton, Tooltip,
} from '@mui/material';
import ChevronLeftIcon from '@mui/icons-material/ChevronLeft';
import ChevronRightIcon from '@mui/icons-material/ChevronRight';
import { useSearchParams } from 'react-router-dom';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import EmptyState from '../components/common/EmptyState';
import { useMyTimesheet } from '../hooks/useTimesheets';

function getMonday(d: Date): Date {
  const day = d.getDay();
  const diff = d.getDate() - day + (day === 0 ? -6 : 1);
  const monday = new Date(d);
  monday.setDate(diff);
  monday.setHours(0, 0, 0, 0);
  return monday;
}

function formatDate(d: Date): string {
  return d.toISOString().split('T')[0]!;
}

function addDays(d: Date, days: number): Date {
  const r = new Date(d);
  r.setDate(r.getDate() + days);
  return r;
}

const DAY_LABELS = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

export default function TimesheetPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const weekParam = searchParams.get('week');
  const weekStart = weekParam ? new Date(weekParam) : getMonday(new Date());
  const weekKey = formatDate(weekStart);

  const { data: timesheet, isLoading } = useMyTimesheet(weekKey);

  const weekDates = useMemo(() => {
    return Array.from({ length: 7 }, (_, i) => addDays(weekStart, i));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [weekKey]);

  const navigateWeek = (offset: number) => {
    const newWeek = addDays(weekStart, offset * 7);
    const params = new URLSearchParams(searchParams);
    params.set('week', formatDate(newWeek));
    setSearchParams(params);
  };

  const goToday = () => {
    const params = new URLSearchParams(searchParams);
    params.set('week', formatDate(getMonday(new Date())));
    setSearchParams(params);
  };

  // Group entries by task
  const taskRows = useMemo(() => {
    if (!timesheet?.entries) return [];
    const map = new Map<string, { taskId: string; taskName: string; projectName: string; days: number[] }>();
    for (const entry of timesheet.entries) {
      const key = entry.taskId;
      if (!map.has(key)) {
        map.set(key, {
          taskId: entry.taskId,
          taskName: entry.taskName ?? 'Unknown Task',
          projectName: entry.projectName ?? '',
          days: [0, 0, 0, 0, 0, 0, 0],
        });
      }
      const row = map.get(key)!;
      const entryDate = new Date(entry.date);
      const dayIndex = Math.round((entryDate.getTime() - weekStart.getTime()) / (86400000));
      if (dayIndex >= 0 && dayIndex < 7) {
        row.days[dayIndex] = (row.days[dayIndex] ?? 0) + entry.hours;
      }
    }
    return Array.from(map.values());
  }, [timesheet, weekKey]);

  const dayTotals = useMemo(() => {
    const totals = [0, 0, 0, 0, 0, 0, 0];
    for (const row of taskRows) {
      for (let i = 0; i < 7; i++) totals[i] = (totals[i] ?? 0) + (row.days[i] ?? 0);
    }
    return totals;
  }, [taskRows]);

  const grandTotal = dayTotals.reduce((a, b) => a + b, 0);

  return (
    <>
      <PageHeader title="Timesheet" />

      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
        <Tooltip title="Previous week">
          <IconButton onClick={() => navigateWeek(-1)}><ChevronLeftIcon /></IconButton>
        </Tooltip>
        <Typography variant="h6">
          {weekDates[0]!.toLocaleDateString()} — {weekDates[6]!.toLocaleDateString()}
        </Typography>
        <Tooltip title="Next week">
          <IconButton onClick={() => navigateWeek(1)}><ChevronRightIcon /></IconButton>
        </Tooltip>
        <Button size="small" variant="outlined" onClick={goToday}>Today</Button>
      </Box>

      <LoadingOverlay open={isLoading} />

      {!isLoading && taskRows.length === 0 ? (
        <EmptyState message="No time entries this week" />
      ) : (
        <Paper sx={{ overflowX: 'auto' }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell sx={{ fontWeight: 700, minWidth: 200 }}>Task</TableCell>
                {weekDates.map((d, i) => (
                  <TableCell key={i} align="center" sx={{ fontWeight: 700, minWidth: 80 }}>
                    {DAY_LABELS[i]}
                    <br />
                    <Typography variant="caption">{d.getDate()}/{d.getMonth() + 1}</Typography>
                  </TableCell>
                ))}
                <TableCell align="center" sx={{ fontWeight: 700 }}>Total</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {taskRows.map((row) => {
                const rowTotal = row.days.reduce((a, b) => a + b, 0);
                return (
                  <TableRow key={row.taskId}>
                    <TableCell>
                      <Typography variant="body2" fontWeight={600}>{row.taskName}</Typography>
                      <Typography variant="caption" color="text.secondary">{row.projectName}</Typography>
                    </TableCell>
                    {row.days.map((h, i) => (
                      <TableCell key={i} align="center">
                        {h > 0 ? h.toFixed(2) : '—'}
                      </TableCell>
                    ))}
                    <TableCell align="center" sx={{ fontWeight: 600 }}>{rowTotal.toFixed(2)}</TableCell>
                  </TableRow>
                );
              })}
              <TableRow sx={{ backgroundColor: 'action.hover' }}>
                <TableCell sx={{ fontWeight: 700 }}>Total</TableCell>
                {dayTotals.map((t, i) => (
                  <TableCell key={i} align="center" sx={{ fontWeight: 700 }}>
                    {t > 0 ? t.toFixed(2) : '—'}
                  </TableCell>
                ))}
                <TableCell align="center" sx={{ fontWeight: 700 }}>{grandTotal.toFixed(2)}</TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </Paper>
      )}
    </>
  );
}
