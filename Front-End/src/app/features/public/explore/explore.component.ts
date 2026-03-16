import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';
import { MOCK_VENDORS } from '../../../shared/data/mock-vendors.data';
import { Vendor } from '../../../shared/types/vendor.interface';

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

  // Temporary local state for panel before applying
  activeType = '';
  activeLoc = '';
  activePrice = 100000;
  activeRating = 0;

  // Active applied filters
  filters = {
    type: '',
    loc: '',
    maxPrice: 100000,
    rating: 0,
    searchQuery: ''
  };

  allVendors = [...MOCK_VENDORS];
  displayVendors: Vendor[] = [];

  constructor(private route: ActivatedRoute) { }

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
      this.triggerSearch();
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
      this.activePrice = this.filters.maxPrice;
      this.activeRating = this.filters.rating;
    }
  }

  updateFilters() {
    this.filters.type = this.activeType;
    this.filters.loc = this.activeLoc;
    this.filters.maxPrice = this.activePrice;
    this.filters.rating = this.activeRating;
    this.activePanel = null;
    this.triggerSearch();
  }

  clearAllFilters() {
    this.filters.type = '';
    this.filters.loc = '';
    this.filters.maxPrice = 100000;
    this.filters.rating = 0;
    this.filters.searchQuery = '';

    this.activeType = '';
    this.activeLoc = '';
    this.activePrice = 100000;
    this.activeRating = 0;
    this.triggerSearch();
  }

  triggerSearch() {
    let filtered = this.allVendors.filter(v => {
      const matchType = !this.filters.type || v.category === this.filters.type;
      const matchLoc = !this.filters.loc || v.location === this.filters.loc;
      const matchPrice = v.startPrice <= this.filters.maxPrice;
      const matchRating = v.rating >= this.filters.rating;
      const matchQuery = !this.filters.searchQuery ||
        v.name.toLowerCase().includes(this.filters.searchQuery.toLowerCase()) ||
        v.category.toLowerCase().includes(this.filters.searchQuery.toLowerCase());

      return matchType && matchLoc && matchPrice && matchRating && matchQuery;
    });

    // Sorting
    if (this.sortOption === 'price_asc') filtered.sort((a, b) => a.startPrice - b.startPrice);
    else if (this.sortOption === 'price_desc') filtered.sort((a, b) => b.startPrice - a.startPrice);
    else if (this.sortOption === 'rating') filtered.sort((a, b) => b.rating - a.rating);
    else if (this.sortOption === 'reviews') filtered.sort((a, b) => b.reviews - a.reviews);

    this.displayVendors = filtered;
    this.vendorCount = filtered.length;
  }
}
