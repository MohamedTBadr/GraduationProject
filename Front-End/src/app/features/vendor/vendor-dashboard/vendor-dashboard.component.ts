import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface DashboardStat {
  label: string;
  value: string;
  icon: string;
  trend?: string;
  sub?: string;
  isCurrency?: boolean;
}

@Component({
  selector: 'app-vendor-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vendor-dashboard.component.html',
  styleUrls: ['./vendor-dashboard.component.scss']
})
export class VendorDashboardComponent {
  stats: DashboardStat[] = [
    { label: 'Total Bookings', value: '45', icon: '📅', trend: '+12%' },
    { label: 'Conversion Rate', value: '62%', icon: '', trend: '+5%' },
    { label: 'Total Earnings', value: '120,400', icon: '', isCurrency: true },
    { label: 'Client Rating', value: '4.9', icon: '⭐', sub: '124 reviews' }
  ];

  revenueData = [
    { month: 'Jan', val: 12000 },
    { month: 'Feb', val: 18000 },
    { month: 'Mar', val: 15000 },
    { month: 'Apr', val: 22000 },
    { month: 'May', val: 30000 },
    { month: 'Jun', val: 28000 }
  ];

  recentBookings = [
    { id: 1, event: 'Sarah Wedding', client: 'Ahmed Helmy', date: 'Dec 12, 2026', amount: '15,000', status: 'Pending' },
    { id: 2, event: 'Annual Gala', client: 'Vodafone Egypt', date: 'Nov 20, 2026', amount: '45,000', status: 'Confirmed' },
    { id: 3, event: 'Birthday Bash', client: 'Mona Zaki', date: 'Oct 30, 2026', amount: '8,000', status: 'Completed' }
  ];

  activityLogs = [
    { text: 'New booking request from Ahmed Helmy', time: '2 mins ago', icon: '' },
    { text: 'Booking "Annual Gala" payout processed', time: '1 hour ago', icon: '' },
    { text: 'Profile updated by you', time: '3 hours ago', icon: '️' },
    { text: 'New 5-star review received', time: '5 hours ago', icon: '⭐' }
  ];

  profileCompletion = 85;

  confirmBooking(id: number) {
    const b = this.recentBookings.find(x => x.id === id);
    if (b) b.status = 'Confirmed';
  }

  cancelBooking(id: number) {
    const b = this.recentBookings.find(x => x.id === id);
    if (b) b.status = 'Cancelled';
  }
}
