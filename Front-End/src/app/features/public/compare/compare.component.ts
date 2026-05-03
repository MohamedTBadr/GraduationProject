import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CompareService } from '../../../shared/services/compare.service';
import { ApiVendor } from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-compare',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './compare.component.html',
  styleUrls: ['./compare.component.scss']
})
export class CompareComponent {
  compareService = inject(CompareService);

  getFeatures(): { key: keyof ApiVendor; label: string }[] {
    return [
      { key: 'vendorTypeName', label: 'Vendor Type' },
      { key: 'location', label: 'Location' },
      { key: 'rating', label: 'Rating ()' },
      { key: 'phone', label: 'Phone Number' },
      { key: 'email', label: 'Contact Email' },
    ];
  }
}
