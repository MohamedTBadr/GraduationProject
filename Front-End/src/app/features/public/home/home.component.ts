import { Component, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';
import { ApiVendor } from '../../../shared/types/api.interfaces';
import { VendorService } from '../../../core/services/vendor.service';
import { EventTypeService } from '../../../core/services/event-type.service';
import { EventType } from '../../../core/models/taxonomy.models';
import { ToastService } from '../../../shared/components/toast/toast.service';

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
    private eventTypeService: EventTypeService,
    private toastService: ToastService
  ) { }

  ngOnInit() {
    this.loading = true;
    this.eventTypeService.getAll().subscribe({
      next: (types) => { this.eventTypes = Array.isArray(types) ? types : []; },
      error: () => this.toastService.show('Failed to load event categories.', 'error')
    });
    this.vendorService.getAll({ pageSize: 20, pageIndex: 1, sortBy: 'rating', isDescending: true }).subscribe({
      next: (data) => {
        this.featuredVendors = data.filter(v => v.status === 'active' || v.isApproved).slice(0, 6);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastService.show('Failed to load featured vendors.', 'error');
      }
    });
  }

  doSearch(query: string) {
    if (query) {
      this.router.navigate(['/explore'], { queryParams: { q: query } });
    }
  }

  filterByEventType(slug: string) {
    const needle = slug.toLowerCase();
    const match = this.eventTypes.find(t => {
      const name = (t.name ?? '').toLowerCase();
      return name === needle || name.includes(needle);
    });
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
