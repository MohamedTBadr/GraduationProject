import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventService } from '../../../core/services/event.service';
import { EventResponseDto } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { ReviewModalComponent } from './review-modal.component';

interface Booking {
  id: string;
  eventId: string;
  vendorName: string;
  serviceType: string;
  eventRef: string;
  status: 'Confirmed' | 'Pending' | 'Completed' | 'Cancelled' | 'Done';
  price: string;
  icon: string;
}

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, ReviewModalComponent],
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
  bookings: Booking[] = [];
  loading = true;
  userId = '';

  isReviewModalOpen = false;
  selectedServiceId = '';

  constructor(
    private eventService: EventService,
    private authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadBookings();
  }

  loadBookings() {
    const user = this.authService.user();
    if (!user || user.role !== 'User') {
      this.loading = false;
      return;
    }
    this.userId = user.id;

    this.eventService.getByUser(user.id).subscribe({
      next: (events: EventResponseDto[]) => {
        let allBookings: Booking[] = [];
        
        events.forEach(ev => {
          if (ev.eventItems && ev.eventItems.length > 0) {
            ev.eventItems.forEach(item => {
              let localStatus: 'Confirmed' | 'Pending' | 'Completed' | 'Cancelled' | 'Done' = 'Pending';
              if (item.itemStatus === 'Approved') localStatus = 'Confirmed';
              if (item.itemStatus === 'Rejected') localStatus = 'Cancelled';
              if (item.itemStatus === 'Done') localStatus = 'Done';
              if (item.itemStatus === 'Completed') localStatus = 'Completed';
              
              const evDate = new Date(ev.eventDate);
              const isPast = evDate.getTime() < new Date().getTime();
              // Auto-complete confirmed past events if not manually done
              if (localStatus === 'Confirmed' && isPast) {
                localStatus = 'Completed';
              }

              allBookings.push({
                id: item.id || `BK-${Math.floor(Math.random() * 10000)}`,
                eventId: ev.id,
                vendorName: item.vendorName || 'Unknown Vendor',
                serviceType: item.serviceName || 'Service',
                eventRef: `${ev.title} · ${new Date(ev.eventDate).toLocaleDateString('en-US', {month: 'short', day: 'numeric'})}`,
                status: localStatus,
                price: `${item.price.toLocaleString()} EGP`,
                icon: '🏪'
              });
            });
          }
        });

        this.bookings = allBookings;
        this.stats.all = allBookings.length;
        this.stats.confirmed = allBookings.filter(b => b.status === 'Confirmed').length;
        this.stats.pending = allBookings.filter(b => b.status === 'Pending').length;
        this.stats.completed = allBookings.filter(b => b.status === 'Completed').length;
        this.stats.cancelled = allBookings.filter(b => b.status === 'Cancelled').length;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load bookings', err);
        this.toastService.show('Failed to load your bookings.', 'error');
        this.loading = false;
      }
    });
  }

  get filteredBookings() {
    if (this.activeTab === 'all') return this.bookings;
    return this.bookings.filter(b => b.status.toLowerCase() === this.activeTab);
  }

  setTab(tab: string) {
    this.activeTab = tab;
  }

  confirmCompletion(bk: Booking) {
    this.eventService.updateItemStatus(bk.eventId, bk.id, 'Completed').subscribe({
      next: () => {
        this.toastService.show('Service confirmed as Completed.', 'success');
        this.loadBookings();
      },
      error: (err) => {
        console.error('Failed to confirm completion', err);
        this.toastService.show('Failed to confirm completion.', 'error');
      }
    });
  }

  openReviewModal(bk: Booking) {
    // We assume booking.id is the serviceId, or we could pass it if it's stored.
    // For now, let's use a mock service id or the booking's vendor service ref.
    this.selectedServiceId = bk.id; 
    this.isReviewModalOpen = true;
  }

  closeReviewModal() {
    this.isReviewModalOpen = false;
    this.selectedServiceId = '';
  }

  reportIssue(bk: Booking) {
    // To be implemented (Open support ticket modal)
  }
}
