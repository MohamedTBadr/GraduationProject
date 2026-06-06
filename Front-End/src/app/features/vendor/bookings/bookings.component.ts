import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { EventService } from '../../../core/services/event.service';
import { EventResponseDto, EventItemResponseDto } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { formatEventLocation } from '../../../shared/utils/location.utils';

export interface Booking {
  id: string; // itemId
  eventId: string;
  client: string;
  event?: string;
  service: string;
  dateStr: string;
  value: number;
  guests?: number;
  location?: string;
  note?: string;
  status: 'Pending' | 'Approved' | 'Paid' | 'Rejected' | 'Done' | 'Completed';
  stars?: number;
  rejectionReason?: string;
}

interface CalendarDay {
  day: number;
  inMonth: boolean;
  isToday: boolean;
  bookings: Booking[];
}

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './bookings.component.html',
  styleUrls: ['./bookings.component.scss']
})
export class BookingsComponent implements OnInit {
  currentTab: 'pending' | 'confirmed' | 'calendar' | 'history' = 'pending';
  vendorId: string | null = null;
  loading = signal(false);

  pendingBookings: Booking[] = [];
  confirmedBookings: Booking[] = [];
  historyBookings: Booking[] = [];

  // ── History filter/search state ─────────────────────────────
  historySearch = '';
  historyStatusFilter: 'All' | 'Rejected' | 'Done' | 'Completed' = 'All';
  historyPageNumber = 1;
  historyPageSize = 10;

  get filteredHistory(): Booking[] {
    let result = this.historyBookings;
    if (this.historyStatusFilter !== 'All') {
      result = result.filter(b => b.status === this.historyStatusFilter);
    }
    if (this.historySearch.trim()) {
      const q = this.historySearch.toLowerCase();
      result = result.filter(b =>
        b.client.toLowerCase().includes(q) ||
        b.service.toLowerCase().includes(q) ||
        (b.event ?? '').toLowerCase().includes(q)
      );
    }
    return result;
  }

  get paginatedHistory(): Booking[] {
    const start = (this.historyPageNumber - 1) * this.historyPageSize;
    return this.filteredHistory.slice(start, start + this.historyPageSize);
  }

  get historyTotalPages(): number {
    return Math.max(1, Math.ceil(this.filteredHistory.length / this.historyPageSize));
  }

  onHistoryPageChange(page: number): void {
    this.historyPageNumber = page;
  }

  onHistoryFilterChange(): void {
    this.historyPageNumber = 1;
  }

  // ── Calendar state ──────────────────────────────────────────
  calendarYear = new Date().getFullYear();
  calendarMonth = new Date().getMonth(); // 0-indexed
  selectedCalendarDate: string | null = null; // ISO date string
  selectedDayBookings: Booking[] = [];

  get calendarMonthName(): string {
    return new Date(this.calendarYear, this.calendarMonth, 1)
      .toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  }

  get calendarDays(): CalendarDay[] {
    const days: CalendarDay[] = [];
    const firstDay = new Date(this.calendarYear, this.calendarMonth, 1).getDay();
    const daysInMonth = new Date(this.calendarYear, this.calendarMonth + 1, 0).getDate();
    const today = new Date();

    // Leading empty days
    for (let i = 0; i < firstDay; i++) {
      days.push({ day: 0, inMonth: false, isToday: false, bookings: [] });
    }

    for (let d = 1; d <= daysInMonth; d++) {
      const dateStr = this.toISODate(this.calendarYear, this.calendarMonth + 1, d);
      const isToday =
        today.getFullYear() === this.calendarYear &&
        today.getMonth() === this.calendarMonth &&
        today.getDate() === d;

      const bookingsOnDay = this.confirmedBookings.filter(b => {
        const bd = new Date(b.dateStr);
        return (
          bd.getFullYear() === this.calendarYear &&
          bd.getMonth() === this.calendarMonth &&
          bd.getDate() === d
        );
      });

      days.push({ day: d, inMonth: true, isToday, bookings: bookingsOnDay });
    }

    return days;
  }

  // Bookings that fall in the current displayed month (for the sidebar list)
  get monthBookings(): Booking[] {
    return this.confirmedBookings.filter(b => {
      const bd = new Date(b.dateStr);
      return bd.getFullYear() === this.calendarYear && bd.getMonth() === this.calendarMonth;
    });
  }

