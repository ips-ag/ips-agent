import { useQuery } from '@tanstack/react-query';
import { timesheetsApi } from '../api/timesheets.api';

export function useMyTimesheet(week?: string) {
  return useQuery({
    queryKey: ['timesheets', 'my', week],
    queryFn: () => timesheetsApi.getMy(week),
  });
}
