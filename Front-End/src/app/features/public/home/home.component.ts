import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';
import { ApiVendor } from '../../../shared/types/api.interfaces';
import { VendorService } from '../../../core/services/vendor.service';
import { EventTypeService } from '../../../core/services/event-type.service';
import { EventType } from '../../../core/models/taxonomy.models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, VendorCardComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  featuredVendors: ApiVendor[] = [];
  eventTypes: EventType[] = [];
  loading = false;

  constructor(
    private router: Router,
    private vendorService: VendorService,
    private eventTypeService: EventTypeService
  ) { }

  ngOnInit() {
    this.loading = true;
    this.eventTypeService.getAll().subscribe({
      next: (types) => { this.eventTypes = Array.isArray(types) ? types : []; }
    });
    this.vendorService.getAll({ pageSize: 20, pageIndex: 1, sortBy: 'rating', isDescending: true }).subscribe({
      next: (data) => {
        this.featuredVendors = data.filter(v => v.status === 'active' || v.isApproved).slice(0, 6);
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

  filterByEventType(slug: string) {
    const match = this.eventTypes.find(t =>
      t.name.toLowerCase() === slug.toLowerCase() ||
      t.name.toLowerCase().includes(slug.toLowerCase())
    );
    this.router.navigate(['/explore'], {
      queryParams: {
        tab: 'services',
        eventTypeId: match?.id ?? null,
        eventType: match ? null : slug
      }
    });
  }

  filterVendorType(typeName: string) {
    this.router.navigate(['/explore'], { queryParams: { tab: 'vendors', type: typeName } });
  }

  bookPackage() {
    this.router.navigate(['/add-event']);
  }
}
