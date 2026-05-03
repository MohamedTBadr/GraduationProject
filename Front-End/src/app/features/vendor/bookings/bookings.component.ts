import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { EventService } from '../../../core/services/event.service';
import { EventResponseDto, EventItemResponseDto } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';

export interface Booking {
  id: string; // itemId
  eventId: string;
  client: string;
  event?: string;
  service: string;
  dateStr: string;
  value: number;
  guests?: number;
  note?: string;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Done' | 'Completed';
  stars?: number;
}

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule],
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

  calendarDays = Array.from({length: 31}, (_, i) => i + 1);
  bookedDates = [5, 12, 14, 18, 22, 25, 28];

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

  private processBookings(events: EventResponseDto[]) {
    const pending: Booking[] = [];
    const confirmed: Booking[] = [];
    const history: Booking[] = [];

    events.forEach(event => {
      event.eventItems.forEach(item => {
        if (item.vendorId === this.vendorId) {
          const booking: Booking = {
            id: item.id,
            eventId: event.id,
            client: event.userName,
            event: event.title,
            service: item.serviceName,
            dateStr: event.eventDate,
            value: item.price * item.quantity,
            guests: event.guestCount,
            note: event.notes,
            status: item.itemStatus
          };

          if (item.itemStatus === 'Pending') {
            pending.push(booking);
          } else if (item.itemStatus === 'Approved') {
            confirmed.push(booking);
          } else if (item.itemStatus === 'Rejected') {
            history.push(booking);
          }
        }
      });
    });

    this.pendingBookings = pending;
    this.confirmedBookings = confirmed;
    this.historyBookings = history;
  }

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
    this.isDetailsModalOpen = false; // close details if open
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

  acceptBooking() {
    if (this.selectedBooking) {
      this.eventService.approveItem(this.selectedBooking.eventId, this.selectedBooking.id, { approve: true }).subscribe({
        next: () => {
          this.toastService.show('Booking accepted successfully.', 'success');
          this.loadBookings();
          this.closeDetails();
        },
        error: (err) => {
          console.error('Error accepting booking', err);
          this.toastService.show('Failed to accept booking.', 'error');
        }
      });
    }
  }

  submitDecline() {
    if (this.selectedBooking && this.declineReason) {
      const fullReason = `${this.declineReason}${this.declineNote ? ': ' + this.declineNote : ''}`;
      this.eventService.approveItem(this.selectedBooking.eventId, this.selectedBooking.id, { approve: false, reason: fullReason }).subscribe({
        next: () => {
          this.toastService.show('Booking declined.', 'info');
          this.loadBookings();
          this.closeDeclineForm();
        },
        error: (err) => {
          console.error('Error declining booking', err);
          this.toastService.show('Failed to decline booking.', 'error');
        }
      });
    }
  }

  markAsDone(booking: Booking) {
    this.eventService.updateItemStatus(booking.eventId, booking.id, 'Done').subscribe({
      next: () => {
        this.toastService.show('Service marked as Done.', 'success');
        this.loadBookings();
      },
      error: (err) => {
        console.error('Error updating status', err);
        this.toastService.show('Failed to update status.', 'error');
      }
    });
  }

  switchTab(tab: 'pending' | 'confirmed' | 'calendar' | 'history') {
    this.currentTab = tab;
  }
}
