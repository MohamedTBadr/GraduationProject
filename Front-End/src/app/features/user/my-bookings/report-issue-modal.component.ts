import { Component, Input, Output, EventEmitter } from '@angular/core';
import { SupportTicketModalComponent } from '../../../shared/components/support-ticket-modal/support-ticket-modal.component';

/** Thin wrapper kept for my-bookings imports — delegates to shared support ticket modal. */
@Component({
  selector: 'app-report-issue-modal',
  standalone: true,
  imports: [SupportTicketModalComponent],
  template: `
    <app-support-ticket-modal
      [isOpen]="isOpen"
      submitterType="Client"
      [bookingRef]="bookingRef"
      (close)="close.emit()">
    </app-support-ticket-modal>
  `,
})
export class ReportIssueModalComponent {
  @Input() isOpen = false;
  @Input() bookingRef = '';
  @Output() close = new EventEmitter<void>();
}
