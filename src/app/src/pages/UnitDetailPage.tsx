import {
  Paper, Table, TableHead, TableRow, TableCell, TableBody,
  Card, CardContent, Typography, Chip, Box, Button,
} from '@mui/material';
import { useParams, useNavigate } from 'react-router-dom';
import PageHeader from '../components/common/PageHeader';
import LoadingOverlay from '../components/common/LoadingOverlay';
import EmptyState from '../components/common/EmptyState';
import { useUnit } from '../hooks/useUnits';
import { useCustomers } from '../hooks/useCustomers';

export default function UnitDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: unit, isLoading: unitLoading } = useUnit(id!);
  const { data: customers, isLoading: customersLoading } = useCustomers(1, 20, undefined, id);

  if (unitLoading) return <LoadingOverlay />;
  if (!unit) return <EmptyState message="Unit not found" />;

  return (
    <>
      <PageHeader title={unit.name} />

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 2 }}>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Name</Typography>
              <Typography>{unit.name}</Typography>
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Status</Typography>
              <Chip
                label={unit.isActive ? 'Active' : 'Archived'}
                color={unit.isActive ? 'success' : 'default'}
                size="small"
              />
            </Box>
            <Box sx={{ gridColumn: '1 / -1' }}>
              <Typography variant="subtitle2" color="text.secondary">Description</Typography>
              <Typography>{unit.description || '—'}</Typography>
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Created</Typography>
              <Typography>{new Date(unit.createdAt).toLocaleDateString()}</Typography>
            </Box>
            <Box>
              <Typography variant="subtitle2" color="text.secondary">Updated</Typography>
              <Typography>{new Date(unit.updatedAt).toLocaleDateString()}</Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>

      <Typography variant="h6" sx={{ mb: 2 }}>Customers</Typography>

      {customersLoading ? (
        <LoadingOverlay />
      ) : !customers || customers.items.length === 0 ? (
        <EmptyState message="No customers for this unit" />
      ) : (
        <Paper>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell>Name</TableCell>
                <TableCell>Contact Email</TableCell>
                <TableCell>Status</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {customers.items.map((c) => (
                <TableRow
                  key={c.id}
                  hover
                  sx={{ cursor: 'pointer' }}
                  onClick={() => navigate(`/customers/${c.id}`)}
                >
                  <TableCell>{c.name}</TableCell>
                  <TableCell>{c.contactEmail ?? '—'}</TableCell>
                  <TableCell>
                    <Chip
                      label={c.isActive ? 'Active' : 'Archived'}
                      color={c.isActive ? 'success' : 'default'}
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
        <Button variant="outlined" onClick={() => navigate('/units')}>Back to Units</Button>
      </Box>
    </>
  );
}
