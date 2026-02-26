import apiClient from './client';
import type { TimesheetDto } from '../types/timeEntry.types';

export const timesheetsApi = {
  getMy: (week?: string) =>
    apiClient.get<TimesheetDto>('/timesheets/my', { params: { week } }).then(r => r.data),
};
