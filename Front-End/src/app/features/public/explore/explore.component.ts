import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';
import { ApiVendor } from '../../../shared/types/api.interfaces';
import { VendorService } from '../../../core/services/vendor.service';

@Component({
  selector: 'app-explore',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, VendorCardComponent],
  templateUrl: './explore.component.html',
  styleUrls: ['./explore.component.scss']
})
export class ExploreComponent implements OnInit {
  activePanel: string | null = null;
  sortOption = 'rating';
  vendorCount = 0;
  loading = false;

  // Temporary local state for panel before applying
  activeType = '';
  activeLoc = '';
  activeRating = 0;

  // Active applied filters
  filters = {
    type: '',
    loc: '',
    rating: 0,
    searchQuery: ''
  };

  allVendors: ApiVendor[] = [];
  displayVendors: ApiVendor[] = [];

  constructor(private route: ActivatedRoute, private vendorService: VendorService) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['category']) {
        this.filters.type = params['category'];
        this.activeType = params['category'];
      }
      if (params['type']) {
        this.filters.type = params['type'];
        this.activeType = params['type'];
      }
      if (params['q']) {
        this.filters.searchQuery = params['q'];
      }
      this.loadVendors();
    });
  }

  loadVendors() {
    this.loading = true;
    this.vendorService.getAll().subscribe({
      next: (data) => {
        // Assume active vendors only
        this.allVendors = data.filter(v => v.status === 'active' || v.isApproved);
        this.loading = false;
        this.triggerSearch();
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  togglePanel(panel: string) {
    if (this.activePanel === panel) {
      this.activePanel = null;
    } else {
      this.activePanel = panel;
      // sync temp state with filters
      this.activeType = this.filters.type;
      this.activeLoc = this.filters.loc;
      this.activeRating = this.filters.rating;
    }
  }

  updateFilters() {
    this.filters.type = this.activeType;
    this.filters.loc = this.activeLoc;
    this.filters.rating = this.activeRating;
    this.activePanel = null;
    this.triggerSearch();
  }

  clearAllFilters() {
    this.filters.type = '';
    this.filters.loc = '';
    this.filters.rating = 0;
    this.filters.searchQuery = '';

    this.activeType = '';
    this.activeLoc = '';
    this.activeRating = 0;
    this.triggerSearch();
  }

  triggerSearch() {
    let filtered = this.allVendors.filter(v => {
      const matchType = !this.filters.type || (v.vendorTypeName && v.vendorTypeName.toLowerCase() === this.filters.type.toLowerCase());
      const matchLoc = !this.filters.loc || (v.location && v.location.toLowerCase() === this.filters.loc.toLowerCase());
      const matchRating = (v.rating || 0) >= this.filters.rating;
      const matchQuery = !this.filters.searchQuery ||
        v.name.toLowerCase().includes(this.filters.searchQuery.toLowerCase()) ||
        (v.vendorTypeName && v.vendorTypeName.toLowerCase().includes(this.filters.searchQuery.toLowerCase()));

      return matchType && matchLoc && matchRating && matchQuery;
    });

    // Sorting
    if (this.sortOption === 'rating') filtered.sort((a, b) => (b.rating || 0) - (a.rating || 0));

    this.displayVendors = filtered;
    this.vendorCount = filtered.length;
  }
}
