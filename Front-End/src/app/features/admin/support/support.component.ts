import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SupportService } from '../../../core/services/support.service';
import { SupportTicket, TicketStats, TicketFilters } from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-support',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './support.component.html',
  styleUrls: ['./support.component.scss']
})
export class SupportComponent implements OnInit {
  stats: TicketStats | null = null;
  tickets: SupportTicket[] = [];
  totalTickets: number = 0;
  loading: boolean = true;
  
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

  loadStats(): void {
    this.supportService.getStats().subscribe({
      next: (data) => this.stats = data,
      error: (err) => {
        console.error('Failed to load stats', err);
        this.errorMessage = 'Failed to load ticket statistics. Please check if the backend service is running correctly.';
      }
    });
  }

  loadTickets(): void {
    this.loading = true;
    this.errorMessage = null; // Reset error on new load
    this.supportService.listTickets(this.filters).subscribe({
      next: (res) => {
        this.tickets = res.data;
        this.totalTickets = res.total;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load tickets', err);
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
}
