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
  selectedCategory = 'all';
  selectedEventTypes: string[] = [];
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
      if (params['category']) this.selectedCategory = params['category'];
      if (params['q']) this.searchQuery = params['q'];
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
    this.serviceTypeService.getAll().subscribe(cats => {
      this.categories = cats;
    });

    // Fetch products
    this.productService.getAll().subscribe({
      next: (data) => {
        this.services = data;
        this.applyFilters();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading services', err);
        this.loading = false;
      }
    });
  }

  applyFilters() {
    let filtered = [...this.services];

    // Search
    if (this.searchQuery) {
      const q = this.searchQuery.toLowerCase();
      filtered = filtered.filter(s => 
        s.name.toLowerCase().includes(q) || 
        (s.description && s.description.toLowerCase().includes(q))
      );
    }

    // Category
    if (this.selectedCategory !== 'all') {
      filtered = filtered.filter(s => 
        s.serviceTypeId === this.selectedCategory || 
        s.categoryName === this.selectedCategory
      );
    }

    // Price
    filtered = filtered.filter(s => s.price <= this.maxPrice);

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
    this.selectedCategory = this.selectedCategory === cat ? 'all' : cat;
    this.applyFilters();
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
