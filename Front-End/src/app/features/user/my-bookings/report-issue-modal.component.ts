import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SupportService } from '../../../core/services/support.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-report-issue-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="vd-modal-overlay" [class.open]="isOpen">
      <div class="vd-modal">
        <button class="vd-modal-close" (click)="closeModal()">✕</button>
        <div class="vd-modal-title">Report an Issue</div>
        <div class="vd-modal-sub mb-[20px]">
          Tell us what went wrong. We will open a support ticket to investigate.
        </div>

        <div class="vd-form-group mb-[12px]">
          <label class="mb-[6px]">Booking Reference</label>
          <input type="text" class="w-[100%] rounded-[8px] p-[10px] text-[0.9rem] border border-[var(--lgray)] bg-gray-100 cursor-not-allowed" 
                 [value]="bookingRef" readonly>
        </div>

        <div class="vd-form-group mb-[12px]">
          <label class="mb-[6px]">Issue Title <span class="text-[#e11d48]">*</span></label>
          <input type="text" class="w-[100%] rounded-[8px] p-[10px] text-[0.9rem] border border-[var(--lgray)]" 
                 placeholder="e.g. Catering vendor didn't show up on time" 
                 [(ngModel)]="title" required>
        </div>

        <div class="grid grid-cols-2 gap-[12px] mb-[12px]">
          <div class="vd-form-group">
            <label class="mb-[6px]">Issue Type <span class="text-[#e11d48]">*</span></label>
            <select class="w-[100%] rounded-[8px] p-[10px] text-[0.9rem] border border-[var(--lgray)] bg-white" 
                    [(ngModel)]="ticketType">
              <option value="Booking">Booking Issue</option>
              <option value="Payment">Payment / Refund</option>
              <option value="Technical">Technical Glitch</option>
              <option value="General">General Inquiry</option>
            </select>
          </div>

          <div class="vd-form-group">
            <label class="mb-[6px]">Priority <span class="text-[#e11d48]">*</span></label>
            <select class="w-[100%] rounded-[8px] p-[10px] text-[0.9rem] border border-[var(--lgray)] bg-white" 
                    [(ngModel)]="priority">
              <option value="low">Low</option>
              <option value="medium">Medium</option>
              <option value="high">High</option>
              <option value="critical">Critical</option>
            </select>
          </div>
        </div>

        <div class="vd-form-group mb-[20px]">
          <label class="mb-[6px]">Description <span class="text-[#e11d48]">*</span></label>
          <textarea class="w-[100%] rounded-[8px] p-[12px] text-[0.9rem] border border-[var(--lgray)]" 
                    rows="4" 
                    placeholder="Provide details about the issue so we can help you faster..." 
                    [(ngModel)]="description" required></textarea>
        </div>

        <div class="vd-submit-bar pt-[20px] mt-[24px] justify-center gap-[14px] [border-top:1px_solid_var(--lgray)]">
          <button class="vd-btn vd-btn-ghost py-[10px] px-[24px] rounded-[50px]" (click)="closeModal()">Cancel</button>
          <button class="vd-btn py-[10px] px-[24px] rounded-[50px] text-white" 
                  style="background-color: #ef4444;" 
                  (click)="submitTicket()" 
                  [disabled]="isSubmitting || !title.trim() || !description.trim()">
            {{ isSubmitting ? 'Submitting...' : 'Submit Ticket' }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .vd-modal-overlay {
      position: fixed; top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(0,0,0,0.5);
      display: flex; align-items: center; justify-content: center;
      z-index: 1000;
      opacity: 0; pointer-events: none; transition: 0.2s;
    }
    .vd-modal-overlay.open {
      opacity: 1; pointer-events: auto;
    }
    .vd-modal {
      background: white; border-radius: 12px;
      padding: 24px; width: 90%; max-width: 480px;
      position: relative;
    }
    .vd-modal-close {
      position: absolute; top: 16px; right: 16px;
      background: none; border: none; font-size: 1.2rem; cursor: pointer;
    }
    .vd-modal-title { font-size: 1.25rem; font-weight: bold; color: var(--navy); }
    .vd-modal-sub { font-size: 0.9rem; color: var(--gray); }
    .vd-form-group label { display: block; font-weight: 600; color: var(--navy); }
    .vd-btn { cursor: pointer; font-weight: 600; border: none; }
    .vd-btn-ghost { background: transparent; color: var(--navy); }
    .vd-submit-bar { display: flex; }
  `]
})
export class ReportIssueModalComponent {
  @Input() isOpen = false;
  @Input() bookingRef = '';
  @Output() close = new EventEmitter<void>();

  title = '';
  description = '';
  ticketType = 'Booking';
  priority = 'medium';
  isSubmitting = false;

  constructor(
    private supportService: SupportService,
    private toastService: ToastService
  ) {}

  submitTicket() {
    if (!this.title.trim() || !this.description.trim()) return;

    this.isSubmitting = true;
    this.supportService.openTicket({
      title: this.title.trim(),
      description: this.description.trim(),
      type: this.ticketType,
      priority: this.priority,
      bookingRef: this.bookingRef
    }).subscribe({
      next: () => {
        this.toastService.show('Support ticket opened successfully!', 'success');
        this.isSubmitting = false;
        this.closeModal();
      },
      error: (err: any) => {
        console.error('Error opening support ticket', err);
        this.toastService.show('Failed to open support ticket.', 'error');
        this.isSubmitting = false;
      }
    });
  }

  closeModal() {
    this.title = '';
    this.description = '';
    this.ticketType = 'Booking';
    this.priority = 'medium';
    this.isSubmitting = false;
    this.close.emit();
  }
}
