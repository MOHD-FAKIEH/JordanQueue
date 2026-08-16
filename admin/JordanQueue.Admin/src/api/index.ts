import { apiClient } from './client'
import type { ApiResponse, AuthResponse, Business, BusinessDetail, DailyStats, Queue, Service, StaffMember, Ticket } from '../types/api'

export async function login(emailOrMobile: string, password: string) {
  const { data } = await apiClient.post<ApiResponse<AuthResponse>>('/api/auth/login', {
    emailOrMobile,
    password,
  })
  return data
}

export async function getMyBusinesses() {
  const { data } = await apiClient.get<ApiResponse<Business[]>>('/api/businesses/mine')
  return data
}

export async function getBusiness(id: string) {
  const { data } = await apiClient.get<ApiResponse<BusinessDetail>>(`/api/businesses/${id}`)
  return data
}

export async function updateBusiness(id: string, payload: Partial<Business> & { isActive: boolean }) {
  const { data } = await apiClient.put<ApiResponse<Business>>(`/api/businesses/${id}`, payload)
  return data
}

export async function getDailyStats(businessId: string) {
  const { data } = await apiClient.get<ApiResponse<DailyStats>>(`/api/businesses/${businessId}/stats/daily`)
  return data
}

export async function getBusinessQueue(businessId: string, serviceId?: string) {
  const { data } = await apiClient.get<ApiResponse<Queue>>(`/api/businesses/${businessId}/queue`, {
    params: serviceId ? { serviceId } : undefined,
  })
  return data
}

export async function openQueue(businessId: string, serviceId: string) {
  const { data } = await apiClient.post<ApiResponse<Queue>>(`/api/businesses/${businessId}/queue/open`, {
    serviceId,
  })
  return data
}

export async function getServices(businessId: string) {
  const { data } = await apiClient.get<ApiResponse<Service[]>>(`/api/businesses/${businessId}/services`)
  return data
}

export async function createService(businessId: string, payload: Omit<Service, 'id' | 'businessId' | 'isActive'>) {
  const { data } = await apiClient.post<ApiResponse<Service>>(`/api/businesses/${businessId}/services`, payload)
  return data
}

export async function updateService(id: string, payload: Omit<Service, 'id' | 'businessId'>) {
  const { data } = await apiClient.put<ApiResponse<Service>>(`/api/services/${id}`, payload)
  return data
}

export async function getQueueTickets(queueId: string) {
  const { data } = await apiClient.get<ApiResponse<Ticket[]>>(`/api/queues/${queueId}/tickets`)
  return data
}

export async function callNext(queueId: string) {
  const { data } = await apiClient.post<ApiResponse<Ticket>>(`/api/queues/${queueId}/next`)
  return data
}

export async function pauseQueue(queueId: string) {
  const { data } = await apiClient.post<ApiResponse<Queue>>(`/api/queues/${queueId}/pause`)
  return data
}

export async function resumeQueue(queueId: string) {
  const { data } = await apiClient.post<ApiResponse<Queue>>(`/api/queues/${queueId}/resume`)
  return data
}

export async function serveTicket(ticketId: string) {
  const { data } = await apiClient.post<ApiResponse<Ticket>>(`/api/tickets/${ticketId}/serve`)
  return data
}

export async function skipTicket(ticketId: string) {
  const { data } = await apiClient.post<ApiResponse<Ticket>>(`/api/tickets/${ticketId}/skip`)
  return data
}

export async function cancelTicket(ticketId: string) {
  const { data } = await apiClient.post<ApiResponse<Ticket>>(`/api/tickets/${ticketId}/cancel`)
  return data
}

export async function getStaff(businessId: string) {
  const { data } = await apiClient.get<ApiResponse<StaffMember[]>>(`/api/businesses/${businessId}/staff`)
  return data
}

export async function addStaff(businessId: string, email: string) {
  const { data } = await apiClient.post<ApiResponse<StaffMember>>(`/api/businesses/${businessId}/staff`, { email })
  return data
}

export async function removeStaff(businessId: string, staffId: string) {
  const { data } = await apiClient.delete<ApiResponse<object>>(`/api/businesses/${businessId}/staff/${staffId}`)
  return data
}
