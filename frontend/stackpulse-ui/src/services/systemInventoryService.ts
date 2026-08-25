import api from './api';

export async function getLatestInventory() {
  const res = await api.get('/systeminventory/latest');
  return res.data.data;
}

export async function getAllInventories() {
  const res = await api.get('/systeminventory');
  return res.data.data;
}
