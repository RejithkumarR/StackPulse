import api from './api';

export type ComputerMaster = {
  id?: string;
  hostname: string;
  assetTag?: string;
  owner?: string;
  environment?: string;
  isActive: boolean;
};

export type IntegrationAccess = {
  id?: string;
  provider: string;
  displayName: string;
  baseUrl: string;
  projectKey?: string;
  username?: string;
  secretReference?: string;
  isActive: boolean;
};

export type MasterConfiguration = {
  computers: ComputerMaster[];
  integrations: IntegrationAccess[];
};

export async function getMasterConfiguration() {
  const response = await api.get<{ data: MasterConfiguration }>('/master-configuration');
  return response.data.data;
}

export async function saveComputerMaster(payload: ComputerMaster) {
  const response = await api.post<{ data: ComputerMaster }>('/master-configuration/computers', payload);
  return response.data.data;
}

export async function saveIntegrationAccess(payload: IntegrationAccess) {
  const response = await api.post<{ data: IntegrationAccess }>('/master-configuration/integrations', payload);
  return response.data.data;
}
