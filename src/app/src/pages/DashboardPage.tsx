import { Box, Card, CardContent, Grid, Typography, Button } from '@mui/material';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import ListAltIcon from '@mui/icons-material/ListAlt';
import { useNavigate } from 'react-router-dom';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import { useMyTimesheet } from '../hooks/useTimesheets';

function getMonday(d: Date): Date {
  const day = d.getDay();
  const diff = d.getDate() - day + (day === 0 ? -6 : 1);
  const monday = new Date(d);
  monday.setDate(diff);
  monday.setHours(0, 0, 0, 0);
  return monday;
}

export default function DashboardPage() {
  const navigate = useNavigate();
  const weekStart = getMonday(new Date()).toISOString().split('T')[0]!;
  const { data: timesheet, isLoading } = useMyTimesheet(weekStart);

  const totalHours = timesheet?.totalHours ?? 0;
  const recentCount = timesheet?.entries?.length ?? 0;

  if (isLoading) return <LoadingOverlay />;

  return (
    <>
      <PageHeader title="Dashboard" />
      <Typography variant="subtitle1" sx={{ mb: 3 }}>
        Welcome to Fake Intra — here's your week at a glance.
      </Typography>

      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <AccessTimeIcon color="primary" sx={{ mr: 1 }} />
                <Typography variant="subtitle2" color="text.secondary">
                  Total Hours This Week
                </Typography>
              </Box>
              <Typography variant="h3" color="primary">
                {totalHours}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
                <ListAltIcon color="secondary" sx={{ mr: 1 }} />
                <Typography variant="subtitle2" color="text.secondary">
                  Recent Entries
                </Typography>
              </Box>
              <Typography variant="h3" color="secondary">
                {recentCount}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      <Typography variant="h6" sx={{ mb: 2 }}>
        Quick Actions
      </Typography>
      <Box sx={{ display: 'flex', gap: 2 }}>
        <Button variant="contained" onClick={() => navigate('/time-entries/new')}>
          Log Time
        </Button>
        <Button variant="outlined" onClick={() => navigate('/timesheets')}>
          View Timesheet
        </Button>
      </Box>
    </>
  );
}
