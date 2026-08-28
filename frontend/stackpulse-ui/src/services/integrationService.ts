import api from './api';

export async function getLatestJira() {
  const response = await api.get<{ data: any[] }>('/integrations/jira/latest');
  return response.data.data ?? [];
}

export async function getLatestBitbucket() {
  const response = await api.get<{ data: any[] }>('/integrations/bitbucket/latest');
  return response.data.data ?? [];
}

export async function getLatestGitHub() {
  const response = await api.get<{ data: any[] }>('/integrations/github/latest');
  return response.data.data ?? [];
}