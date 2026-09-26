import api from "./api";

export interface AdminUser {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
  companyId: number | null;
  companyName: string | null;
  isActive: boolean;
}

export interface AdminCompany {
  companyId: number;
  name: string;
}

export async function listUsers(): Promise<AdminUser[]> {
  const { data } = await api.get<AdminUser[]>("/v1/Users");
  return data;
}

export async function grantAdmin(id: string): Promise<void> {
  await api.post(`/v1/Users/${id}/admin`);
}

export async function revokeAdmin(id: string): Promise<void> {
  await api.delete(`/v1/Users/${id}/admin`);
}

export async function setUserCompany(id: string, companyId: number): Promise<void> {
  await api.put(`/v1/Users/${id}/company`, { companyId });
}

export async function setUserActive(id: string, isActive: boolean): Promise<void> {
  if (isActive) {
    await api.post(`/v1/Users/${id}/activate`);
  } else {
    await api.delete(`/v1/Users/${id}/activate`);
  }
}

export async function resetPassword(id: string): Promise<void> {
  await api.post(`/v1/Users/${id}/reset-password`);
}

export async function listCompanies(): Promise<AdminCompany[]> {
  const { data } = await api.get<AdminCompany[]>("/v1/Companies");
  return data;
}
