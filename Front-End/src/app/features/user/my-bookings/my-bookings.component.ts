import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { EventService } from '../../../core/services/event.service';
import { EventResponseDto } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { ReviewModalComponent } from './review-modal.component';
import { ReportIssueModalComponent } from './report-issue-modal.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { OrderService, OrderResponse } from '../../../core/services/order.service';
import { VoucherService, Voucher } from '../../../core/services/voucher.service';

interface Booking {
  id: string;
  eventId: string;
  serviceId?: string;
  vendorId?: string;
  vendorName: string;
  serviceType: string;
  eventRef: string;
  status: 'Confirmed' | 'Pending' | 'Completed' | 'Cancelled' | 'Done' | 'Paid';
  price: string;
  icon: string;
}

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, ReviewModalComponent, ReportIssueModalComponent, PaginationComponent],
  templateUrl: './my-bookings.component.html',
  styleUrls: ['./my-bookings.component.scss']
})
export class MyBookingsComponent implements OnInit {
  stats = {
    all: 0,
    confirmed: 0,
    pending: 0,
    completed: 0,
    cancelled: 0
  };

  activeTab = 'all';
  pageNumber = 1;
  pageSize = 8;
  bookings: Booking[] = [];
  orders: OrderResponse[] = [];
  ordersPageNumber = 1;
  readonly ordersPageSize = 8;
  loyaltyPoints = 0;
  loading = true;
  userId = '';

  isReviewModalOpen = false;
  selectedServiceId = '';

  isReportModalOpen = false;
  selectedBookingRef = '';

  // Voucher and referral variables
  myVouchers: Voucher[] = [];
  referralLink = '';

  constructor(
    private eventService: EventService,
    private authService: AuthService,
    private toastService: ToastService,
    private orderService: OrderService,
    private voucherService: VoucherService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadBookings();
    this.loadVouchers();
  }

  loadBookings() {
    const user = this.authService.user();
    if (!user || user.role !== 'User') {
      this.loading = false;
      return;
    }
    this.userId = user.id;

    this.eventService.getByUser().subscribe({
      next: (events: EventResponseDto[]) => {
        const allBookings = this.mapEventsToBookings(events);
        this.applyBookings(allBookings);
        this.loadOrders();
      },
      error: (err) => {
        console.error('Failed to load bookings', err);
        this.toastService.show('Failed to load your bookings.', 'error');
        this.loading = false;
      }
    });
  }

  private mapEventsToBookings(events: EventResponseDto[]): Booking[] {
    const allBookings: Booking[] = [];
    events.forEach(ev => {
      if (ev.eventItems && ev.eventItems.length > 0) {
        ev.eventItems.forEach(item => {
          let localStatus: Booking['status'] = 'Pending';
          if (item.itemStatus === 'Approved') localStatus = 'Confirmed';
          if (item.itemStatus === 'Paid') localStatus = 'Paid';
          if (item.itemStatus === 'Rejected') localStatus = 'Cancelled';
          if (item.itemStatus === 'Done') localStatus = 'Done';
          if (item.itemStatus === 'Completed' || ev.eventStatus === 'Completed') {
            localStatus = 'Completed';
          }

          allBookings.push({
                id: item.id || `BK-${Math.floor(Math.random() * 10000)}`,
                eventId: ev.id,
                serviceId: item.serviceId,
                vendorId: item.vendorId,
                vendorName: item.vendorName || 'Unknown Vendor',
                serviceType: item.serviceName || 'Service',
                eventRef: `${ev.title} · ${new Date(ev.eventDate).toLocaleDateString('en-US', {month: 'short', day: 'numeric'})}`,
                status: localStatus,
                price: localStatus === 'Pending'
                  ? (item.price > 0 ? `${item.price.toLocaleString()} EGP` : 'TBD')
                  : `${item.price.toLocaleString()} EGP`,
                icon: 'shop'
          });
        });
      }
    });
    return allBookings;
  }

  private applyBookings(allBookings: Booking[]) {
    this.bookings = allBookings;
    this.stats.all = allBookings.length;
    this.stats.confirmed = allBookings.filter(b => b.status === 'Confirmed').length;
    this.stats.pending = allBookings.filter(b => b.status === 'Pending').length;
    this.stats.completed = allBookings.filter(b => b.status === 'Completed').length;
    this.stats.cancelled = allBookings.filter(b => b.status === 'Cancelled').length;
  }

  confirmCompletion(bk: Booking) {
    this.eventService.updateItemStatus(bk.eventId, bk.id, 'Completed').subscribe({
      next: () => {
        this.eventService.getByStatus('Completed').subscribe({
          next: (completedEvents) => {
            const fromCompleted = this.mapEventsToBookings(completedEvents);
            const merged = this.mergeBookings(this.bookings, fromCompleted);
            this.applyBookings(merged);
            this.toastService.show('Service marked as completed. You can now leave a review!', 'success');
            this.openReviewModal(bk);
          },
          error: () => {
            this.loadBookings();
            this.toastService.show('Service marked as completed. You can now leave a review!', 'success');
            this.openReviewModal(bk);
          }
        });
      },
      error: () => this.toastService.show('Failed to confirm completion. Please try again.', 'error')
    });
  }

  private mergeBookings(primary: Booking[], fromStatus: Booking[]): Booking[] {
    const byId = new Map(primary.map(b => [b.id, b]));
    fromStatus.forEach(b => byId.set(b.id, b));
    return Array.from(byId.values());
  }

