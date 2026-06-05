import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { OrderService, OrderResponse } from '../../../core/services/order.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-bookings',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './bookings.component.html',
  styleUrls: ['./bookings.component.scss']
})
export class BookingsComponent implements OnInit {
  allOrders: OrderResponse[] = [];
  loading = false;
  searchTerm = '';
  statusFilter = 'All';
  pageNumber = 1;
  pageSize = 15;

  readonly statusOptions = ['All', 'Pending', 'Paid', 'Failed', 'Cancelled', 'Refunded'];

  private searchSubject = new Subject<string>();

  constructor(
    private orderService: OrderService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadOrders();
    this.searchSubject.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.pageNumber = 1;
    });
  }

  loadOrders() {
    this.loading = true;
    this.orderService.getAllOrders().subscribe({
      next: orders => { this.allOrders = orders; this.loading = false; },
      error: () => { this.toastService.show('Failed to load orders', 'error'); this.loading = false; }
    });
  }

  onSearchChange() {
    this.searchSubject.next(this.searchTerm);
  }

  onStatusChange() {
    this.pageNumber = 1;
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

  get paginatedOrders(): OrderResponse[] {
    const start = (this.pageNumber - 1) * this.pageSize;
    return this.filteredOrders.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.filteredOrders.length / this.pageSize));
  }

  onPageChange(page: number) {
    this.pageNumber = page;
  }

  countByStatus(status: string): number {
    return this.allOrders.filter(o =>
      (o.paymentStatus ?? '').toLowerCase() === status.toLowerCase()
    ).length;
  }

  statusPillClass(status: string): string {
    const s = (status ?? '').toLowerCase();
    if (s === 'paid') return 'ap-green';
    if (s === 'pending') return 'ap-amber';
    if (s === 'cancelled' || s === 'failed') return 'ap-red';
    if (s === 'refunded') return 'ap-amber';
    return 'ap-amber';
  }

  shortId(id: string): string {
    return id ? id.slice(0, 8).toUpperCase() : '—';
  }
}
