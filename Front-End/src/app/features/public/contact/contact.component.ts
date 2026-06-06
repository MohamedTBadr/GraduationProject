import { Component, inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastService } from '../../../shared/components/toast/toast.service';

const SUPPORT_EMAIL = 'epichub@gmail.com';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.scss']
})
export class ContactComponent {
  private fb = inject(FormBuilder);
  private toastService = inject(ToastService);
  private platformId = inject(PLATFORM_ID);

  readonly supportEmail = SUPPORT_EMAIL;
  readonly subjects = [
    'General Inquiry',
    'Booking Issue',
    'Payment Question',
    'Vendor Support'
  ];

  contactForm = this.fb.group({
    name: ['', [Validators.required, Validators.maxLength(120)]],
    email: ['', [Validators.required, Validators.email]],
    subject: [this.subjects[0], Validators.required],
    message: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(2000)]]
  });

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
