import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { VendorService } from '../../../core/services/vendor.service';
import { VendorTypeService } from '../../../core/services/vendor-type.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { VendorType } from '../../../core/models/taxonomy.models';
import {
  EGYPT_CITY_OPTIONS,
  EGYPT_GOVERNORATE_OPTIONS,
  getLocationByCity
} from '../../../shared/constants/egypt-locations';
import {
  addressToServiceArea,
  normalizeAddressFields
} from '../../../shared/utils/location.utils';
import { appendVendorCreateFormData } from '../../../shared/utils/vendor-form.utils';

@Component({
  selector: 'app-vendor-join',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './vendor-join.component.html',
  styleUrls: ['./vendor-join.component.scss']
})
export class VendorJoinComponent implements OnInit {
  vendorForm!: FormGroup;
  vendorTypes: VendorType[] = [];
  isSubmitting = false;
  isSuccess = false;
  currentStep = 1;
  totalSteps = 3;
  selectedDocuments: File[] = [];
  selectedProfilePicture: File | null = null;
  profilePreviewUrl: string | null = null;
  readonly cityOptions = EGYPT_CITY_OPTIONS;
  readonly governorateOptions = EGYPT_GOVERNORATE_OPTIONS;
  readonly maxDocuments = 5;
  readonly maxProfileMb = 5;
  readonly maxDocumentMb = 10;
  readonly acceptedDocTypes = '.pdf,.doc,.docx,.jpg,.jpeg,.png,.webp';

  constructor(
    private fb: FormBuilder,
    private vendorService: VendorService,
    private vendorTypeService: VendorTypeService,
    private toastService: ToastService,
    private router: Router
  ) {
    this.initForm();
  }

  ngOnInit() {
    this.loadVendorTypes();
  }

