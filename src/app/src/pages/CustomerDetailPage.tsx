import {
  Paper, Table, TableHead, TableRow, TableCell, TableBody,
  Card, CardContent, Typography, Chip, Box, Button,
} from '@mui/material';
import { useParams, useNavigate } from 'react-router-dom';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import EmptyState from '../components/common/EmptyState';
import { useCustomer } from '../hooks/useCustomers';
import { useProjects } from '../hooks/useProjects';

export default function CustomerDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: customer, isLoading: customerLoading } = useCustomer(id!);
  const { data: projects, isLoading: projectsLoading } = useProjects(1, 50, undefined, id);

  if (customerLoading) return <LoadingOverlay />;
  if (!customer) return <EmptyState message="Customer not found" />;

  return (
    <>
      <PageHeader title={customer.name} />

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Name</Typography>
              <Typography>{customer.name}</Typography>
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Unit</Typography>
              <Typography>{customer.unitName ?? '—'}</Typography>
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Contact Email</Typography>
              <Typography>{customer.contactEmail || '—'}</Typography>
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Contact Phone</Typography>
              <Typography>{customer.contactPhone || '—'}</Typography>
            </Box>
            <Box sx={{ gridColumn: '1 / -1' }}>
              <Typography variant="subtitle2" color="text.secondary">Description</Typography>
              <Typography>{customer.description || '—'}</Typography>
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Status</Typography>
              <Chip
                label={customer.isActive ? 'Active' : 'Archived'}
                color={customer.isActive ? 'success' : 'default'}
                size="small"
              />
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Created</Typography>
              <Typography>{new Date(customer.createdAt).toLocaleDateString()}</Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>

      <Typography variant="h6" sx={{ mb: 2 }}>Projects</Typography>

      {projectsLoading ? (
        <LoadingOverlay />
      ) : !projects || projects.items.length === 0 ? (
        <EmptyState message="No projects for this customer" />
      ) : (
        <Paper>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Code</TableCell>
                <TableCell>Start Date</TableCell>
                <TableCell>Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {projects.items.map((p) => (
                <TableRow
                  key={p.id}
                  hover
                  sx={{ cursor: 'pointer' }}
                  onClick={() => navigate(`/projects/${p.id}`)}
                >
                  <TableCell>{p.name}</TableCell>
                  <TableCell>{p.code}</TableCell>
                  <TableCell>{p.startDate ? new Date(p.startDate).toLocaleDateString() : ''}</TableCell>
                  <TableCell>
                    <Chip
                      label={p.isActive ? 'Active' : 'Archived'}
                      color={p.isActive ? 'success' : 'default'}
                      size="small"
                    />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Paper>
      )}

      <Box sx={{ mt: 2 }}>
        <Button variant="outlined" onClick={() => navigate('/customers')}>Back to Customers</Button>
      </Box>
    </>
  );
}
