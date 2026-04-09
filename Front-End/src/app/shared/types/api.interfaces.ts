// ─────────────────────────────────────────────
// Authentication
// ─────────────────────────────────────────────
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface AuthApiResponse {
  value: { name: string;
    email: string;
    accessToken: string;
    refreshToken: string; 
    role: string; }
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

// ─────────────────────────────────────────────
// Category
// ─────────────────────────────────────────────
export interface Category {
  id: string;
  name: string;
  // description?: string;
}

export interface CreateCategoryRequest {
  name: string;
  // description?: string;
}

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
  categoryId?: string;
  categoryName?: string;
  location?: string;
  rating?: number;
  isApproved?: boolean;
  status?: 'active' | 'suspended' | 'pending';
  createdAt?: string;
  about?: string;
}

export interface CreateVendorRequest {
  name: string;
  email: string;
  phone?: string;
  categoryId?: string;
  location?: string;
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
  categoryId?: string;
  categoryName?: string;
  vendorId?: string;
  vendorName?: string;
  serviceTypeId?: string;
  serviceTypeName?: string;
  imageUrl?: string;
  status?: 'active' | 'paused';
  duration?: string;
  leadTime?: string;
  createdAt?: string;
}

export interface CreateProductRequest {
  name: string;
  description?: string;
  price: number;
  categoryId?: string;
  serviceTypeId?: string;
  imageUrl?: string;
  status?: 'active' | 'paused';
  duration?: string;
  leadTime?: string;
}

export interface UpdateProductRequest {
  name?: string;
  description?: string;
  price?: number;
  categoryId?: string;
  serviceTypeId?: string;
  imageUrl?: string;
  status?: 'active' | 'paused';
  duration?: string;
  leadTime?: string;
}

// ─────────────────────────────────────────────
// Payment
// ─────────────────────────────────────────────
export interface PaymobPaymentRequest {
  amount: number;
  currency?: string;
  productId?: string;
  description?: string;
}

export interface PaymobPaymentResponse {
  paymentUrl?: string;
  paymentKey?: string;
  orderId?: string;
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
  zipCode: string;
  country: string;
}

export interface EventItemResponseDto {
  id: string;
  eventId: string;
  serviceImage?: string;
  serviceName: string;
  price: number;
  vendorId: string;
  vendorName: string;
  quantity: number;
  itemStatus: 'Pending' | 'Approved' | 'Rejected';
  rejectionReason?: string;
}

export interface EventResponseDto {
  id: string;
  userId: string;
  userName: string;
  title: string;
  categoryName: string;
  eventDate: string;
  totalBudget: number;
  guestCount: number;
  notes?: string;
  eventStatus: string;
  location: AddressDto;
  eventItems: EventItemResponseDto[];
}
