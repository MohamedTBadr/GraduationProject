import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SupportService } from '../../../../core/services/support.service';
import { SupportTicket, TicketReply } from '../../../../shared/types/api.interfaces';

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

  // Form states
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
    private supportService: SupportService
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
      error: (err) => {
        this.error = 'Failed to load ticket details.';
        this.loading = false;
        console.error(err);
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
      },
      error: (err) => alert('Failed to send reply.')
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
      },
      error: (err) => alert('Failed to assign ticket.')
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
      },
      error: (err) => alert('Failed to resolve ticket.')
    });
  }

  onEscalate(): void {
    if (!this.escalateReason.trim()) return;
    this.supportService.escalate(this.ticketId, {
      reason: this.escalateReason,
      escalate_to: this.escalateTo,
      notify_finance: this.notifyFinance
    }).subscribe({
      next: (res) => {
        alert(`Ticket escalated to ${this.escalateTo}`);
        this.escalateReason = '';
      },
      error: (err) => alert('Failed to escalate ticket.')
    });
  }
}
