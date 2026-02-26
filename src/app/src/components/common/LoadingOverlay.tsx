import { Backdrop, CircularProgress } from '@mui/material';

export default function LoadingOverlay({ open = true }: { open?: boolean }) {
  return (
    <Backdrop open={open} sx={{ zIndex: (t) => t.zIndex.drawer + 1 }}>
      <CircularProgress color="primary" />
    </Backdrop>
  );
}
