import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ApiVendor } from '../../../shared/types/api.interfaces';
import { VendorService } from '../../../core/services/vendor.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import {
  EGYPT_CITY_OPTIONS,
  EGYPT_GOVERNORATE_OPTIONS,
  getLocationByCity
} from '../../../shared/constants/egypt-locations';
import {
  citiesToServiceAreas,
  normalizeAddressFields
} from '../../../shared/utils/location.utils';
import { appendVendorUpdateFormData } from '../../../shared/utils/vendor-form.utils';

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
  saving = false;

  readonly cityOptions = EGYPT_CITY_OPTIONS;
  readonly governorateOptions = EGYPT_GOVERNORATE_OPTIONS;
  readonly maxProfileMb = 5;

  addressStreet = '';
  addressCity = '';
  addressState = '';
  coverageCities: string[] = [];
  selectedProfilePicture: File | null = null;
  profilePreviewUrl: string | null = null;

  constructor(
    private vendorService: VendorService,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadProfile();
  }

  get displayProfilePicture(): string | null {
    return this.profilePreviewUrl || this.vendor?.profilePictureUrl || null;
  }

  loadProfile() {
    const user = this.authService.user();
    if (!user) return;

    this.loading = true;
    this.selectedProfilePicture = null;
    this.profilePreviewUrl = null;
    this.vendorService.getById(user.id).subscribe({
      next: (data) => {
        this.vendor = { ...data };
        this.hydrateAddressFromVendor(data);
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load profile', 'error');
        this.loading = false;
        const user = this.authService.user();
        if (user) {
          this.vendor = { id: user.id, name: user.name, email: user.email };
        }
      }
    });
  }

  private hydrateAddressFromVendor(vendor: ApiVendor): void {
    const addr = vendor.address;
    if (addr && typeof addr === 'object') {
      this.addressStreet = addr.street || '';
      this.addressCity = addr.city || '';
      this.addressState = addr.state || '';
    } else if (typeof vendor.location === 'string' && vendor.location.trim()) {
      this.addressStreet = vendor.location;
    }

    if (vendor.serviceAreas?.length) {
      this.coverageCities = [...new Set(vendor.serviceAreas.map((a) => a.city).filter(Boolean))];
    } else if (this.addressCity) {
      this.coverageCities = [this.addressCity];
    } else {
      this.coverageCities = [];
    }
  }

  onCityChange(city: string): void {
    const loc = getLocationByCity(city);
    if (loc) {
      this.addressState = loc.governorate;
      if (!this.coverageCities.includes(loc.city)) {
        this.coverageCities = [loc.city, ...this.coverageCities];
      }
    }
  }

  toggleCoverageCity(city: string): void {
    if (this.coverageCities.includes(city)) {
      this.coverageCities = this.coverageCities.filter((c) => c !== city);
    } else {
      this.coverageCities = [...this.coverageCities, city];
    }
  }

  isCoverageSelected(city: string): boolean {
    return this.coverageCities.includes(city);
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

  saveChanges() {
    if (!this.vendor || this.saving) return;

    const { city, state } = normalizeAddressFields(this.addressCity, this.addressState);
    const serviceAreas = citiesToServiceAreas(
      this.coverageCities.length ? this.coverageCities : [city]
    );

    const formData = new FormData();
    appendVendorUpdateFormData(formData, {
      name: this.vendor.name,
      businessName: this.vendor.name,
      ownerName: this.vendor.name,
      phone: this.vendor.phone,
      description: this.vendor.about,
      address: {
        street: this.addressStreet.trim(),
        city,
        state,
        postalCode: ''
      },
      serviceAreas,
      profilePicture: this.selectedProfilePicture
    });

    const vendorId = this.vendor.id;
    this.saving = true;

    this.vendorService.update(vendorId, formData).subscribe({
      next: (updated) => {
        this.saving = false;
        this.vendor = { ...this.vendor!, ...updated };
        this.selectedProfilePicture = null;
        this.profilePreviewUrl = null;
        this.toastService.show('Profile updated successfully!', 'success');
        this.router.navigate(['/vendor', vendorId]);
      },
      error: () => {
        this.saving = false;
        this.toastService.show('Failed to update profile', 'error');
      }
    });
  }
}
