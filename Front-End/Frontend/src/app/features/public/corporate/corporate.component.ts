import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-corporate',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './corporate.component.html',
  styleUrls: ['./corporate.component.scss']
})
export class CorporateComponent {

  constructor(
    private toastService: ToastService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  scrollToForm() {
    if (isPlatformBrowser(this.platformId)) {
      document.getElementById('corp-quote')?.scrollIntoView({ behavior: 'smooth' });
    }
  }

  submitCorpForm() {
    // Basic mock submission
    this.toastService.show('Corporate request submitted successfully! Our team will contact you within 4 hours.', 'success');
  }
}
