import { Box, Card, CardContent, Typography, Avatar } from '@mui/material';
import PageHeader from '../components/common/PageHeader';

export default function ProfilePage() {
  // Placeholder user info — replace with real auth context
  const user = {
    name: 'Jane Doe',
    email: 'jane.doe@fakeintra.dev',
    role: 'Employee',
    initials: 'JD',
  };

  return (
    <>
      <PageHeader title="Profile" />
      <Card sx={{ maxWidth: 480 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, mb: 3 }}>
            <Avatar sx={{ width: 72, height: 72, bgcolor: 'primary.main', fontSize: 28 }}>
              {user.initials}
            </Avatar>
            <Box>
              <Typography variant="h5">{user.name}</Typography>
              <Typography variant="body2" color="text.secondary">
                {user.email}
              </Typography>
            </Box>
          </Box>
          <Typography variant="subtitle2" color="text.secondary">Role</Typography>
          <Typography variant="body1" sx={{ mb: 2 }}>{user.role}</Typography>
          <Typography variant="body2" color="text.secondary">
            Profile management will be available once authentication is configured.
          </Typography>
        </CardContent>
      </Card>
    </>
  );
}
