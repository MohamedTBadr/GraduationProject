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
  appendServiceAreasToFormData,
  normalizeAddressFields
} from '../../../shared/utils/location.utils';

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
  selectedDocument: File | null = null;
  selectedProfilePicture: File | null = null;
  profilePreviewUrl: string | null = null;
  readonly cityOptions = EGYPT_CITY_OPTIONS;
  readonly governorateOptions = EGYPT_GOVERNORATE_OPTIONS;

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
      name: ['', [Validators.required, Validators.minLength(3)]], // This maps to Username
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

  onDocumentSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedDocument = file;
    }
  }

  onProfilePictureSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      this.selectedProfilePicture = file;
      
      // Create preview URL
      const reader = new FileReader();
      reader.onload = () => {
        this.profilePreviewUrl = reader.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit() {
    if (this.vendorForm.invalid) {
      this.markFormGroupTouched(this.vendorForm);
      this.toastService.show('Please fill in all required fields correctly', 'error');
      return;
    }

    this.isSubmitting = true;
    
    // Create FormData for file upload support
    const formData = new FormData();
    const formValue = this.vendorForm.getRawValue();

    // Append standard fields
    Object.keys(formValue).forEach(key => {
      if (key !== 'address' && key !== 'serviceAreas') {
        formData.append(key, formValue[key]);
      }
    });

    // Append Address fields (canonical city/governorate)
    if (formValue.address) {
      const { city, state } = normalizeAddressFields(
        formValue.address.city || '',
        formValue.address.state || ''
      );
      formData.append('Address.Street', formValue.address.street || '');
      formData.append('Address.City', city);
      formData.append('Address.State', state);
      formData.append('Address.PostalCode', formValue.address.postalCode || '');
      appendServiceAreasToFormData(formData, [
        addressToServiceArea({ city, state, street: formValue.address.street })
      ]);
    }

    // Append Files
    if (this.selectedDocument) {
      formData.append('Document', this.selectedDocument);
    }
    if (this.selectedProfilePicture) {
      formData.append('ProfilePicture', this.selectedProfilePicture);
    }

    this.vendorService.create(formData).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.isSuccess = true;
        this.toastService.show('Application submitted successfully!', 'success');
      },
      error: (err) => {
        this.isSubmitting = false;
        const msg = err.error?.message || 'Failed to submit application. Please try again.';
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
