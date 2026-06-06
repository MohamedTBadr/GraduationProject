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

// ─────────────────────────────────────────────
// Pagination & Filtering
// ─────────────────────────────────────────────
export interface PaginatedRequest {
  // 📍 Location filters (flattened)
  city?: string;
  region?: string;
  latitude?: number;
  longitude?: number;
  radiusKm?: number;

  // 📄 Pagination
  pageIndex?: number;
  pageSize?: number;

  // 🔍 Search & Sort
  searchTerm?: string;
  sortBy?: string;
  isDescending?: boolean;
  includeHidden?: boolean;

  // Taxonomy filters
  classification?: string;
  eventTypeId?: string;
  vendorTypeId?: string;
  serviceTypeId?: string;

  // Price filters (services)
  minPrice?: number;
  maxPrice?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number; // Keep for UI if used
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
  vendorTypeId: string;
}

export interface UpdateServiceTypeRequest {
  name?: string;
  vendorTypeId?: string;
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
  documentUrl?: string;
  profilePictureUrl?: string;
  serviceAreas?: ServiceAreaDTO[];
}

/** Service ratings aggregated on GET /Vendor/{id} (VendorDetailsDTO.VendorRating). */
export interface VendorRatingDto {
  id: string;
  vendorName?: string;
  userName: string;
  rating: number;
  review: string;
  createdAt: string;
  serviceName?: string;
}

export interface VendorDetails extends ApiVendor {
  vendorRatings: VendorRatingDto[];
  startingPrice?: number;
  yearsInBusiness?: number;
}

export interface ServiceAreaDTO {
  id?: string; // Guid
  city: string;
  region: string;
  latitude: number;
  longitude: number;
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
  document?: File;
  profilePicture?: File;
  serviceAreas?: ServiceAreaDTO[];
  address: {
    street: string;
    city: string;
    state: string;
    postalCode?: string;
  };
}

export interface UpdateVendorRequest {
  email?: string;
  password?: string;
  phone?: string;
  name?: string;
  businessName?: string;
  ownerName?: string;
  serviceTypes?: any[];
  yearsInBusiness?: number;
  description?: string;
  portfolioLink?: string;
  address?: AddressDto;
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
  imageUrls?: string[]; // Multiple images for slider
  status?: 'active' | 'paused';
  isHidden?: boolean;
  duration?: string;
  leadTime?: string;
  classification?: 'Personal' | 'Corporate';
  eventTypeIds?: string[];
  allowedEventTypes?: string[]; // backwards compatibility
  createdAt?: string;
  serviceAreas?: ServiceAreaDTO[];
  rating?: number;
  reviewCount?: number;
  requiresApproval?: boolean;
  bookedCount?: number;
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
  eventTypeIds?: string[];
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
  eventTypeIds?: string[];
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
// AI Event Planning
// ─────────────────────────────────────────────
export interface AiPlanItem {
  ServiceId?: string;
  serviceId?: string;
  Service_name?: string;
  category: string;
  vendor: string;
  price: number;
  reason: string;
  selected?: boolean;
}

export interface AiEventPlanParsed {
  event_title: string;
  event_type: string;
  guest_count: number;
  total_budget: number;
  plan_summary: string;
  selected_items: AiPlanItem[];
  total_cost: number;
  remaining_budget: number;
  tips: string[];
}

export interface AiEventPlanResponse {
  eventId: string;
  eventTitle: string;
  budget: number;
  eventTypeName: string;
  servicesConsidered: number;
  aiPlan: string; // JSON string representing AiEventPlanParsed
}

export interface BudgetCategory {
  name: string;
  amount: number;
  percentage: number;
  description: string;
}

export interface BudgetAllocationResponse {
  totalBudget: number;
  eventType: string;
  categories: BudgetCategory[];
  advice: string;
}

export interface TimelineItem {
  time: string;
  activity: string;
  duration: string;
  importance: string; // Low, Medium, High
}

export interface EventTimelineResponse {
  eventId: string;
  eventTitle: string;
  timeline: TimelineItem[];
  planningNotes: string;
}

export interface RecommendationItem {
  ServiceId: string;
  ServiceName: string;
  VendorName: string;
  Reasoning: string;
}

export interface RecommendationResponse {
  recommendations: RecommendationItem[];
}

export interface ApiResult<T> {
  isSuccess: boolean;
  value: T;
  error?: {
    code: number;
    type: string;
    description: string;
  };
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
  postalCode?: string;
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
  itemStatus: 'Pending' | 'Approved' | 'Paid' | 'Rejected' | 'Done' | 'Completed';
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
  packageId?: string;
  quantity: number;
  // Legacy fields sent by some callers — backend ignores these
  serviceImage?: string;
  serviceName?: string;
  price?: number;
  vendorId?: string;
  vendorName?: string;
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

export interface EventCollaboratorDto {
  userId: string;
  fullName: string;
  email: string;
  role: 0 | 1; // 0 = Viewer, 1 = Editor
  invitedAt: string;
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
  type?: number; // NotificationType enum ordinal from backend (integer, no StringEnumConverter)
  isRead: boolean;
  createdAt: string;
  isLive?: boolean; // true when pushed in real-time via SSE or SignalR
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
  orderId: string;
}

// Backend returns a plain string (iframe URL) for paid orders,
// or this object for free/zero-amount orders.
export interface PaymobFreeResponse {
  isFree: true;
  message: string;
  redirectUrl: string;
}

// ─────────────────────────────────────────────
// Support Tickets
// ─────────────────────────────────────────────
export interface CreateTicketRequest {
  title: string;
  description: string;
  type: string;
  priority: string;
  bookingRef?: string | null;
}

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
