export interface TimeEntryDto {
  id: string;
  userId: string;
  userName?: string;
  taskId: string;
  taskName?: string;
  projectName?: string;
  date: string;
  hours: number;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTimeEntryRequest {
  taskId: string;
  date: string;
  hours: number;
  description?: string;
}

export interface UpdateTimeEntryRequest {
  taskId: string;
  date: string;
  hours: number;
  description?: string;
}

export interface TimesheetDto {
  weekStart: string;
  entries: TimeEntryDto[];
  totalHours: number;
}