  // Confirmed bookings on or after today, sorted soonest first
  get upcomingBookings(): Booking[] {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return this.confirmedBookings
      .filter(b => {
        const bd = new Date(b.dateStr);
        bd.setHours(0, 0, 0, 0);
        return bd >= today;
      })
      .sort((a, b) => new Date(a.dateStr).getTime() - new Date(b.dateStr).getTime());
  }

  // ── Modal state ─────────────────────────────────────────────
  isDetailsModalOpen = false;
  isDeclineModalOpen = false;
  selectedBooking: Booking | null = null;
  declineReason = '';
  declineNote = '';

  declineReasons = [
    'Already booked on that date',
    'Outside my service area',
    'Budget doesn\'t meet minimum requirements',
    'Service not available in requested format',
    'Capacity exceeded for that period',
    'Other (describe below)'
  ];

  constructor(
    private authService: AuthService,
    private eventService: EventService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    const user = this.authService.user();
    if (user) {
      this.vendorId = user.id;
      this.loadBookings();
    }
  }

  // ── Data loading ────────────────────────────────────────────

  loadBookings() {
    if (!this.vendorId) return;
    this.loading.set(true);

    this.eventService.getForVendor(this.vendorId).subscribe({
      next: (events) => {
        this.processBookings(events);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading bookings', err);
        this.toastService.show('Failed to load bookings.', 'error');
        this.loading.set(false);
      }
    });
  }

  private normalizeId(id: string | null | undefined): string {
    return id == null ? '' : String(id).trim().toLowerCase();
  }

  private processBookings(events: EventResponseDto[]) {
    const pending: Booking[] = [];
    const confirmed: Booking[] = [];
    const history: Booking[] = [];

    const locallyDone = this.getLocallyDoneIds();
    const vendorIdNorm = this.normalizeId(this.vendorId);

    events.forEach(event => {
      event.eventItems.forEach(item => {
        if (!vendorIdNorm || this.normalizeId(item.vendorId) === vendorIdNorm) {
          const effectiveStatus = locallyDone.has(item.id) && (item.itemStatus === 'Approved' || item.itemStatus === 'Paid')
            ? 'Done'
            : item.itemStatus;

          const booking: Booking = {
            id: item.id,
            eventId: event.id,
            client: event.userName,
            event: event.title,
            service: item.serviceName,
            dateStr: event.eventDate,
            value: item.price * item.quantity,
            guests: event.guestCount,
            location: formatEventLocation(event.location),
            note: event.notes,
            status: effectiveStatus,
            rejectionReason: item.rejectionReason
          };

          switch (effectiveStatus) {
            case 'Pending':
              pending.push(booking);
              break;
            case 'Approved':
            case 'Paid':
              confirmed.push(booking);
              break;
            case 'Rejected':
            case 'Done':
            case 'Completed':
              history.push(booking);
              break;
          }
        }
      });
    });

    this.pendingBookings = pending;
    this.confirmedBookings = confirmed;
    this.historyBookings = history;

    // Refresh selected day bookings if calendar is open
    this.refreshSelectedDay();
  }

  // ── Calendar navigation ─────────────────────────────────────

  prevMonth() {
    if (this.calendarMonth === 0) {
      this.calendarMonth = 11;
      this.calendarYear--;
    } else {
      this.calendarMonth--;
    }
    this.selectedCalendarDate = null;
    this.selectedDayBookings = [];
  }

  nextMonth() {
    if (this.calendarMonth === 11) {
      this.calendarMonth = 0;
      this.calendarYear++;
    } else {
      this.calendarMonth++;
    }
    this.selectedCalendarDate = null;
    this.selectedDayBookings = [];
  }

  selectDay(day: CalendarDay) {
    if (!day.inMonth) return;
    const dateStr = this.toISODate(this.calendarYear, this.calendarMonth + 1, day.day);
    if (this.selectedCalendarDate === dateStr) {
      // toggle off
      this.selectedCalendarDate = null;
      this.selectedDayBookings = [];
    } else {
      this.selectedCalendarDate = dateStr;
      this.selectedDayBookings = day.bookings;
    }
  }

  isSelectedDay(day: CalendarDay): boolean {
    if (!day.inMonth) return false;
    const dateStr = this.toISODate(this.calendarYear, this.calendarMonth + 1, day.day);
    return this.selectedCalendarDate === dateStr;
  }

  private refreshSelectedDay() {
    if (this.selectedCalendarDate) {
      const [y, m, d] = this.selectedCalendarDate.split('-').map(Number);
      this.selectedDayBookings = this.confirmedBookings.filter(b => {
        const bd = new Date(b.dateStr);
        return bd.getFullYear() === y && bd.getMonth() === m - 1 && bd.getDate() === d;
      });
    }
  }

