import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';
import { ApiVendor } from '../../../shared/types/api.interfaces';
import { VendorService } from '../../../core/services/vendor.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, VendorCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  featuredVendors: ApiVendor[] = [];
  loading = false;

  constructor(private router: Router, private vendorService: VendorService) { }

  ngOnInit() {
    this.loading = true;
    this.vendorService.getAll().subscribe({
      next: (data) => {
        // Take top 4 rated or approved vendors
        this.featuredVendors = data.filter(v => v.status === 'active' || v.isApproved)
          .sort((a, b) => (b.rating || 0) - (a.rating || 0))
          .slice(0, 4);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

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
