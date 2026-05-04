// ─────────────────────────────────────────────
// Authentication
// ─────────────────────────────────────────────
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
  password: string;
}

export interface AuthApiResponse {
  name: string;
  email: string;
  accessToken: string;
  refreshToken: string;
  role: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ForgetPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword?: string;
  newPassword?: string;
}

// ─────────────────────────────────────────────
// Pagination
// ─────────────────────────────────────────────
export interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Category concept replaced by Vendor Type, Service Type, and Event Type (taxonomy.models.ts)

// ─────────────────────────────────────────────
// Service Type
// ─────────────────────────────────────────────
export interface ServiceType {
  id: string;
  name: string;
  // description?: string;
}

export interface CreateServiceTypeRequest {
  name: string;
  // description?: string;
}

export interface UpdateServiceTypeRequest {
  name?: string;
  // description?: string;
}

// ─────────────────────────────────────────────
// User
// ─────────────────────────────────────────────
export interface ApiUser {
  id: string;
  name: string;
  email: string;
  role?: string;
  phone?: string;
  createdAt?: string;
  status?: 'active' | 'suspended';
}

export interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
  role?: string;
}

export interface UpdateUserRequest {
  name?: string;
  email?: string;
  phone?: string;
}

// ─────────────────────────────────────────────
// Vendor
// ─────────────────────────────────────────────
export interface ApiVendor {
  id: string;
  name: string;
  email?: string;
  phone?: string;
  vendorTypeId?: string;
  vendorTypeName?: string;
  location?: string;
  rating?: number;
  isApproved?: boolean;
  status?: 'active' | 'suspended' | 'pending';
  createdAt?: string;
  about?: string;
}

export interface CreateVendorRequest {
  firstName: string;
  lastName: string;
  email: string;
  password?: string; // Admin can set a default or leave it to auto-gen
  phone: string;
  name: string; // UserName
  businessName: string;
  ownerName: string;
  vendorTypeId: string;
  yearsInBusiness: number;
  description: string;
  portfolioLink?: string;
  address: {
    street: string;
    city: string;
    state: string;
    postalCode?: string;
  };
}

export interface UpdateVendorRequest {
  name?: string;
  phone?: string;
  location?: string;
  about?: string;
}

// ─────────────────────────────────────────────
// Product
// ─────────────────────────────────────────────
export interface ApiProduct {
  id: string;
  name: string;
  description?: string;
  price: number;
  vendorTypeId?: string;
  vendorTypeName?: string;
  vendorId?: string;
  vendorName?: string;
  serviceTypeId?: string;
  serviceTypeName?: string;
  imageUrl?: string;
  status?: 'active' | 'paused';
  duration?: string;
  leadTime?: string;
  classification?: 'Personal' | 'Corporate';
  allowedEventTypes?: string[];
  createdAt?: string;
}

export interface CreateProductRequest {
  name: string;
  description?: string;
  price: number;
  vendorTypeId?: string;
  serviceTypeId?: string;
  imageUrl?: string;
  status?: 'active' | 'paused';
  duration?: string;
  leadTime?: string;
  classification?: 'Personal' | 'Corporate';
  allowedEventTypes?: string[];
}

export interface UpdateProductRequest {
  name?: string;
  description?: string;
  price?: number;
  vendorTypeId?: string;
  serviceTypeId?: string;
  imageUrl?: string;
  status?: 'active' | 'paused';
  duration?: string;
  leadTime?: string;
  classification?: 'Personal' | 'Corporate';
  allowedEventTypes?: string[];
}

// ─────────────────────────────────────────────
// File Upload
// ─────────────────────────────────────────────
export interface FileUploadResponse {
  url: string;
  fileName?: string;
  size?: number;
}

// ─────────────────────────────────────────────
// Chat / Messages
// ─────────────────────────────────────────────
export interface ChatMessage {
  id?: string;
  senderId: string;
  receiverId: string;
  content: string;
  sentAt?: string;
  isRead?: boolean;
}

export interface Conversation {
  userId: string;
  userName?: string;
  userAvatar?: string;
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount?: number;
}

// ─────────────────────────────────────────────
// Gemini AI
// ─────────────────────────────────────────────
export interface GeminiResponse {
  result: string;
  prompt?: string;
}