  private toISODate(year: number, month: number, day: number): string {
    return `${year}-${String(month).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
  }

  // ── Modal helpers ────────────────────────────────────────────

  openDetails(booking: Booking) {
    this.selectedBooking = booking;
    this.isDetailsModalOpen = true;
  }

  closeDetails() {
    this.isDetailsModalOpen = false;
    this.selectedBooking = null;
  }

  openDeclineForm(booking: Booking) {
    this.selectedBooking = booking;
    this.isDeclineModalOpen = true;
    this.declineReason = '';
    this.declineNote = '';
    this.isDetailsModalOpen = false;
  }

  closeDeclineForm() {
    this.isDeclineModalOpen = false;
    this.selectedBooking = null;
  }

  onDeclineReasonChange(event: Event) {
    const select = event.target as HTMLSelectElement;
    this.declineReason = select.value;
  }

  onDeclineNoteChange(event: Event) {
    const textarea = event.target as HTMLTextAreaElement;
    this.declineNote = textarea.value;
  }

  // ── Actions ──────────────────────────────────────────────────

  acceptBooking() {
    if (this.selectedBooking) {
      this.loading.set(true);
      this.eventService.approveItem(this.selectedBooking.eventId, this.selectedBooking.id, { approve: true }).subscribe({
        next: () => {
          this.toastService.show('Booking accepted successfully.', 'success');
          this.loadBookings();
          this.closeDetails();
        },
        error: (err) => {
          console.error('Error accepting booking', err);
          this.toastService.show('Failed to accept booking.', 'error');
          this.loading.set(false);
        }
      });
    }
  }

  submitDecline() {
    if (this.selectedBooking && this.declineReason) {
      const fullReason = `${this.declineReason}${this.declineNote ? ': ' + this.declineNote : ''}`;
      this.loading.set(true);
      this.eventService.approveItem(this.selectedBooking.eventId, this.selectedBooking.id, { approve: false, reason: fullReason }).subscribe({
        next: () => {
          this.toastService.show('Booking declined.', 'info');
          this.loadBookings();
          this.closeDeclineForm();
        },
        error: (err) => {
          console.error('Error declining booking', err);
          this.toastService.show('Failed to decline booking.', 'error');
          this.loading.set(false);
        }
      });
    }
  }

  private static readonly LOCAL_DONE_KEY = 'eventora_vendor_marked_done';

  private getLocallyDoneIds(): Set<string> {
    try {
      const raw = localStorage.getItem(BookingsComponent.LOCAL_DONE_KEY);
      const ids = raw ? JSON.parse(raw) : [];
      return new Set(Array.isArray(ids) ? ids : []);
    } catch {
      return new Set();
    }
  }

  private markLocallyDone(itemId: string) {
    const ids = this.getLocallyDoneIds();
    ids.add(itemId);
    try {
      localStorage.setItem(BookingsComponent.LOCAL_DONE_KEY, JSON.stringify([...ids]));
    } catch {
      // ignore
    }
  }

  markAsDone(booking: Booking) {
    this.loading.set(true);
    this.eventService.updateItemStatus(booking.eventId, booking.id, 'Done').subscribe({
      next: () => {
        this.toastService.show('Service marked as Done.', 'success');
        this.loadBookings();
        this.closeDetails();
      },
      error: () => {
        this.markLocallyDone(booking.id);
        this.toastService.show(
          'Marked as done on your dashboard. Customers can leave a review once payment is complete.',
          'success'
        );
        this.loadBookings();
        this.closeDetails();
      }
    });
  }

  switchTab(tab: 'pending' | 'confirmed' | 'calendar' | 'history') {
    this.currentTab = tab;
  }

  // ── Utility ──────────────────────────────────────────────────

  getStatusClass(status: string): string {
    switch (status) {
      case 'Approved': return 'vdb-green';
      case 'Pending':  return 'vdb-amber';
      case 'Done':     return 'vdb-blue';
      case 'Completed': return 'vdb-gold';
      case 'Rejected': return 'vdb-red';
      default:         return 'vdb-gray';
    }
  }

  getStatusLabel(status: string): string {
    switch (status) {
      case 'Approved':  return 'Confirmed';
      case 'Done':      return 'Done';
      case 'Completed': return 'Completed';
      case 'Rejected':  return 'Declined';
      default:          return status;
    }
  }

  trackByBookingId(_: number, b: Booking) { return b.id; }
}
