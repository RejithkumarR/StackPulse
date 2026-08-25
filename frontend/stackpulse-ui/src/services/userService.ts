import api from './api';

export interface UserRow {
  id: string;
  username: string;
  email: string;
  firstName?: string;
  lastName?: string;
  isActive: boolean;
  role: string;
}

export const userService = {
  async getUsers() {
    const response = await api.get('/users');
    return response.data.data as UserRow[];
  },
};
