import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ApiVendor, UpdateVendorRequest } from '../../../shared/types/api.interfaces';
import { VendorService } from '../../../core/services/vendor.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  vendor: ApiVendor | null = null;
  activeTab = 'info';
  loading = false;

  constructor(
    private vendorService: VendorService,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadProfile();
  }

  loadProfile() {
    const user = this.authService.user();
    if (!user) return;

    this.loading = true;
    this.vendorService.getById(user.id).subscribe({
      next: (data) => {
        this.vendor = { ...data };
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load profile', 'error');
        this.loading = false;
        // Optionally map from user to vendor stub if no vendor found
        this.vendor = { id: user.id, name: user.name, email: user.email };
      }
    });
  }

  saveChanges() {
    if (!this.vendor) return;

    const payload: UpdateVendorRequest = {
      name: this.vendor.name,
      businessName: this.vendor.name,
      phone: this.vendor.phone,
      description: this.vendor.about,
      address: {
        street: this.vendor.location || '',
        city: 'Not Specified',
        state: 'Not Specified',
        postalCode: ''
      }
    };

    const vendorId = this.vendor.id;

    this.vendorService.update(vendorId, payload).subscribe({
      next: (res) => {
        this.toastService.show('Profile updated successfully!', 'success');
        this.router.navigate(['/vendor', vendorId]);
      },
      error: () => {
        this.toastService.show('Failed to update profile', 'error');
      }
    });
  }
}
