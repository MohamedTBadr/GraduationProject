import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface Booking {
  client: string;
  event?: string;
  service: string;
  dateStr: string;
  value: string;
  guests?: number;
  note?: string;
  status?: string;
  stars?: number;
}

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bookings.component.html',
  styleUrls: ['./bookings.component.scss']
})
export class BookingsComponent {
  currentTab: 'pending' | 'confirmed' | 'calendar' | 'history' = 'pending';

  pendingBookings: Booking[] = [
    {
      client: 'Sara Mohamed',
      event: 'Engagement Party',
      dateStr: 'Jun 12',
      service: 'Full Room Styling',
      guests: 60,
      value: '12,000',
      note: "Hi! We'd love a pink and white theme with candles and fresh roses. Budget is flexible."
    },
    {
      client: 'Khaled Hassan',
      event: 'Wedding',
      dateStr: 'Sep 20',
      service: 'Wedding Stage + Tables',
      guests: 250,
      value: '28,000',
      note: "Looking for an elegant garden theme. Venue is Four Seasons Cairo. Please send portfolio."
    },
    {
      client: 'Dina Mostafa',
      event: 'Birthday',
      dateStr: 'May 5',
      service: 'Balloon Art & Décor',
      guests: 40,
      value: '5,500',
      note: "Unicorn theme for my daughter's 7th birthday. Need balloons, cake table, and wall backdrop."
    }
  ];

  confirmedBookings: Booking[] = [
    { client: 'Nour Ahmed', event: 'Wedding', service: 'Wedding Stage Floral', dateStr: 'Jun 14', value: '18,000', status: 'confirmed' },
    { client: 'Sara Mohamed', event: 'Engagement', service: 'Room Styling', dateStr: 'Jun 12', value: '12,000', status: 'confirmed' },
    { client: 'Layla Karim', event: 'Birthday', service: 'Balloon Setup', dateStr: 'May 22', value: '4,500', status: 'pending' },
    { client: 'Omar Hassan', event: 'Corporate', service: 'Corporate Florals', dateStr: 'Apr 5', value: '8,000', status: 'confirmed' },
    { client: 'Rania Saleh', event: 'Engagement', service: 'Room Styling', dateStr: 'Mar 18', value: '10,000', status: 'confirmed' }
  ];

  historyBookings: Booking[] = [
    { client: 'Heba Mahmoud', service: 'Wedding Stage Floral', dateStr: 'Jan 15', value: '22,000', status: 'completed', stars: 5 },
    { client: 'Ahmed Faris', service: 'Engagement Styling', dateStr: 'Feb 8', value: '9,500', status: 'completed', stars: 5 },
    { client: 'Rania Said', service: 'Birthday Decor', dateStr: 'Feb 20', value: '4,000', status: 'completed', stars: 4 },
    { client: 'Mona Ibrahim', service: 'Corporate Florals', dateStr: 'Jan 28', value: '7,000', status: 'completed', stars: 5 }
  ];

  calendarDays = Array.from({length: 31}, (_, i) => i + 1);
  bookedDates = [5, 12, 14, 18, 22, 25, 28];

  switchTab(tab: 'pending' | 'confirmed' | 'calendar' | 'history') {
    this.currentTab = tab;
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
      // Remove from pending
      const idx = this.pendingBookings.indexOf(this.selectedBooking);
      if (idx > -1) {
        this.pendingBookings.splice(idx, 1);
        
        // Add to confirmed
        this.confirmedBookings.unshift({
          client: this.selectedBooking.client,
          event: this.selectedBooking.event,
          service: this.selectedBooking.service,
          dateStr: this.selectedBooking.dateStr,
          value: this.selectedBooking.value,
          status: 'confirmed'
        });
      }
      this.closeDetails();
    }
  }

  submitDecline() {
    if (this.selectedBooking && this.declineReason) {
      // Remove from pending
      const idx = this.pendingBookings.indexOf(this.selectedBooking);
      if (idx > -1) {
        this.pendingBookings.splice(idx, 1);
        
        // Add to history as cancelled
        this.historyBookings.unshift({
          client: this.selectedBooking.client,
          service: this.selectedBooking.service,
          dateStr: this.selectedBooking.dateStr,
          value: this.selectedBooking.value,
          status: 'cancelled',
          stars: 0
        });
      }
      this.closeDeclineForm();
    }
  }
}
