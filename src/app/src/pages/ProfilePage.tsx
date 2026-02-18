import { Box, Card, CardContent, Typography, Avatar } from '@mui/material';
import PageHeader from '../components/common/PageHeader';
import { useAuth } from '../hooks/useAuth';

export default function ProfilePage() {
  const { user } = useAuth();

  const initials = `${user?.givenName?.[0] ?? ''}${user?.familyName?.[0] ?? ''}`;

  return (
    <>
      <PageHeader title="Profile" />
      <Card sx={{ maxWidth: 480 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 3, mb: 3 }}>
            <Avatar sx={{ width: 72, height: 72, bgcolor: 'primary.main', fontSize: 28 }}>
              {initials}
            </Avatar>
            <Box>
              <Typography variant="h5">{user?.name}</Typography>
              <Typography variant="body2" color="text.secondary">
                {user?.email}
              </Typography>
            </Box>
          </Box>
          <Typography variant="subtitle2" color="text.secondary">First Name</Typography>
          <Typography variant="body1" sx={{ mb: 1 }}>{user?.givenName}</Typography>
          <Typography variant="subtitle2" color="text.secondary">Last Name</Typography>
          <Typography variant="body1" sx={{ mb: 1 }}>{user?.familyName}</Typography>
        </CardContent>
      </Card>
    </>
  );
}
