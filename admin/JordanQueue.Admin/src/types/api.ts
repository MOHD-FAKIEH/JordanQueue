export interface ApiResponse<T> {
  success: boolean
  data: T
  message?: string
  errors?: { code: string; message: string }[]
}

export interface AuthResponse {
  userId: string
  firstName: string
  lastName: string
  email: string
  mobileNumber: string
  roles: string[]
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
  refreshTokenExpiresAt: string
}

export interface Business {
  id: string
  nameArabic: string
  nameEnglish: string
  descriptionArabic: string
  descriptionEnglish: string
  phoneNumber: string
  addressArabic: string
  addressEnglish: string
  category: number
  isActive: boolean
}

export interface BusinessDetail extends Business {
  workingHours: WorkingHours[]
  services: ServiceSummary[]
  currentQueue: QueueSummary | null
}

export interface WorkingHours {
  dayOfWeek: number
  openingTime: string
  closingTime: string
  isClosed: boolean
}

export interface ServiceSummary {
  id: string
  nameArabic: string
  nameEnglish: string
  averageServiceMinutes: number
  isActive: boolean
}

export interface Service extends ServiceSummary {
  businessId: string
  descriptionArabic: string
  descriptionEnglish: string
}

export interface QueueSummary {
  queueId: string
  nowServing: string | null
  waitingCount: number
  estimatedWaitMinutes: number
  status: number
}

export interface Queue {
  id: string
  businessId: string
  serviceId: string
  serviceNameEnglish: string
  queueDate: string
  status: number
  waitingCount: number
  nowServing: string | null
  estimatedWaitMinutes: number
}

export interface Ticket {
  id: string
  queueId: string
  businessId: string
  businessNameEnglish: string
  serviceNameEnglish: string
  ticketNumber: string
  status: number
  position: number
  estimatedWaitMinutes: number
  nowServing: string | null
  joinedAt: string
  calledAt: string | null
  servedAt: string | null
  cancelledAt: string | null
}

export interface DailyStats {
  waitingCount: number
  servingCount: number
  servedToday: number
  averageWaitMinutes: number
  averageServiceMinutes: number
}

export interface StaffMember {
  id: string
  userId: string
  firstName: string
  lastName: string
  email: string
  mobileNumber: string
  isActive: boolean
  createdAt: string
}

export const QueueStatus = {
  Open: 0,
  Paused: 1,
  Closed: 2,
} as const

export const TicketStatus = {
  Waiting: 0,
  Called: 1,
  Serving: 2,
  Served: 3,
  Skipped: 4,
  Cancelled: 5,
} as const

export const BusinessCategory = {
  Clinics: 0,
  Barbers: 1,
  CarServices: 2,
  GovernmentServices: 3,
  RepairServices: 4,
  Beauty: 5,
  Other: 6,
} as const
