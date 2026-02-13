import { Card, CardActionArea, CardContent, Grid2, Typography } from '@mui/material';
import AssessmentIcon from '@mui/icons-material/Assessment';
import PersonIcon from '@mui/icons-material/Person';
import BarChartIcon from '@mui/icons-material/BarChart';
import { useNavigate } from 'react-router-dom';
import PageHeader from '../components/common/PageHeader';

const reportCards = [
  {
    title: 'Project Reports',
    description: 'View time breakdowns per project, grouped by task and user.',
    icon: <AssessmentIcon sx={{ fontSize: 48, color: '#17428c' }} />,
    path: '/projects',
  },
  {
    title: 'User Reports',
    description: 'View time breakdowns per user, grouped by project.',
    icon: <PersonIcon sx={{ fontSize: 48, color: '#009dc3' }} />,
    path: '/users',
  },
  {
    title: 'Overall Report',
    description: 'See aggregated hours across all projects and users.',
    icon: <BarChartIcon sx={{ fontSize: 48, color: '#e91e63' }} />,
    path: '/reports/overall',
  },
];

export default function ReportsPage() {
  const navigate = useNavigate();

  return (
    <>
      <PageHeader title="Reports" />
      <Typography variant="subtitle1" sx={{ mb: 3 }}>
        Choose a report type to explore time tracking data.
      </Typography>

      <Grid2 container spacing={3}>
        {reportCards.map((card) => (
          <Grid2 size={{ xs: 12, sm: 6, md: 4 }} key={card.title}>
            <Card sx={{ height: '100%' }}>
              <CardActionArea onClick={() => navigate(card.path)} sx={{ p: 2, height: '100%' }}>
                <CardContent sx={{ textAlign: 'center' }}>
                  {card.icon}
                  <Typography variant="h6" sx={{ mt: 2, mb: 1 }}>
                    {card.title}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {card.description}
                  </Typography>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid2>
        ))}
      </Grid2>
    </>
  );
}
