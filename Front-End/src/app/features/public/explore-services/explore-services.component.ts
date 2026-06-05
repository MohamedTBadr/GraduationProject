import { Component, OnInit, OnDestroy, AfterViewInit, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { ServiceTypeService } from '../../../core/services/service-type.service';
import { ApiProduct, ServiceType } from '../../../shared/types/api.interfaces';
import { ModalService } from '../../../shared/services/modal.service';
import { Subject, takeUntil } from 'rxjs';
import * as L from 'leaflet';

@Component({
  selector: 'app-explore-services',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './explore-services.component.html',
  styleUrls: ['./explore-services.component.scss']
})
export class ExploreServicesComponent implements OnInit, OnDestroy, AfterViewInit {
  private destroy$ = new Subject<void>();

  // Map State
  private map: L.Map | undefined;
  private markersLayer: L.LayerGroup | undefined;

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
  selectedCategories: string[] = [];
  selectedEventTypes: string[] = [];
  selectedClassification: string = 'all';
  maxPrice = 100000;
  minRating = 0;
  
  // Advanced Location Filters
  selectedCity: string = '';
  selectedRegion: string = '';
  latitude?: number;
  longitude?: number;
  radiusKm: number = 50;
  isLocating = false;

  showAvailableOnly = false;
  instantBookingOnly = false;
  sortOption = 'recommended';

  // Compare & Wishlist
  compareList: ApiProduct[] = [];
  wishlist: string[] = []; // IDs
  showCompareBar = false;
  showCompareModal = false;

  // AI Studio
  showAiStudio = false;

  // Filter panel (pill dropdowns — mirrors explore-vendors UX)
  activePanel: string | null = null;

  // Preview
  selectedPreviewService: ApiProduct | null = null;
  showPreview = false;
  previewSlideIndex = 0;

  // Aliases for template
  get selectedService() { return this.selectedPreviewService; }
  get activeImageIndex() { return this.previewSlideIndex; }
  set activeImageIndex(v: number) { this.previewSlideIndex = v; }
  get previewImages(): string[] { return this.selectedPreviewService ? this.getPreviewImages(this.selectedPreviewService) : []; }

  constructor(
    private productService: ProductService,
    private serviceTypeService: ServiceTypeService,
    private route: ActivatedRoute,
    private ngZone: NgZone,
    private modalService: ModalService
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

  ngAfterViewInit() {
    // Map will be initialized when viewMode changes to 'map'
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.map) {
      this.map.remove();
    }
  }

  loadData() {
    this.loading = true;
    
    // Fetch categories
    this.serviceTypeService.getAll().subscribe({
      next: (cats) => {
        this.categories = Array.isArray(cats) ? cats : [];
        // Re-apply if categories arrived after the products (race condition)
        if (this.selectedCategories.length > 0 && !this.loading) {
          this.applyFilters();
        }
      },
      error: (err) => {
        console.error('Error loading service types', err);
        this.categories = [];
      }
    });

    // Construct PaginatedRequest for backend
    const request: any = {
      searchTerm: this.searchQuery,
      classification: this.selectedClassification !== 'all' ? this.selectedClassification : undefined,
      city: this.selectedCity || undefined,
      region: this.selectedRegion || undefined,
      latitude: this.latitude,
      longitude: this.longitude,
      radiusKm: this.radiusKm
    };

    if (this.selectedEventTypes.length > 0) {
      request.eventTypeId = this.selectedEventTypes[0];
    }
    if (this.selectedCategories.length > 0) {
      request.serviceTypeId = this.selectedCategories[0];
    }

    this.productService.getAll(request).subscribe({
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
      const selectedNames = this.categories
        .filter(cat => this.selectedCategories.includes(cat.id))
        .map(cat => cat.name.toLowerCase());

      filtered = filtered.filter(s =>
        (s.serviceTypeId && this.selectedCategories.includes(s.serviceTypeId)) ||
        (s.serviceTypeName && selectedNames.includes(s.serviceTypeName.toLowerCase())) ||
        (s.vendorTypeName && selectedNames.includes(s.vendorTypeName.toLowerCase()))
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
    this.updateMapMarkers();
  }

  setView(mode: 'grid' | 'list' | 'map') {
    this.viewMode = mode;
    if (mode === 'map') {
      this.initMap();
      if (this.map) {
        setTimeout(() => this.map!.invalidateSize(), 100);
      }
    }
  }

  private initMap() {
    if (this.map) return;
    
    setTimeout(() => {
      this.map = L.map('service-map', {
        zoomControl: false // Optional: hide default zoom controls
      }).setView([30.0444, 31.2357], 11); // Cairo coordinates

      // Add zoom control manually to right-bottom or something if needed
      L.control.zoom({ position: 'bottomright' }).addTo(this.map);

      // Using CARTO Voyager for a clean, light base map
      L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; OpenStreetMap &copy; CARTO',
        subdomains: 'abcd',
        maxZoom: 19
      }).addTo(this.map);

      this.markersLayer = L.layerGroup().addTo(this.map);
      this.updateMapMarkers();
    }, 100);
  }

  private updateMapMarkers() {
    if (!this.map || !this.markersLayer) return;

    this.markersLayer.clearLayers();

    // Generate pseudo-random coordinates around Cairo if none exist for demonstration
    const baseLat = 30.0444;
    const baseLng = 31.2357;

    this.filteredServices.forEach((svc, index) => {
      const areas = svc.serviceAreas && svc.serviceAreas.length > 0 ? svc.serviceAreas : [{ latitude: 0, longitude: 0 }];

      areas.forEach((area, areaIdx) => {
        let lat = area.latitude;
        let lng = area.longitude;

        // If no real coordinates, fallback to pseudo-random around Cairo
        if (lat === 0 && lng === 0) {
          const latOffset = (Math.sin((index + areaIdx) * 13) * 0.05);
          const lngOffset = (Math.cos((index + areaIdx) * 17) * 0.05);
          lat = baseLat + latOffset;
          lng = baseLng + lngOffset;
        }

        const formattedPrice = (svc.price || 0).toLocaleString() + ' EGP';
        const markerIcon = svc.imageUrl
          ? '<i class="bi bi-image" style="font-size:1rem;color:#c9a84c"></i>'
          : '<i class="bi bi-palette" style="font-size:1rem;color:#c9a84c"></i>';

        const customIcon = L.divIcon({
          className: 'custom-map-marker',
          html: `<div style="background: white; border-radius: 50px; padding: 6px 16px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); font-weight: 600; font-size: 0.85rem; display: flex; align-items: center; gap: 8px; border: 1.5px solid #e8e4dc; white-space: nowrap; cursor: pointer; transition: transform 0.2s ease, border-color 0.2s ease;" onmouseover="this.style.transform='scale(1.05)'; this.style.borderColor='#c9a84c'" onmouseout="this.style.transform='scale(1)'; this.style.borderColor='#e8e4dc'">
                   ${markerIcon}
                   <span style="color: #1a2540;">${svc.name}</span>
                   <span style="color: #c9a84c;">${formattedPrice}</span>
                 </div>`,
          iconSize: [220, 40],
          iconAnchor: [110, 20]
        });

        const marker = L.marker([lat, lng], { icon: customIcon }).addTo(this.markersLayer!);
        marker.on('click', () => {
          this.ngZone.run(() => {
            this.openPreview(svc);
          });
        });
      });
    });
  }

  toggleCategory(cat: string) {
    const idx = this.selectedCategories.indexOf(cat);
    if (idx > -1) {
      this.selectedCategories.splice(idx, 1);
    } else {
      this.selectedCategories.push(cat);
    }
    this.loadData();
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

  getCurrentLocation() {
    if (!navigator.geolocation) {
      alert('Geolocation is not supported by your browser');
      return;
    }

    this.isLocating = true;
    navigator.geolocation.getCurrentPosition(
      (position) => {
        this.latitude = position.coords.latitude;
        this.longitude = position.coords.longitude;
        this.isLocating = false;
        this.loadData(); // Reload with lat/lng
      },
      (error) => {
        console.error('Error getting location', error);
        this.isLocating = false;
        alert('Could not get your location. Please check your permissions.');
      }
    );
  }

  clearLocation() {
    this.latitude = undefined;
    this.longitude = undefined;
    this.selectedCity = '';
    this.selectedRegion = '';
    this.loadData();
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
    this.previewSlideIndex = 0;
  }

  closePreview() {
    this.showPreview = false;
    this.selectedPreviewService = null;
    this.previewSlideIndex = 0;
  }

  getPreviewImages(svc: ApiProduct): string[] {
    if (!svc) return [];
    if (svc.imageUrls && Array.isArray(svc.imageUrls) && svc.imageUrls.length > 0) {
      return svc.imageUrls.filter((u: string) => !!u);
    }
    if (svc.imageUrl) {
      const parts = svc.imageUrl.split(',').map((s: string) => s.trim()).filter((s: string) => !!s);
      if (parts.length > 0) return parts;
    }
    return [];
  }

  prevPreviewSlide(images: string[]) {
    this.previewSlideIndex = this.previewSlideIndex > 0 ? this.previewSlideIndex - 1 : images.length - 1;
  }

  nextPreviewSlide(images: string[]) {
    this.previewSlideIndex = this.previewSlideIndex < images.length - 1 ? this.previewSlideIndex + 1 : 0;
  }

  goToPreviewSlide(index: number) {
    this.previewSlideIndex = index;
  }

  togglePreviewWishlist(id: string, event: Event) {
    event.stopPropagation();
    const idx = this.wishlist.indexOf(id);
    if (idx > -1) {
      this.wishlist.splice(idx, 1);
    } else {
      this.wishlist.push(id);
    }
  }

  togglePanel(panel: string) {
    this.activePanel = this.activePanel === panel ? null : panel;
  }

  bookService(svc: ApiProduct) {
    this.closePreview();
    this.modalService.open('service-detail', svc);
  }

  openServiceModal(product: ApiProduct) {
    this.closePreview();
    this.modalService.open('service-detail', product);
  }
}