  private initForm() {
    this.vendorForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^\+?[0-9]{10,14}$/)]],
      name: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      businessName: ['', [Validators.required]],
      ownerName: ['', [Validators.required]],
      vendorTypeId: ['', [Validators.required]],
      yearsInBusiness: [0, [Validators.required, Validators.min(0)]],
      description: ['', [Validators.required, Validators.minLength(20)]],
      portfolioLink: [''],
      address: this.fb.group({
        street: ['', [Validators.required]],
        city: ['', [Validators.required]],
        state: ['', [Validators.required]],
        postalCode: ['']
      })
    });
  }

  private loadVendorTypes() {
    this.vendorTypeService.getAll().subscribe({
      next: (types) => this.vendorTypes = types,
      error: () => this.toastService.show('Failed to load vendor types', 'error')
    });
  }

  onDocumentsSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const files = input.files ? Array.from(input.files) : [];
    if (!files.length) return;

    const valid: File[] = [];
    for (const file of files) {
      if (file.size > this.maxDocumentMb * 1024 * 1024) {
        this.toastService.show(`${file.name} exceeds ${this.maxDocumentMb}MB and was skipped`, 'error');
        continue;
      }
      valid.push(file);
    }

    const slotsLeft = this.maxDocuments - this.selectedDocuments.length;
    if (slotsLeft <= 0) {
      this.toastService.show(`You can upload up to ${this.maxDocuments} documents`, 'error');
      input.value = '';
      return;
    }

    const toAdd = valid.slice(0, slotsLeft);
    this.selectedDocuments = [...this.selectedDocuments, ...toAdd];

    if (valid.length > toAdd.length) {
      this.toastService.show(`Only ${toAdd.length} file(s) added (max ${this.maxDocuments} total)`, 'info');
    }

    input.value = '';
  }

  removeDocument(index: number) {
    this.selectedDocuments.splice(index, 1);
  }

  onProfilePictureSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (!file.type.startsWith('image/') && !/\.(jpe?g|png|webp)$/i.test(file.name)) {
      this.toastService.show('Profile picture must be JPG, PNG, or WebP', 'error');
      input.value = '';
      return;
    }
    if (file.size > this.maxProfileMb * 1024 * 1024) {
      this.toastService.show(`Profile picture must be under ${this.maxProfileMb}MB`, 'error');
      input.value = '';
      return;
    }
    this.selectedProfilePicture = file;
    const reader = new FileReader();
    reader.onload = () => {
      this.profilePreviewUrl = reader.result as string;
    };
    reader.readAsDataURL(file);
  }

  onSubmit() {
    if (this.vendorForm.invalid) {
      this.markFormGroupTouched(this.vendorForm);
      this.toastService.show('Please fill in all required fields correctly', 'error');
      return;
    }

    this.isSubmitting = true;
    const formValue = this.vendorForm.getRawValue();
    const { city, state } = normalizeAddressFields(
      formValue.address?.city || '',
      formValue.address?.state || ''
    );

    const formData = new FormData();
    appendVendorCreateFormData(formData, {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      password: formValue.password,
      phone: formValue.phone,
      name: formValue.name,
      businessName: formValue.businessName,
      ownerName: formValue.ownerName,
      vendorTypeId: formValue.vendorTypeId,
      yearsInBusiness: formValue.yearsInBusiness,
      description: formValue.description,
      address: {
        street: formValue.address?.street,
        city,
        state,
        postalCode: formValue.address?.postalCode
      },
      serviceAreas: [
        addressToServiceArea({ city, state, street: formValue.address?.street })
      ],
      profilePicture: this.selectedProfilePicture,
      documents: this.selectedDocuments
    });

    this.vendorService.create(formData).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.isSuccess = true;
        this.toastService.show('Application submitted successfully!', 'success');
      },
      error: (err) => {
        this.isSubmitting = false;
        const msg = err.error?.message || err.error?.detail || 'Failed to submit application. Please try again.';
        this.toastService.show(msg, 'error');
      }
    });
  }

  private markFormGroupTouched(formGroup: FormGroup) {
    Object.values(formGroup.controls).forEach(control => {
      control.markAsTouched();
      if ((control as any).controls) {
        this.markFormGroupTouched(control as FormGroup);
      }
    });
  }

  goToHome() {
    this.router.navigate(['/']);
  }

  nextStep() {
    if (this.canGoNext()) {
      this.currentStep++;
      this.scrollToForm();
    } else {
      this.markStepFieldsTouched();
      this.toastService.show('Please fill in all required fields for this step', 'error');
    }
  }

  prevStep() {
    if (this.currentStep > 1) {
      this.currentStep--;
      this.scrollToForm();
    }
  }

  goToStep(step: number) {
    if (step < this.currentStep || this.canGoNext()) {
      this.currentStep = step;
      this.scrollToForm();
    }
  }

  private canGoNext(): boolean {
    const controls = this.vendorForm.controls;
    if (this.currentStep === 1) {
      return controls['name'].valid &&
             controls['password'].valid &&
             controls['firstName'].valid &&
             controls['lastName'].valid &&
             controls['email'].valid &&
             controls['phone'].valid;
    }
    if (this.currentStep === 2) {
      return controls['businessName'].valid &&
             controls['ownerName'].valid &&
             controls['vendorTypeId'].valid &&
             controls['yearsInBusiness'].valid &&
             controls['description'].valid;
    }
    return true;
  }

  private markStepFieldsTouched() {
    const fieldsByStep: { [key: number]: string[] } = {
      1: ['name', 'password', 'firstName', 'lastName', 'email', 'phone'],
      2: ['businessName', 'ownerName', 'vendorTypeId', 'yearsInBusiness', 'description']
    };

    const currentFields = fieldsByStep[this.currentStep];
    if (currentFields) {
      currentFields.forEach(field => {
        this.vendorForm.get(field)?.markAsTouched();
      });
    }
  }

  onCityChange(city: string): void {
    const loc = getLocationByCity(city);
    if (loc) {
      this.vendorForm.get('address.state')?.setValue(loc.governorate);
    }
  }

  scrollToForm() {
    const formElement = document.getElementById('vendor-form');
    if (formElement) {
      formElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
  }
}
