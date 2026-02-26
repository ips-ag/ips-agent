export interface TaskDto {
  id: string;
  projectId: string;
  projectName?: string;
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
  startDate?: string;
  endDate?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateTaskRequest {
  projectId: string;
  name: string;
  code: string;
  description?: string;
  startDate?: string;
  endDate?: string;
}

export interface UpdateTaskRequest {
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
  startDate?: string;
  endDate?: string;
}
