import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { CompanyInquiryService } from '../../../core/services/company-inquiry.service';
import { EventTypeService } from '../../../core/services/event-type.service';
import { EventType } from '../../../core/models/taxonomy.models';

@Component({
  selector: 'app-corporate',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './corporate.component.html',
  styleUrls: ['./corporate.component.scss']
})
export class CorporateComponent implements OnInit {
  corpForm!: FormGroup;
  eventTypes: EventType[] = [];
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private toastService: ToastService,
    private companyInquiryService: CompanyInquiryService,
    private eventTypeService: EventTypeService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  ngOnInit() {
    this.initForm();
    this.loadEventTypes();
  }

  private initForm() {
    this.corpForm = this.fb.group({
      companyName: ['', Validators.required],
      contactPerson: ['', Validators.required],
      phoneNumber: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      eventTypeId: ['', [Validators.required]],
      expectedDate: ['', Validators.required],
      estimatedAttendees: [null, [Validators.min(1)]],
      approximateBudget: [null, [Validators.min(0)]],
      additionalRequirements: ['']
    });
  }

  private loadEventTypes() {
    this.eventTypeService.getAll().subscribe({
      next: (data) => this.eventTypes = data,
      error: (err) => {
        console.error('Failed to load event types', err);
        this.toastService.show('Failed to load event types.', 'error');
      }
    });
  }

  scrollToForm() {
    if (isPlatformBrowser(this.platformId)) {
      document.getElementById('corp-quote')?.scrollIntoView({ behavior: 'smooth' });
    }
  }

  submitCorpForm() {
    if (this.corpForm.invalid) {
      this.toastService.show('Please fill in all required fields correctly.', 'error');
      this.corpForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValue = this.corpForm.value;

    this.companyInquiryService.submitInquiry(formValue).subscribe({
      next: (res) => {
        this.toastService.show(res.message || 'Corporate request submitted successfully.', 'success');
        this.corpForm.reset();
        this.isSubmitting = false;
      },
      error: (err) => {
        this.toastService.show(err.error?.message || 'Failed to submit request. Please try again.', 'error');
        this.isSubmitting = false;
      }
    });
  }
}
