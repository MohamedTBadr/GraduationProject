import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { VendorService } from '../../../../core/services/vendor.service';
import { VendorTypeService } from '../../../../core/services/vendor-type.service';
import { VendorType } from '../../../../core/models/taxonomy.models';
import { ToastService } from '../../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-vendor-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './vendor-create.component.html',
  styleUrls: ['./vendor-create.component.scss']
})
export class VendorCreateComponent implements OnInit {
  form: FormGroup;
  vendorTypes: VendorType[] = [];
  loading = false;
  submitting = false;

  constructor(
    private fb: FormBuilder,
    private vendorService: VendorService,
    private vendorTypeService: VendorTypeService,
    private toastService: ToastService,
    private router: Router
  ) {
    this.form = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['Vendor123!', Validators.required], // Default password for manual onboarding
      phone: ['', Validators.required],
      name: ['', Validators.required], // UserName
      businessName: ['', Validators.required],
      ownerName: ['', Validators.required],
      vendorTypeId: ['', Validators.required],
      yearsInBusiness: [0, [Validators.required, Validators.min(0)]],
      description: ['', Validators.required],
      portfolioLink: [''],
      street: ['', Validators.required],
      city: ['', Validators.required],
      state: ['', Validators.required],
      postalCode: ['']
    });
  }

  ngOnInit(): void {
    this.loadVendorTypes();
  }

  loadVendorTypes() {
    this.loading = true;
    this.vendorTypeService.getAll().subscribe({
      next: (data) => {
        this.vendorTypes = data;
        this.loading = false;
      }
    });
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    const val = this.form.value;
    const formData = new FormData();

    // Append standard fields
    Object.keys(val).forEach(key => {
      if (key !== 'street' && key !== 'city' && key !== 'state' && key !== 'postalCode') {
        formData.append(key, val[key]);
      }
    });

    // Append Address fields
    formData.append('Address.Street', val.street || '');
    formData.append('Address.City', val.city || '');
    formData.append('Address.State', val.state || '');
    formData.append('Address.PostalCode', val.postalCode || '');

    this.vendorService.create(formData).subscribe({
      next: (res) => {
        this.toastService.show('Vendor created successfully!', 'success');
        this.router.navigate(['/admin/vendors']);
      },
      error: (err) => {
        this.toastService.show('Error creating vendor. Please try again.', 'error');
        this.submitting = false;
      }
    });
  }
}