// ─────────────────────────────────────────────
// Event / Bookings
// ─────────────────────────────────────────────
export interface EventSummaryDto {
  id: string;
  title: string;
  eventDate: string;
  eventStatus: string;
  totalBudget: number;
  itemCount: number;
}

export interface AddressDto {
  street: string;
  city: string;
  state: string;
}

export interface EventItemResponseDto {
  id: string;
  eventId: string;
  serviceId?: string; // ← Add this
  serviceImage?: string;
  serviceName: string;
  price: number;
  vendorId: string;
  vendorName: string;
  quantity: number;
  itemStatus: 'Pending' | 'Approved' | 'Rejected' | 'Done' | 'Completed';
  rejectionReason?: string;
}

export interface EventResponseDto {
  id: string;
  userId: string;
  userName: string;
  title: string;
  eventTypeName: string;
  eventDate: string;
  totalBudget: number;
  guestCount: number;
  notes?: string;
  eventStatus: string;
  cancellationReason?: string;
  additionalNotes?: string;
  cancelledAt?: string;
  location?: AddressDto;
  eventItems: EventItemResponseDto[];
}

export interface CreateEventItemDto {
  eventId: string;
  serviceId?: string;
  serviceImage: string;
  serviceName: string;
  price: number;
  vendorId: string;
  vendorName: string;
  quantity: number;
}

export interface CartItem {
  product: ApiProduct;
  quantity: number;
}

export interface CreateEventDto {
  userId?: string;
  title: string;
  eventTypeId: string;
  eventDate: string;
  location?: AddressDto;
  totalBudget: number;
  guestCount: number;
  notes?: string;
}

export interface UpdateEventDto {
  title: string;
  eventTypeId: string;
  eventDate: string;
  location?: AddressDto;
  totalBudget: number;
  guestCount: number;
  notes?: string;
  eventStatus: string;
}

export interface ApproveItemRequest {
  approve: boolean;
  reason?: string;
}

export interface CancelEventRequest {
  reason?: string;
  additionalNotes?: string;
}

// ─────────────────────────────────────────────
// Notifications
// ─────────────────────────────────────────────
export interface AppNotification {
  id: string;
  userId: string;
  title: string;
  message: string;
  type?: string;
  isRead: boolean;
  createdAt: string;
}

// ─────────────────────────────────────────────
// Payments (Paymob)
// ─────────────────────────────────────────────
export interface PaymobBillingData {
  first_name: string;
  last_name: string;
  email: string;
  phone_number: string;
}

export interface PaymobPaymentRequest {
  amount: number;
  billing: PaymobBillingData;
  orderId?: string;
}

export interface PaymobPaymentResponse {
  iframeUrl: string;
}

// ─────────────────────────────────────────────
// Support Tickets
// ─────────────────────────────────────────────
export interface SupportTicket {
  ticket_id: string;
  title: string;
  from: string;
  type: 'Client' | 'Vendor';
  priority: 'critical' | 'high' | 'medium' | 'low';
  status: 'open' | 'in_progress' | 'resolved';
  opened_at: string;
  description: string;
  booking_ref?: string | null;
  assigned_to?: {
    agent_id: string;
    name: string;
  } | null;
  resolved_at?: string | null;
  replies?: TicketReply[];
}

export interface TicketReply {
  reply_id: string;
  ticket_id: string;
  message: string;
  replied_by: string;
  replied_at: string;
  notified_via: string[];
}

export interface TicketStats {
  critical: number;
  open: number;
  in_progress: number;
  resolution_rate: number;
}

export interface TicketFilters {
  status?: 'open' | 'in_progress' | 'resolved';
  priority?: 'critical' | 'high' | 'medium' | 'low';
  type?: 'Client' | 'Vendor';
  page?: number;
  limit?: number;
}

export interface ReplyTicketRequest {
  message: string;
  send_email?: boolean;
  send_sms?: boolean;
}

export interface AssignTicketRequest {
  agent_id: string;
  note?: string;
}

export interface ResolveTicketRequest {
  resolution_note: string;
}

export interface EscalateTicketRequest {
  reason: string;
  escalate_to: 'senior_management' | 'legal_team' | 'cto';
  notify_finance?: boolean;
}

// ─────────────────────────────────────────────
// Reviews & Ratings
// ─────────────────────────────────────────────
export interface CreateReviewDto {
  userId: string;
  serviceId: string;
  rating: number;
  review: string;
  photoUrl?: string; // Optional: Uploaded event photo URL
}
