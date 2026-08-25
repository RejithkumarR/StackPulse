import api from './api';
import type { User } from '../types/auth';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user: User;
}

export const authService = {
  async login(payload: LoginRequest) {
    const response = await api.post('/auth/login', payload);
    const data: LoginResponse = response.data.data;

    localStorage.setItem('stackpulse_access_token', data.accessToken);
    localStorage.setItem('stackpulse_refresh_token', data.refreshToken);
    localStorage.setItem('stackpulse_user', JSON.stringify(data.user));

    return data;
  },

  async getCurrentUser() {
    const response = await api.get('/auth/me');
    return response.data.data as User;
  },

  logout() {
    localStorage.removeItem('stackpulse_access_token');
    localStorage.removeItem('stackpulse_refresh_token');
    localStorage.removeItem('stackpulse_user');
  },
};
