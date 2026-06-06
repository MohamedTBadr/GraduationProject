import { Component, inject, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CompareService } from '../../../shared/services/compare.service';
import { ApiVendor, ApiProduct } from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-compare',
  standalone: true,
  imports: [CommonModule, RouterLink, DecimalPipe],
  templateUrl: './compare.component.html',
  styleUrls: ['./compare.component.scss']
})
export class CompareComponent implements OnInit {
  compareService = inject(CompareService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  activeTab: 'vendors' | 'services' = 'vendors';

  ngOnInit() {
    this.route.queryParamMap.subscribe(params => {
      const tab = params.get('tab');
      if (tab === 'services') {
        this.activeTab = 'services';
      } else if (tab === 'vendors') {
        this.activeTab = 'vendors';
      } else if (this.compareService.serviceCompareCount() >= 2) {
        this.activeTab = 'services';
      } else {
        this.activeTab = 'vendors';
      }
    });
  }

  switchTab(tab: 'vendors' | 'services') {
    this.activeTab = tab;
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  formatVendorValue(vendor: ApiVendor, key: keyof ApiVendor): string {
    const value = vendor[key];
    if (value === null || value === undefined || value === '') {
      return '—';
    }
    return String(value);
  }

  formatServiceValue(service: ApiProduct, key: keyof ApiProduct): string {
    const value = service[key];
    if (value === null || value === undefined || value === '') {
      return '—';
    }
    return String(value);
  }

  getVendorFeatures(): { key: keyof ApiVendor; label: string }[] {
    return [
      { key: 'vendorTypeName', label: 'Vendor Type' },
      { key: 'location', label: 'Location' },
      { key: 'rating', label: 'Rating' },
      { key: 'phone', label: 'Phone Number' },
      { key: 'email', label: 'Contact Email' },
    ];
  }

  getServiceFeatures(): { key: keyof ApiProduct; label: string }[] {
    return [
      { key: 'vendorName', label: 'Vendor' },
      { key: 'serviceTypeName', label: 'Service Type' },
      { key: 'price', label: 'Price (EGP)' },
      { key: 'duration', label: 'Setup Duration' },
      { key: 'leadTime', label: 'Lead Time' },
      { key: 'rating', label: 'Rating' },
    ];
  }
}
