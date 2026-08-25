import api from './api';

export interface DashboardSummary {
  totalUsers: number;
  activeUsers: number;
  systemHealth: string;
  uptime: string;
  alerts: number;
  activeSessions: number;
  totalAuditLogs: number;
}

export interface ActivityItem {
  id: string;
  action: string;
  details: string;
  createdAt: string;
  userName: string;
}

export const dashboardService = {
  async getDashboard() {
    const response = await api.get('/dashboard');
    return response.data.data as { summary: DashboardSummary; activity: ActivityItem[] };
  },
};
