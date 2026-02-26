import apiClient from './client';
import type { ProjectReportDto, UserReportDto, OverallReportDto } from '../types/report.types';

export const reportsApi = {
  getProjectReport: (id: string) =>
    apiClient.get<ProjectReportDto>(`/reports/project/${id}`).then(r => r.data),
  getUserReport: (id: string) =>
    apiClient.get<UserReportDto>(`/reports/user/${id}`).then(r => r.data),
  getOverallReport: () =>
    apiClient.get<OverallReportDto>('/reports/overall').then(r => r.data),
};
