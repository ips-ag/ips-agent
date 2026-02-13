import {
  Card, CardContent, Typography, Paper, Table, TableHead,
  TableRow, TableCell, TableBody,
} from '@mui/material';
import { useParams } from 'react-router-dom';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
} from 'recharts';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import EmptyState from '../components/common/EmptyState';
import { useUserReport } from '../hooks/useReports';

export default function UserReportPage() {
  const { id } = useParams<{ id: string }>();
  const { data: report, isLoading } = useUserReport(id ?? '');

  if (isLoading) return <LoadingOverlay />;
  if (!report) return <EmptyState message="No report data available." />;

  return (
    <>
      <PageHeader title={`User Report — ${report.userName}`} />

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

      <Paper sx={{ mb: 3, p: 2 }}>
        <Typography variant="h6" sx={{ mb: 2 }}>Hours by Project</Typography>
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={report.projectBreakdown}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="projectName" />
            <YAxis />
            <Tooltip />
            <Bar dataKey="hours" fill="#009dc3" />
          </BarChart>
        </ResponsiveContainer>
      </Paper>

      <Paper>
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
    </>
  );
}
