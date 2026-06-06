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
  @Input() pageSubtitle = 'Open a ticket and track submissions from this device.';
  @Input() emptyHint = 'No tickets submitted yet. Use the button above to contact support.';

  tickets: SubmittedTicketRecord[] = [];
  isModalOpen = false;
  selectedBookingRef = '';

  constructor(private supportService: SupportService) {}

  ngOnInit(): void {
    this.refreshTickets();
  }

  refreshTickets(): void {
    this.tickets = this.supportService.getSubmittedTickets(this.submitterType);
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
    this.refreshTickets();
  }
}
