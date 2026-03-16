import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

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
export class MyBookingsComponent {
  stats = {
    all: 9,
    confirmed: 4,
    pending: 3,
    completed: 2,
    cancelled: 0
  };

  activeTab = 'all';

  bookings: Booking[] = [
    {
      id: 'BK-312',
      vendorName: 'White Rose Decor',
      serviceType: 'Wedding Stage Floral',
      eventRef: "Sara & Karim's Wedding · Jun 14",
      status: 'Confirmed',
      price: '18,000 EGP',
      icon: ''
    },
    {
      id: 'BK-311',
      vendorName: 'Royal Hall Cairo',
      serviceType: 'Wedding Hall Rental',
      eventRef: "Sara & Karim's Wedding · Jun 14",
      status: 'Confirmed',
      price: '45,000 EGP',
      icon: '️'
    }
  ];

  get filteredBookings() {
    if (this.activeTab === 'all') return this.bookings;
    return this.bookings.filter(b => b.status.toLowerCase() === this.activeTab);
  }

  setTab(tab: string) {
    this.activeTab = tab;
  }
}
