import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SupportService } from '../../../../core/services/support.service';
import { SupportTicket, TicketReply } from '../../../../shared/types/api.interfaces';
import { ToastService } from '../../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-ticket-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './ticket-detail.component.html',
  styleUrls: ['./ticket-detail.component.scss']
})
export class TicketDetailComponent implements OnInit {
  ticketId: string = '';
  ticket: SupportTicket | null = null;
  loading: boolean = true;
  error: string | null = null;

  replyMessage: string = '';
  sendEmail: boolean = true;
  sendSms: boolean = false;

  agentId: string = '';
  assignNote: string = '';

  resolutionNote: string = '';

  escalateReason: string = '';
  escalateTo: 'senior_management' | 'legal_team' | 'cto' = 'senior_management';
  notifyFinance: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private supportService: SupportService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.ticketId = this.route.snapshot.paramMap.get('id') || '';
    if (this.ticketId) {
      this.loadTicket();
    }
  }

  loadTicket(): void {
    this.loading = true;
    this.supportService.getTicket(this.ticketId).subscribe({
      next: (data) => {
        this.ticket = data;
        this.loading = false;
      },
      error: () => {
        this.error = 'Failed to load ticket details.';
        this.loading = false;
        this.toastService.show('Failed to load ticket details.', 'error');
      }
    });
  }

  onReply(): void {
    if (!this.replyMessage.trim()) return;
    this.supportService.reply(this.ticketId, {
      message: this.replyMessage,
      send_email: this.sendEmail,
      send_sms: this.sendSms
    }).subscribe({
      next: (reply) => {
        if (this.ticket && !this.ticket.replies) this.ticket.replies = [];
        this.ticket?.replies?.push(reply);
        this.replyMessage = '';
        this.toastService.show('Reply sent.', 'success');
      },
      error: () => this.toastService.show('Failed to send reply.', 'error')
    });
  }

  onAssign(): void {
    if (!this.agentId) return;
    this.supportService.assign(this.ticketId, {
      agent_id: this.agentId,
      note: this.assignNote
    }).subscribe({
      next: (res) => {
        if (this.ticket) {
          this.ticket.status = res.status;
          this.ticket.assigned_to = res.assigned_to;
        }
        this.agentId = '';
        this.assignNote = '';
        this.toastService.show('Ticket assigned.', 'success');
      },
      error: () => this.toastService.show('Failed to assign ticket.', 'error')
    });
  }

  onResolve(): void {
    if (!this.resolutionNote.trim()) return;
    this.supportService.resolve(this.ticketId, {
      resolution_note: this.resolutionNote
    }).subscribe({
      next: (res) => {
        if (this.ticket) {
          this.ticket.status = 'resolved';
          this.ticket.resolved_at = res.resolved_at;
        }
        this.resolutionNote = '';
        this.toastService.show('Ticket resolved.', 'success');
      },
      error: () => this.toastService.show('Failed to resolve ticket.', 'error')
    });
  }

  onEscalate(): void {
    if (!this.escalateReason.trim()) return;
    this.supportService.escalate(this.ticketId, {
      reason: this.escalateReason,
      escalate_to: this.escalateTo,
      notify_finance: this.notifyFinance
    }).subscribe({
      next: () => {
        this.toastService.show(`Ticket escalated to ${this.escalateTo.replace('_', ' ')}.`, 'success');
        this.escalateReason = '';
      },
      error: (err) => {
        const status = err?.status;
        if (status === 403) {
          this.toastService.show(
            'Escalation is not permitted for this account. Backend may need to allow Admin on this endpoint.',
            'error'
          );
        } else {
          this.toastService.show('Failed to escalate ticket.', 'error');
        }
      }
    });
  }
}
