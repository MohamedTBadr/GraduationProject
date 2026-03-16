import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalService } from '../../../shared/services/modal.service';

interface DashboardStat {
  label: string;
  value: string;
  icon: string;
  trend: string;
  isCurrency?: boolean;
  isDanger?: boolean;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent {
  modalService = inject(ModalService);

  stats: DashboardStat[] = [
    { label: 'Total Users', value: '12,450', icon: '', trend: '+12%' },
    { label: 'Active Vendors', value: '842', icon: '', trend: '+5%' },
    { label: 'Platform Revenue', value: '345,000', icon: '', trend: '+24%', isCurrency: true },
    { label: 'Open Tickets', value: '12', icon: '', trend: '-2%', isDanger: true }
  ];

  healthMetrics = [
    { label: 'Booking Success Rate', value: '98.5%', status: 'optimal' },
    { label: 'Avg. Response Time', value: '18m', status: 'optimal' },
    { label: 'System Uptime', value: '99.99%', status: 'optimal' },
    { label: 'Dispute Rate', value: '0.4%', status: 'warning' }
  ];

  chartData = [
    { day: 'Mon', value: 45 },
    { day: 'Tue', value: 72 },
    { day: 'Wed', value: 65 },
    { day: 'Thu', value: 89 },
    { day: 'Fri', value: 110 },
    { day: 'Sat', value: 95 },
    { day: 'Sun', value: 55 }
  ];

  applications = [
    { id: 1, name: 'Lumiere Events', category: 'Photography', location: 'Cairo, Egypt', date: 'Oct 24, 2026', status: 'Reviewing' },
    { id: 2, name: 'The Golden Plate', category: 'Catering', location: 'Giza, Egypt', date: 'Oct 23, 2026', status: 'Approved' },
    { id: 3, name: 'White Rose Decor', category: 'Decoration', location: 'New Cairo, Egypt', date: 'Oct 22, 2026', status: 'Pending' }
  ];

  approveApp(id: number) {
    const app = this.applications.find(a => a.id === id);
    if (app) app.status = 'Approved';
  }

  reviewApp(app: any) {
    this.modalService.open('review-vendor', app);
  }

  rejectApp(id: number) {
    const app = this.applications.find(a => a.id === id);
    if (app) app.status = 'Rejected';
  }

  openQuickActions() {
    this.modalService.open('quick-action');
  }

  openAddVendor() {
    this.modalService.open('add-vendor');
  }

  openAddPackage() {
    this.modalService.open('add-package');
  }

  openScheduleReport() {
    this.modalService.open('schedule-report');
  }
}
