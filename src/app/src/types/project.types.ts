export interface ProjectDto {
  id: string;
  customerId: string;
  customerName?: string;
  parentId?: string;
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
  startDate: string;
  endDate?: string;
  createdAt: string;
  updatedAt: string;
  children?: ProjectDto[];
}

export interface CreateProjectRequest {
  customerId: string;
  parentId?: string;
  name: string;
  code: string;
  description?: string;
  startDate: string;
  endDate?: string;
}

export interface UpdateProjectRequest {
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
  startDate: string;
  endDate?: string;
}

export interface ProjectTreeDto {
  id: string;
  name: string;
  code: string;
  children: ProjectTreeDto[];
}
