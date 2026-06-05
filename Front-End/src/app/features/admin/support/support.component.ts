import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SupportService } from '../../../core/services/support.service';
import { SupportTicket, TicketStats, TicketFilters } from '../../../shared/types/api.interfaces';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-support',
  standalone: true,
  imports: [CommonModule, RouterModule, PaginationComponent],
  templateUrl: './support.component.html',
  styleUrls: ['./support.component.scss']
})
export class SupportComponent implements OnInit {
  stats: TicketStats | null = null;
  tickets: SupportTicket[] = [];
  totalTickets = 0;
  loading = true;

  activeTab: 'open' | 'in_progress' | 'resolved' = 'open';
  filters: TicketFilters = {
    page: 1,
    limit: 10,
    status: 'open'
  };

  errorMessage: string | null = null;

  constructor(private supportService: SupportService) {}

  ngOnInit(): void {
    this.loadStats();
    this.loadTickets();
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalTickets / (this.filters.limit || 10)));
  }

  loadStats(): void {
    this.supportService.getStats().subscribe({
      next: (data) => this.stats = data,
      error: () => {
        this.errorMessage = 'Failed to load ticket statistics. Please check if the backend service is running correctly.';
      }
    });
  }

  loadTickets(): void {
    this.loading = true;
    this.errorMessage = null;
    this.supportService.listTickets(this.filters).subscribe({
      next: (res) => {
        this.tickets = res.data;
        this.totalTickets = res.total;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load tickets. Please check if the backend service is running correctly.';
        this.loading = false;
      }
    });
  }

  setTab(status: 'open' | 'in_progress' | 'resolved'): void {
    this.activeTab = status;
    this.filters.status = status;
    this.filters.page = 1;
    this.loadTickets();
  }

  onPageChange(page: number): void {
    this.filters.page = page;
    this.loadTickets();
  }
}
