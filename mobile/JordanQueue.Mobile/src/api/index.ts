import { apiClient } from './client';
import type {
  ApiResponse,
  AuthResponse,
  Business,
  BusinessDetail,
  NotificationItem,
  PagedResult,
  QueuePosition,
  Ticket,
} from '../types/api';

export async function login(emailOrMobile: string, password: string) {
  const { data } = await apiClient.post<ApiResponse<AuthResponse>>('/api/auth/login', {
    emailOrMobile,
    password,
  });
  return data;
}

export async function register(payload: {
  firstName: string;
  lastName: string;
  mobileNumber: string;
  email: string;
  password: string;
}) {
  const { data } = await apiClient.post<ApiResponse<AuthResponse>>('/api/auth/register', {
    ...payload,
    role: 'Customer',
  });
  return data;
}

export async function searchBusinesses(search?: string, page = 1) {
  const { data } = await apiClient.get<ApiResponse<PagedResult<Business>>>('/api/businesses', {
    params: { search, page, pageSize: 20 },
  });
  return data;
}

export async function getBusiness(id: string) {
  const { data } = await apiClient.get<ApiResponse<BusinessDetail>>(`/api/businesses/${id}`);
  return data;
}

export async function joinQueue(businessId: string, serviceId: string) {
  const { data } = await apiClient.post<ApiResponse<Ticket>>(
    `/api/businesses/${businessId}/queue/join`,
    { serviceId },
  );
  return data;
}

export async function getMyTickets() {
  const { data } = await apiClient.get<ApiResponse<Ticket[]>>('/api/tickets/mine');
  return data;
}

export async function getTicket(id: string) {
  const { data } = await apiClient.get<ApiResponse<Ticket>>(`/api/tickets/${id}`);
  return data;
}

export async function getQueuePosition(queueId: string) {
  const { data } = await apiClient.get<ApiResponse<QueuePosition>>(`/api/queues/${queueId}/position`);
  return data;
}

export async function cancelTicket(ticketId: string) {
  const { data } = await apiClient.post<ApiResponse<Ticket>>(`/api/tickets/${ticketId}/cancel`);
  return data;
}

export async function getNotifications() {
  const { data } = await apiClient.get<ApiResponse<NotificationItem[]>>('/api/notifications');
  return data;
}

export async function markNotificationRead(id: string) {
  const { data } = await apiClient.post<ApiResponse<object>>(`/api/notifications/${id}/read`);
  return data;
}
