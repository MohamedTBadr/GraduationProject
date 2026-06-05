import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { OrderService, OrderResponse } from '../../../core/services/order.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './bookings.component.html',
  styleUrls: ['./bookings.component.scss']
})
export class BookingsComponent implements OnInit {
  allOrders: OrderResponse[] = [];
  loading = false;
  searchTerm = '';
  statusFilter = 'All';

  readonly statusOptions = ['All', 'Pending', 'Paid', 'Failed', 'Cancelled', 'Refunded'];

  constructor(
    private orderService: OrderService,
    private toastService: ToastService
  ) {}

  ngOnInit() { this.loadOrders(); }

  loadOrders() {
    this.loading = true;
    this.orderService.getAllOrders().subscribe({
      next: orders => { this.allOrders = orders; this.loading = false; },
      error: ()     => { this.toastService.show('Failed to load orders', 'error'); this.loading = false; }
    });
  }

  get filteredOrders(): OrderResponse[] {
    return this.allOrders.filter(o => {
      const matchStatus = this.statusFilter === 'All' ||
        (o.paymentStatus ?? '').toLowerCase() === this.statusFilter.toLowerCase();
      const term = this.searchTerm.toLowerCase();
      const matchSearch = !term ||
        o.id.toLowerCase().includes(term) ||
        o.eventId.toLowerCase().includes(term) ||
        o.userId.toLowerCase().includes(term);
      return matchStatus && matchSearch;
    });
  }

  countByStatus(status: string): number {
    return this.allOrders.filter(o =>
      (o.paymentStatus ?? '').toLowerCase() === status.toLowerCase()
    ).length;
  }

  statusPillClass(status: string): string {
    const s = (status ?? '').toLowerCase();
    if (s === 'paid')      return 'ap-green';
    if (s === 'pending')   return 'ap-amber';
    if (s === 'cancelled') return 'ap-red';
    if (s === 'failed')    return 'ap-red';
    if (s === 'refunded')  return 'ap-amber';
    return 'ap-amber';
  }

  shortId(id: string): string {
    return id ? id.slice(0, 8).toUpperCase() : '—';
  }
}
