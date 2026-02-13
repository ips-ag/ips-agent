import { Box, Typography, Button } from '@mui/material';
import AddIcon from '@mui/icons-material/Add';

interface PageHeaderProps {
  title: string;
  onAdd?: () => void;
  addLabel?: string;
}

export default function PageHeader({ title, onAdd, addLabel }: PageHeaderProps) {
  return (
    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
      <Typography variant="h4" fontWeight={700}>{title}</Typography>
      {onAdd && (
        <Button variant="contained" startIcon={<AddIcon />} onClick={onAdd}>
          {addLabel ?? 'Add New'}
        </Button>
      )}
    </Box>
  );
}
