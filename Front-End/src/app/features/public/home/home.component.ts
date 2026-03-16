import { Component } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';
import { MOCK_VENDORS } from '../../../shared/data/mock-vendors.data';
import { Vendor } from '../../../shared/types/vendor.interface';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, VendorCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  featuredVendors: Vendor[] = MOCK_VENDORS.slice(0, 4);

  constructor(private router: Router) { }

  doSearch(query: string) {
    if (query) {
      this.router.navigate(['/explore'], { queryParams: { q: query } });
    }
  }

  filterCat(category: string) {
    this.router.navigate(['/explore'], { queryParams: { type: category } });
  }

  bookPackage() {
    this.router.navigate(['/add-event']);
  }
}
