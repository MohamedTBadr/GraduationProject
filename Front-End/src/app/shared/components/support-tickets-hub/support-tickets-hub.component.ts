import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SupportService, SubmittedTicketRecord } from '../../../core/services/support.service';
import { SupportTicketModalComponent } from '../support-ticket-modal/support-ticket-modal.component';
import { TicketSubmitterType } from '../../utils/support-ticket.utils';

@Component({
  selector: 'app-support-tickets-hub',
  standalone: true,
  imports: [CommonModule, SupportTicketModalComponent],
  templateUrl: './support-tickets-hub.component.html',
  styleUrls: ['./support-tickets-hub.component.scss'],
})
export class SupportTicketsHubComponent implements OnInit {
  @Input() submitterType: TicketSubmitterType = 'Client';
  @Input() pageTitle = 'Support';
  @Input() pageSubtitle = 'Open a ticket anytime — we typically respond within 1–2 business days.';
  @Input() emptyHint = 'No tickets yet. Use the button above if you need help from our team.';

  tickets: SubmittedTicketRecord[] = [];
  isModalOpen = false;
  selectedBookingRef = '';
  loading = true;
  loadError: string | null = null;

  constructor(private supportService: SupportService) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.loading = true;
    this.loadError = null;
    this.supportService.listMyTickets({ page: 1, limit: 50 }, this.submitterType).subscribe({
      next: (tickets) => {
        this.tickets = tickets;
        this.loading = false;
      },
      error: () => {
        this.loadError = 'Unable to load your tickets. Please try again.';
        this.loading = false;
      },
    });
  }

  openNewTicket(bookingRef = ''): void {
    this.selectedBookingRef = bookingRef;
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.selectedBookingRef = '';
  }

  onTicketSubmitted(): void {
    this.loadTickets();
  }
}