  loadOrders() {
    this.orderService.getOrdersByUser(this.userId).subscribe({
      next: (orders: OrderResponse[]) => {
        this.orders = orders.filter(o => !!o.id);
        this.calculateLoyaltyPoints();
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load orders', err);
        this.toastService.show('Failed to load your order history.', 'error');
        this.loading = false;
      }
    });
  }

  calculateLoyaltyPoints() {
    const eligibleOrders = this.orders.filter(o => 
      o.paymentStatus === 'Paid' || o.paymentStatus === 'Completed'
    );
    const totalSpent = eligibleOrders.reduce((sum, o) => sum + o.amount, 0);
    this.loyaltyPoints = Math.floor(totalSpent / 10);
  }

  get filteredBookings() {
    if (this.activeTab === 'all') return this.bookings;
    return this.bookings.filter(b => b.status.toLowerCase() === this.activeTab);
  }

  get paginatedBookings(): Booking[] {
    const start = (this.pageNumber - 1) * this.pageSize;
    return this.filteredBookings.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.filteredBookings.length / this.pageSize));
  }

  get paginatedOrders(): OrderResponse[] {
    const start = (this.ordersPageNumber - 1) * this.ordersPageSize;
    return this.orders.slice(start, start + this.ordersPageSize);
  }

  get ordersTotalPages(): number {
    return Math.max(1, Math.ceil(this.orders.length / this.ordersPageSize));
  }

  setTab(tab: string) {
    this.activeTab = tab;
    this.pageNumber = 1;
    this.ordersPageNumber = 1;
  }

  onPageChange(page: number) {
    this.pageNumber = page;
  }

  onOrdersPageChange(page: number) {
    this.ordersPageNumber = page;
  }

  openReviewModal(bk: Booking) {
    // Priority: serviceId from backend > item.id as fallback
    this.selectedServiceId = bk.serviceId || bk.id; 
    this.isReviewModalOpen = true;
  }

  closeReviewModal() {
    this.isReviewModalOpen = false;
    this.selectedServiceId = '';
  }

  reportIssue(bk: Booking) {
    this.selectedBookingRef = bk.id;
    this.isReportModalOpen = true;
  }

  closeReportModal() {
    this.isReportModalOpen = false;
    this.selectedBookingRef = '';
  }

  messageVendor(bk: Booking) {
    if (!bk.vendorId) {
      this.toastService.show('Vendor contact information is not available.', 'error');
      return;
    }
    this.router.navigate(['/user/messages'], {
      queryParams: { vendorId: bk.vendorId, vendorName: bk.vendorName },
    });
  }

  cancelBooking(bk: Booking) {
    const confirmed = window.confirm(
      `Are you sure you want to cancel your booking with "${bk.vendorName}"?\n\nThis will cancel the entire event associated with this booking. This action cannot be undone.`
    );
    if (!confirmed) return;

    this.eventService.cancelEvent(bk.eventId, { reason: 'Cancelled by customer from bookings page' }).subscribe({
      next: () => {
        this.toastService.show('Event has been cancelled successfully.', 'success');
        this.loadBookings();
      },
      error: (err) => {
        console.error('Failed to cancel event', err);
        this.toastService.show('Failed to cancel. Please try again or cancel from My Events.', 'error');
      }
    });
  }

  loadVouchers() {
    this.voucherService.getReferralLink().subscribe({
      next: (res) => {
        const code = (res ?? '').trim();
        this.referralLink = code ? `${window.location.origin}/register?ref=${code}` : '';
      },
      error: (err) => {
        console.error('Failed to load referral link', err);
      }
    });

    this.voucherService.getMyVouchers().subscribe({
      next: (vouchers) => {
        this.myVouchers = vouchers;
      },
      error: (err) => {
        console.error('Failed to load vouchers', err);
      }
    });
  }

  // Pay Now is enabled when at least one approved (unpaid) service exists for this event.
  // Paid items (itemStatus === 'Paid') are already paid in a previous round and don't block.
  hasApprovedItems(eventId: string): boolean {
    return this.bookings.some(b => b.eventId === eventId && b.status === 'Confirmed');
  }

  pendingItemCount(eventId: string): number {
    return this.bookings.filter(b => b.eventId === eventId && b.status === 'Pending').length;
  }

  approvedItemCount(eventId: string): number {
    return this.bookings.filter(b => b.eventId === eventId && b.status === 'Confirmed').length;
  }

  paidItemCount(eventId: string): number {
    return this.bookings.filter(b => b.eventId === eventId && b.status === 'Paid').length;
  }

  getBookingStatusLabel(status: string): string {
    if (status === 'Pending') return 'Awaiting Confirmation';
    if (status === 'Paid')    return 'Paid';
    return status;
  }

  getPaymentStatusLabel(status: string): string {
    if (status === 'Pending') return 'Awaiting Payment';
    return status;
  }

  navigateToCheckout(orderId: string) {
    this.router.navigate(['/checkout', orderId]);
  }

  copyReferralLink() {
    if (!this.referralLink) return;
    navigator.clipboard.writeText(this.referralLink).then(() => {
      this.toastService.show('Referral link copied to clipboard!', 'success');
    }).catch(err => {
      console.error('Could not copy text: ', err);
      this.toastService.show('Failed to copy referral link.', 'error');
    });
  }
}

