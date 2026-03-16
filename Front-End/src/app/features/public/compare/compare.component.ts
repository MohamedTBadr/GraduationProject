import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CompareService } from '../../../shared/services/compare.service';
import { Vendor } from '../../../shared/types/vendor.interface';

@Component({
  selector: 'app-compare',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './compare.component.html',
  styleUrls: ['./compare.component.scss']
})
export class CompareComponent {
  compareService = inject(CompareService);

  getFeatures(): { key: keyof Vendor; label: string }[] {
    return [
      { key: 'category', label: 'Category' },
      { key: 'location', label: 'Location' },
      { key: 'rating', label: 'Rating ()' },
      { key: 'reviews', label: 'Review Count' },
      { key: 'startPrice', label: 'Starting Price (EGP)' },
      { key: 'responseTime', label: 'Response Time' },
      { key: 'deposit', label: 'Required Deposit' },
      { key: 'cancellation', label: 'Cancellation Policy' },
      { key: 'capacity', label: 'Guest Capacity' },
    ];
  }
}
