import {
  Box, Card, CardContent, Typography, Paper, Table, TableHead,
  TableRow, TableCell, TableBody,
} from '@mui/material';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import EmptyState from '../components/common/EmptyState';
import { useOverallReport } from '../hooks/useReports';

export default function OverallReportPage() {
  const { data: report, isLoading } = useOverallReport();

  if (isLoading) return <LoadingOverlay />;
  if (!report) return <EmptyState message="No report data available." />;

  return (
    <>
      <PageHeader title="Overall Report" />

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h4" color="primary">
            {report.totalHours} hours
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Total tracked across all projects and users
          </Typography>
        </CardContent>
      </Card>

      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', mb: 3 }}>
        <Paper sx={{ flex: 1, minWidth: 350, p: 2 }}>
          <Typography variant="h6" sx={{ mb: 2 }}>Hours by Project</Typography>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={report.projectBreakdown}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="projectName" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="hours" fill="#17428c" />
            </BarChart>
          </ResponsiveContainer>
        </Paper>

        <Paper sx={{ flex: 1, minWidth: 350, p: 2 }}>
          <Typography variant="h6" sx={{ mb: 2 }}>Hours by User</Typography>
          <ResponsiveContainer width="100%" height={300}>
            <BarChart data={report.userBreakdown}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="userName" />
              <YAxis />
              <Tooltip />
              <Bar dataKey="hours" fill="#009dc3" />
            </BarChart>
          </ResponsiveContainer>
        </Paper>
      </Box>

      <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
        <Paper sx={{ flex: 1, minWidth: 300 }}>
          <Typography variant="h6" sx={{ p: 2, pb: 0 }}>Project Breakdown</Typography>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Project</TableCell>
                <TableCell align="right">Hours</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {report.projectBreakdown.map((p) => (
                <TableRow key={p.projectName}>
                  <TableCell>{p.projectName}</TableCell>
                  <TableCell align="right">{p.hours}</TableCell>
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
