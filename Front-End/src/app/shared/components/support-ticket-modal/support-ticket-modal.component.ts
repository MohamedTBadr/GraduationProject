import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SupportService } from '../../../core/services/support.service';
import { ToastService } from '../toast/toast.service';
import {
  TICKET_CATEGORIES,
  TicketCategory,
  TicketSubmitterType,
  buildCreateTicketPayload,
} from '../../utils/support-ticket.utils';

@Component({
  selector: 'app-support-ticket-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './support-ticket-modal.component.html',
  styleUrls: ['./support-ticket-modal.component.scss'],
})
export class SupportTicketModalComponent {
  @Input() isOpen = false;
  @Input() submitterType: TicketSubmitterType = 'Client';
  @Input() bookingRef = '';
  @Input() modalTitle = 'Report an Issue';
  @Input() modalSubtitle = 'Tell us what went wrong. We will open a support ticket to investigate.';
  @Output() close = new EventEmitter<void>();
  @Output() submitted = new EventEmitter<string>();

  readonly categories = TICKET_CATEGORIES;

  title = '';
  description = '';
  category: TicketCategory = 'Booking';
  priority = 'medium';
  isSubmitting = false;

  constructor(
    private supportService: SupportService,
    private toastService: ToastService,
  ) {}

  get showBookingRef(): boolean {
    return !!this.bookingRef?.trim();
  }

  submitTicket(): void {
    if (!this.title.trim() || !this.description.trim()) return;

    this.isSubmitting = true;
    const payload = buildCreateTicketPayload({
      submitterType: this.submitterType,
      category: this.category,
      title: this.title,
      description: this.description,
      priority: this.priority,
      bookingRef: this.bookingRef || null,
    });

    this.supportService.openTicket(payload).subscribe({
      next: (ticket) => {
        const ticketId = ticket.ticket_id;
        const suffix = ticketId ? ` Reference: ${ticketId}` : '';
        this.toastService.show(`Support ticket opened successfully!${suffix}`, 'success');
        this.isSubmitting = false;
        this.submitted.emit(ticketId);
        this.closeModal();
      },
      error: (err: unknown) => {
        console.error('Error opening support ticket', err);
        this.toastService.show('Failed to open support ticket.', 'error');
        this.isSubmitting = false;
      },
    });
  }

  closeModal(): void {
    this.title = '';
    this.description = '';
    this.category = 'Booking';
    this.priority = 'medium';
    this.isSubmitting = false;
    this.close.emit();
  }
}
