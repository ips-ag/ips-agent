export interface ProjectReportDto {
  projectId: string;
  projectName: string;
  totalHours: number;
  taskBreakdown: { taskName: string; hours: number }[];
  userBreakdown: { userName: string; hours: number }[];
}

export interface UserReportDto {
  userId: string;
  userName: string;
  totalHours: number;
  projectBreakdown: { projectName: string; hours: number }[];
}

export interface OverallReportDto {
  totalHours: number;
  projectBreakdown: { projectName: string; hours: number }[];
  userBreakdown: { userName: string; hours: number }[];
}
