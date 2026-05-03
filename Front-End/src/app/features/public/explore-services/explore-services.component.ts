import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { ServiceTypeService } from '../../../core/services/service-type.service';
import { ApiProduct, ServiceType } from '../../../shared/types/api.interfaces';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-explore-services',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './explore-services.component.html',
  styleUrls: ['./explore-services.component.scss']
})
export class ExploreServicesComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // State
  services: ApiProduct[] = [];
  filteredServices: ApiProduct[] = [];
  categories: ServiceType[] = [];
  viewMode: 'grid' | 'list' | 'map' = 'grid';
  loading = true;
  
  // Custom type for display (extending ApiProduct with optional rating for filtering)
  displayServices: (ApiProduct & { rating?: number })[] = [];

  
  // Filters
  searchQuery = '';
  selectedCategories: string[] = []; // Changed to array for multi-select
  selectedEventTypes: string[] = [];
  selectedClassification: string = 'all'; // Personal, Corporate, all
  maxPrice = 100000;
  minRating = 0;
  selectedLocation = 'All Egypt';
  showAvailableOnly = false;
  instantBookingOnly = false;
  sortOption = 'recommended';

  // Compare & Wishlist
  compareList: ApiProduct[] = [];
  wishlist: string[] = []; // IDs
  showCompareBar = false;
  showCompareModal = false;
  
  // Preview
  selectedPreviewService: ApiProduct | null = null;
  showPreview = false;

  constructor(
    private productService: ProductService,
    private serviceTypeService: ServiceTypeService,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    this.loadData();
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      if (params['category']) this.selectedCategories = [params['category']];
      if (params['q']) this.searchQuery = params['q'];
      if (params['eventTypeId']) this.selectedEventTypes = [params['eventTypeId']];
      this.applyFilters();
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData() {
    this.loading = true;
    
    // Fetch categories
    this.serviceTypeService.getAll().subscribe({
      next: (cats) => {
        this.categories = Array.isArray(cats) ? cats : [];
      },
      error: (err) => {
        console.error('Error loading service types', err);
        this.categories = [];
      }
    });

    // Fetch products. Passing eventTypeId if any is selected (sending to backend as requested)
    const filters = this.selectedEventTypes.length > 0 ? { eventTypeId: this.selectedEventTypes[0] } : {};
    
    this.productService.getAll(filters).subscribe({
      next: (data) => {
        this.services = Array.isArray(data) ? data : [];
        this.applyFilters();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading services', err);
        this.services = [];
        this.applyFilters();
        this.loading = false;
      }
    });
  }

  applyFilters() {
    let filtered = [...this.services];

    // Search
    if (this.searchQuery) {
      const q = this.searchQuery.toLowerCase();
      filtered = filtered.filter(s => {
        const name = (s.name ?? '').toLowerCase();
        const desc = (s.description ?? '').toLowerCase();
        return name.includes(q) || desc.includes(q);
      });
    }

    // Service Type / Vendor Type (Multi-select)
    if (this.selectedCategories.length > 0) {
      filtered = filtered.filter(s => 
        (s.serviceTypeId && this.selectedCategories.includes(s.serviceTypeId)) || 
        (s.vendorTypeName && this.selectedCategories.includes(s.vendorTypeName))
      );
    }

    // Classification
    if (this.selectedClassification !== 'all') {
      filtered = filtered.filter(s => s.classification === this.selectedClassification);
    }

    // Price
    filtered = filtered.filter(s => {
      const p = typeof s.price === 'number' && !Number.isNaN(s.price) ? s.price : 0;
      return p <= this.maxPrice;
    });

    // Rating (Assuming 5 as default if not provided, or 0)
    filtered = filtered.filter(s => ((s as any).rating || 5) >= this.minRating);

    // Sorting
    if (this.sortOption === 'price-asc') {
      filtered.sort((a, b) => a.price - b.price);
    } else if (this.sortOption === 'price-desc') {
      filtered.sort((a, b) => b.price - a.price);
    } else if (this.sortOption === 'rating') {
      filtered.sort((a, b) => ((b as any).rating || 5) - ((a as any).rating || 5));
    }

    this.filteredServices = filtered;
  }

  setView(mode: 'grid' | 'list' | 'map') {
    this.viewMode = mode;
  }

  toggleCategory(cat: string) {
    const idx = this.selectedCategories.indexOf(cat);
    if (idx > -1) {
      this.selectedCategories.splice(idx, 1);
    } else {
      this.selectedCategories.push(cat);
    }
    this.applyFilters();
  }

  toggleEventType(evtId: string) {
    const idx = this.selectedEventTypes.indexOf(evtId);
    if (idx > -1) {
      this.selectedEventTypes.splice(idx, 1);
    } else {
      this.selectedEventTypes.push(evtId);
    }
    // As per requirement, sending event type to backend
    this.loadData();
  }

  setClassification(classification: string) {
    this.selectedClassification = classification;
    this.applyFilters();
  }

  clearAllFilters() {
    this.selectedCategories = [];
    this.selectedEventTypes = [];
    this.selectedClassification = 'all';
    this.searchQuery = '';
    this.loadData();
  }

  updatePrice(event: any) {
    this.maxPrice = event.target.value;
    this.applyFilters();
  }

  toggleWishlist(svc: ApiProduct, event: Event) {
    event.stopPropagation();
    const idx = this.wishlist.indexOf(svc.id);
    if (idx > -1) {
      this.wishlist.splice(idx, 1);
    } else {
      this.wishlist.push(svc.id);
    }
  }

  isInWishlist(id: string): boolean {
    return this.wishlist.includes(id);
  }

  addToCompare(svc: ApiProduct, event: Event) {
    event.stopPropagation();
    if (this.compareList.find(s => s.id === svc.id)) {
      this.compareList = this.compareList.filter(s => s.id !== svc.id);
    } else if (this.compareList.length < 3) {
      this.compareList.push(svc);
    }
    this.showCompareBar = this.compareList.length > 0;
  }

  isInCompare(id: string): boolean {
    return !!this.compareList.find(s => s.id === id);
  }

  clearCompare() {
    this.compareList = [];
    this.showCompareBar = false;
  }

  openPreview(svc: ApiProduct) {
    this.selectedPreviewService = svc;
    this.showPreview = true;
  }

  closePreview() {
    this.showPreview = false;
    this.selectedPreviewService = null;
  }
}
