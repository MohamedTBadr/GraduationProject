import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SupportService } from '../../../../core/services/support.service';
import { SupportTicket } from '../../../../shared/types/api.interfaces';
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
    this.error = null;
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
      message: this.replyMessage.trim(),
      sendEmail: this.sendEmail,
      sendSms: this.sendSms,
    }).subscribe({
      next: () => {
        this.replyMessage = '';
        this.toastService.show('Reply sent.', 'success');
        this.loadTicket();
      },
      error: () => this.toastService.show('Failed to send reply.', 'error')
    });
  }

  onAssign(): void {
    if (!this.agentId.trim()) return;
    this.supportService.assign(this.ticketId, {
      agentId: this.agentId.trim(),
      note: this.assignNote.trim() || undefined,
    }).subscribe({
      next: () => {
        this.agentId = '';
        this.assignNote = '';
        this.toastService.show('Ticket assigned.', 'success');
        this.loadTicket();
      },
      error: () => this.toastService.show('Failed to assign ticket. Check the agent ID and try again.', 'error')
    });
  }

  onResolve(): void {
    if (!this.resolutionNote.trim()) return;
    this.supportService.resolve(this.ticketId, {
      resolutionNote: this.resolutionNote.trim(),
    }).subscribe({
      next: () => {
        this.resolutionNote = '';
        this.toastService.show('Ticket resolved.', 'success');
        this.loadTicket();
      },
      error: () => this.toastService.show('Failed to resolve ticket.', 'error')
    });
  }

  onEscalate(): void {
    if (!this.escalateReason.trim()) return;
    this.supportService.adminEscalate(this.ticketId, {
      reason: this.escalateReason.trim(),
      escalateTo: this.escalateTo,
      notifyFinance: this.notifyFinance,
    }).subscribe({
      next: () => {
        this.toastService.show(`Ticket escalated to ${this.escalateTo.replace('_', ' ')}.`, 'success');
        this.escalateReason = '';
        this.loadTicket();
      },
      error: () => {
        this.toastService.show('Failed to escalate ticket. Please try again.', 'error');
      }
    });
  }
}
