import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventService } from '../../../core/services/event.service';
import { EventResponseDto } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { AuthService } from '../../../core/services/auth.service';

interface Booking {
  id: string;
  vendorName: string;
  serviceType: string;
  eventRef: string;
  status: 'Confirmed' | 'Pending' | 'Completed' | 'Cancelled';
  price: string;
  icon: string;
}

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule],
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

    this.eventService.getByUser(user.id).subscribe({
      next: (events: EventResponseDto[]) => {
        let allBookings: Booking[] = [];
        
        events.forEach(ev => {
          if (ev.eventItems && ev.eventItems.length > 0) {
            ev.eventItems.forEach(item => {
              let localStatus: 'Confirmed' | 'Pending' | 'Completed' | 'Cancelled' = 'Pending';
              if (item.itemStatus === 'Approved') localStatus = 'Confirmed';
              if (item.itemStatus === 'Rejected') localStatus = 'Cancelled';
              
              const evDate = new Date(ev.eventDate);
              const isPast = evDate.getTime() < new Date().getTime();
              if (localStatus === 'Confirmed' && isPast) {
                localStatus = 'Completed';
              }

              allBookings.push({
                id: item.id || `BK-${Math.floor(Math.random() * 10000)}`,
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
}
