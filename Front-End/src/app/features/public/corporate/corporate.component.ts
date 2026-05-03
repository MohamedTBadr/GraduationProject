import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { CompanyInquiryService } from '../../../core/services/company-inquiry.service';
import { VendorTypeService } from '../../../core/services/vendor-type.service';
import { VendorType } from '../../../core/models/taxonomy.models';

@Component({
  selector: 'app-corporate',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './corporate.component.html',
  styleUrls: ['./corporate.component.scss']
})
export class CorporateComponent implements OnInit {
  corpForm!: FormGroup;
  vendorTypes: VendorType[] = [];
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private toastService: ToastService,
    private companyInquiryService: CompanyInquiryService,
    private vendorTypeService: VendorTypeService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  ngOnInit() {
    this.initForm();
    this.loadVendorTypes();
  }

  private initForm() {
    this.corpForm = this.fb.group({
      companyName: ['', Validators.required],
      contactPerson: ['', Validators.required],
      phoneNumber: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      vendorTypeId: ['', [Validators.required]],
      expectedDate: ['', Validators.required],
      estimatedAttendees: [null, [Validators.min(1)]],
      approximateBudget: [null, [Validators.min(0)]],
      additionalRequirements: ['']
    });
  }

  private loadVendorTypes() {
    this.vendorTypeService.getAll().subscribe({
      next: (data) => this.vendorTypes = data,
      error: (err) => {
        console.error('Failed to load vendor types', err);
        this.toastService.show('Failed to load vendor types.', 'error');
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
