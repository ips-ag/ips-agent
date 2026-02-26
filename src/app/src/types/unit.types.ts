export interface UnitDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateUnitRequest {
  name: string;
  description?: string;
}

export interface UpdateUnitRequest {
  name: string;
  description?: string;
  isActive: boolean;
}
