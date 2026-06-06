import { Component, inject, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
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
