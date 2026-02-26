import {
  Box, Card, CardContent, Typography, Paper, Table, TableHead,
  TableRow, TableCell, TableBody,
} from '@mui/material';
import { useParams } from 'react-router-dom';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, Legend,
} from 'recharts';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import EmptyState from '../components/common/EmptyState';
import { useProjectReport } from '../hooks/useReports';

const COLORS = ['#17428c', '#009dc3', '#e91e63', '#ff9800', '#4caf50', '#9c27b0', '#00bcd4', '#795548'];

export default function ProjectReportPage() {
  const { id } = useParams<{ id: string }>();
  const { data: report, isLoading } = useProjectReport(id ?? '');

  if (isLoading) return <LoadingOverlay />;
  if (!report) return <EmptyState message="No report data available." />;

  return (
    <>
      <PageHeader title={`Project Report — ${report.projectName}`} />

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h4" color="primary">
            {report.totalHours} hours
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Total tracked time
          </Typography>
        </CardContent>
      </Card>

      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', mb: 3 }}>
        {/* Hours by Task — Bar Chart */}
        <Paper sx={{ flex: 1, minWidth: 350, p: 2 }}>
          <Typography variant="h6" sx={{ mb: 2 }}>Hours by Task</Typography>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={report.taskBreakdown}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="taskName" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="hours" fill="#17428c" />
            </BarChart>
          </ResponsiveContainer>
        </Paper>

        {/* Hours by User — Pie Chart */}
        <Paper sx={{ flex: 1, minWidth: 350, p: 2 }}>
          <Typography variant="h6" sx={{ mb: 2 }}>Hours by User</Typography>
          <ResponsiveContainer width="100%" height={300}>
            <PieChart>
              <Pie
                data={report.userBreakdown}
                dataKey="hours"
                nameKey="userName"
                cx="50%"
                cy="50%"
                outerRadius={100}
                label
              >
                {report.userBreakdown.map((_, idx) => (
                  <Cell key={idx} fill={COLORS[idx % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip />
              <Legend />
            </PieChart>
          </ResponsiveContainer>
        </Paper>
      </Box>

      {/* Data Tables */}
      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
        <Paper sx={{ flex: 1, minWidth: 300 }}>
          <Typography variant="h6" sx={{ p: 2, pb: 0 }}>Task Breakdown</Typography>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Task</TableCell>
                <TableCell align="right">Hours</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {report.taskBreakdown.map((t) => (
                <TableRow key={t.taskName}>
                  <TableCell>{t.taskName}</TableCell>
                  <TableCell align="right">{t.hours}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>

        <Paper sx={{ flex: 1, minWidth: 300 }}>
          <Typography variant="h6" sx={{ p: 2, pb: 0 }}>User Breakdown</Typography>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>User</TableCell>
                <TableCell align="right">Hours</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {report.userBreakdown.map((u) => (
                <TableRow key={u.userName}>
                  <TableCell>{u.userName}</TableCell>
                  <TableCell align="right">{u.hours}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>
      </Box>
    </>
  );
}
