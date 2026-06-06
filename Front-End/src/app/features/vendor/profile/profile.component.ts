import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { ApiVendor, UpdateVendorRequest } from '../../../shared/types/api.interfaces';
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

  readonly cityOptions = EGYPT_CITY_OPTIONS;
  readonly governorateOptions = EGYPT_GOVERNORATE_OPTIONS;

  addressStreet = '';
  addressCity = '';
  addressState = '';
  coverageCities: string[] = [];

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

  saveChanges() {
    if (!this.vendor) return;

    const { city, state } = normalizeAddressFields(this.addressCity, this.addressState);
    const serviceAreas = citiesToServiceAreas(
      this.coverageCities.length ? this.coverageCities : [city]
    );

    const payload: UpdateVendorRequest = {
      name: this.vendor.name,
      businessName: this.vendor.name,
      phone: this.vendor.phone,
      description: this.vendor.about,
      address: {
        street: this.addressStreet.trim(),
        city,
        state,
        postalCode: ''
      },
      serviceAreas
    };

    const vendorId = this.vendor.id;

    this.vendorService.update(vendorId, payload).subscribe({
      next: () => {
        this.toastService.show('Profile updated successfully!', 'success');
        this.router.navigate(['/vendor', vendorId]);
      },
      error: () => {
        this.toastService.show('Failed to update profile', 'error');
      }
    });
  }
}
