import { Component, inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { SupportService } from '../../../core/services/support.service';
import {
  buildCreateTicketPayload,
  mapSubjectToCategory,
  TicketSubmitterType,
} from '../../../shared/utils/support-ticket.utils';

const SUPPORT_EMAIL = 'epichub@gmail.com';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.scss']
})
export class ContactComponent implements OnInit {
  private fb = inject(FormBuilder);
  private toastService = inject(ToastService);
  private authService = inject(AuthService);
  private supportService = inject(SupportService);
  private platformId = inject(PLATFORM_ID);

  readonly supportEmail = SUPPORT_EMAIL;
  readonly subjects = [
    'General Inquiry',
    'Booking Issue',
    'Payment Question',
    'Vendor Support'
  ];

  isSubmitting = false;

  contactForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    email: ['', [Validators.required, Validators.email]],
    subject: [this.subjects[0], Validators.required],
    message: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(2000)]]
  });

  ngOnInit(): void {
    const user = this.authService.user();
    if (user) {
      this.contactForm.patchValue({
        name: user.name || '',
        email: user.email || '',
      });
    }
  }

  get isLoggedIn(): boolean {
    const role = this.authService.role();
    return role === 'User' || role === 'Vendor';
  }

  get submitLabel(): string {
    return this.isLoggedIn ? 'Submit Support Ticket' : 'Open in Email App';
  }

  get submitHint(): string {
    return this.isLoggedIn
      ? 'Signed in — your message opens a support ticket in EpicHub.'
      : `Opens your email app with your message pre-filled to ${SUPPORT_EMAIL}.`;
  }

  sendMessage() {
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      this.toastService.show('Please fill in all required fields.', 'error');
      return;
    }

    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const { name, email, subject, message } = this.contactForm.getRawValue();
    if (!name || !email || !subject || !message) return;

    if (this.isLoggedIn) {
      this.submitSupportTicket(name, email, subject, message);
      return;
    }

    this.openMailto(name, email, subject, message);
  }

  private submitSupportTicket(
    name: string,
    email: string,
    subject: string,
    message: string,
  ): void {
    const role = this.authService.role();
    const submitterType: TicketSubmitterType = role === 'Vendor' ? 'Vendor' : 'Client';
    const category = mapSubjectToCategory(subject);

    this.isSubmitting = true;
    const payload = buildCreateTicketPayload({
      submitterType,
      category,
      title: subject,
      description: message,
      priority: 'medium',
      contactName: name,
      contactEmail: email,
    });

    this.supportService.openTicket(payload, category).subscribe({
      next: (ticket) => {
        const suffix = ticket.ticket_id ? ` Reference: ${ticket.ticket_id}` : '';
        this.toastService.show(`Support ticket submitted.${suffix}`, 'success');
        this.contactForm.reset({
          name: '',
          email: '',
          subject: this.subjects[0],
          message: '',
        });
        this.isSubmitting = false;
      },
      error: () => {
        this.toastService.show('Failed to submit support ticket. Try email instead.', 'error');
        this.isSubmitting = false;
      },
    });
  }

  private openMailto(name: string, email: string, subject: string, message: string): void {
    const body = [
      `Name: ${name}`,
      `Reply-to: ${email}`,
      '',
      message
    ].join('\n');

    const mailto = `mailto:${SUPPORT_EMAIL}?subject=${encodeURIComponent(`[EpicHub] ${subject}`)}&body=${encodeURIComponent(body)}`;
    window.location.href = mailto;

    this.toastService.show('Your email app should open — send the message from there.', 'info');
  }
}
