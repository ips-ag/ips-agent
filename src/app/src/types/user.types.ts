export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: 'Admin' | 'Manager' | 'Employee';
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  role: 'Admin' | 'Manager' | 'Employee';
  isActive: boolean;
}
