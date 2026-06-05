import { Component, OnInit, OnDestroy, AfterViewInit, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';
import { ApiVendor, ApiProduct, ServiceType } from '../../../shared/types/api.interfaces';
import { VendorType } from '../../../core/models/taxonomy.models';
import { VendorService } from '../../../core/services/vendor.service';
import { ProductService } from '../../../core/services/product.service';
import { ServiceTypeService } from '../../../core/services/service-type.service';
import { VendorTypeService } from '../../../core/services/vendor-type.service';
import { ModalService } from '../../../shared/services/modal.service';
import { Subject, takeUntil } from 'rxjs';
import * as L from 'leaflet';

@Component({
  selector: 'app-explore',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, VendorCardComponent],
  templateUrl: './explore.component.html',
  styleUrls: ['./explore.component.scss']
})
export class ExploreComponent implements OnInit, OnDestroy, AfterViewInit {
  private destroy$ = new Subject<void>();

  // ── Tab ──────────────────────────────────────────────────
  activeTab: 'vendors' | 'services' = 'services';

  // ── Shared UI state ──────────────────────────────────────
  activePanel: string | null = null;
  sortOption = 'rating';
  loading = false;
  viewMode: 'grid' | 'list' | 'map' = 'grid';

  // ── Map ──────────────────────────────────────────────────
  private map: L.Map | undefined;
  private markersLayer: L.LayerGroup | undefined;

  // ── Vendor state ─────────────────────────────────────────
  allVendors: ApiVendor[] = [];
  displayVendors: ApiVendor[] = [];
  vendorCount = 0;

  activeType = '';
  activeLoc = '';
  activeRating = 0;
  filters = { type: '', loc: '', rating: 0, searchQuery: '' };
  vendorTypes: VendorType[] = [];

  // ── Service state ────────────────────────────────────────
  services: ApiProduct[] = [];
  filteredServices: ApiProduct[] = [];
  serviceCategories: ServiceType[] = [];
  serviceCount = 0;

  selectedCategories: string[] = [];
  selectedEventTypes: string[] = [];
  maxPrice = 100000;
  minRating = 0;
  selectedCity = '';

  // ── Pagination ───────────────────────────────────────────
  currentPage = 1;
  readonly pageSize = 12;

  get totalPages(): number {
    const total = this.activeTab === 'vendors' ? this.displayVendors.length : this.filteredServices.length;
    return Math.max(1, Math.ceil(total / this.pageSize));
  }

  get pageNumbers(): number[] {
    const t = this.totalPages;
    if (t <= 7) return Array.from({ length: t }, (_, i) => i + 1);
    const cur = this.currentPage;
    const pages: number[] = [1];
    if (cur > 3) pages.push(-1); // ellipsis
    for (let i = Math.max(2, cur - 1); i <= Math.min(t - 1, cur + 1); i++) pages.push(i);
    if (cur < t - 2) pages.push(-1); // ellipsis
    pages.push(t);
    return pages;
  }

  get pagedVendors(): ApiVendor[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.displayVendors.slice(start, start + this.pageSize);
  }

  get pagedServices(): ApiProduct[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredServices.slice(start, start + this.pageSize);
  }

  goToPage(page: number) {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.currentPage = page;
    window.scrollTo({ top: 400, behavior: 'smooth' });
  }

  // Preview modal
  showPreview = false;
  selectedService: ApiProduct | null = null;
  previewImages: string[] = [];
  activeImageIndex = 0;

  // Compare / wishlist
  compareList: ApiProduct[] = [];
  wishlist: string[] = [];
  showCompareBar = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private vendorService: VendorService,
    private productService: ProductService,
    private serviceTypeService: ServiceTypeService,
    private vendorTypeService: VendorTypeService,
    private modalService: ModalService,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    // Support /explore-services route via route data
    const routeData = this.route.snapshot.data;
    if (routeData['tab'] === 'services') this.activeTab = 'services';

    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      // Determine active tab from query param (default: services)
      if (params['tab'] === 'vendors') {
        this.activeTab = 'vendors';
      } else if (!routeData['tab']) {
        this.activeTab = 'services';
      }

      // Vendor params
      if (params['category'] || params['type']) {
        this.filters.type = params['category'] || params['type'];
        this.activeType = this.filters.type;
      }
      if (params['q']) this.filters.searchQuery = params['q'];

      // Service params
      if (params['serviceCategory']) {
        let cat = params['serviceCategory'];
        if (cat.toLowerCase() === 'decor') {
          cat = 'Decoration';
        }
        this.selectedCategories = [cat];
      }
      if (params['eventType']) this.selectedEventTypes = [params['eventType']];

      const openServiceId = params['openServiceId'];
      if (openServiceId) {
        this.productService.getById(openServiceId).subscribe({
          next: (svc) => {
            if (svc) {
              this.activeTab = 'services';
              this.modalService.open('service-detail', svc);
            }
          },
          error: (err) => {
            console.error('Error fetching service for auto-open:', err);
          }
        });
      }

      this.loadData();
    });
  }

  ngAfterViewInit() {}

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.map) this.map.remove();
  }

  // ── Tab switching ────────────────────────────────────────
  switchTab(tab: 'vendors' | 'services') {
    if (this.activeTab === tab) return;
    this.activeTab = tab;
    this.activePanel = null;
    this.viewMode = 'grid';
    this.destroyMap();
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: tab === 'vendors' ? { tab: 'vendors' } : {},
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
    this.loadData();
  }

  loadData() {
    if (this.activeTab === 'vendors') {
      this.loadVendors();
    } else {
      this.loadServices();
    }
  }

  // ── Vendor logic ─────────────────────────────────────────
  loadVendors() {
    this.loading = true;

    const doLoad = () => {
      const typeMatch = this.vendorTypes.find(
        t => t.name.toLowerCase() === this.filters.type.toLowerCase()
      );
      this.vendorService.getAll({
        pageSize: 1000,
        pageIndex: 1,
        searchTerm: this.filters.searchQuery || undefined,
        city: this.filters.loc || undefined,
        vendorTypeId: typeMatch?.id,
      }).subscribe({
        next: (data) => {
          this.allVendors = data.filter(v => v.status === 'active' || v.isApproved);
          this.loading = false;
          this.triggerSearch();
        },
        error: () => { this.loading = false; }
      });
    };

    if (this.vendorTypes.length > 0) {
      doLoad();
    } else {
      this.vendorTypeService.getAll().subscribe({
        next: (types) => { this.vendorTypes = Array.isArray(types) ? types : []; doLoad(); },
        error: () => doLoad()
      });
    }
  }

  updateFilters() {
    this.filters.type = this.activeType;
    this.filters.loc = this.activeLoc;
    this.filters.rating = this.activeRating;
    this.activePanel = null;
    this.loadVendors();
  }

  triggerSearch() {
    let filtered = this.allVendors.filter(v => {
      const matchType = !this.filters.type || (v.vendorTypeName?.toLowerCase() === this.filters.type.toLowerCase());
      const matchLoc = !this.filters.loc ||
        v.location?.toLowerCase().includes(this.filters.loc.toLowerCase()) ||
        v.serviceAreas?.some(a => a.city?.toLowerCase().includes(this.filters.loc.toLowerCase()));
      const matchRating = (v.rating || 0) >= this.filters.rating;
      const matchQ = !this.filters.searchQuery ||
        v.name.toLowerCase().includes(this.filters.searchQuery.toLowerCase()) ||
        (v.vendorTypeName?.toLowerCase().includes(this.filters.searchQuery.toLowerCase()));
      return matchType && matchLoc && matchRating && matchQ;
    });

    if (this.sortOption === 'rating') filtered.sort((a, b) => (b.rating || 0) - (a.rating || 0));

    this.displayVendors = filtered;
    this.vendorCount = filtered.length;
    this.currentPage = 1;
    this.updateMapMarkers();
  }

  // ── Service logic ────────────────────────────────────────
  loadServices() {
    this.loading = true;
    this.serviceTypeService.getAll().subscribe({
      next: (cats) => { this.serviceCategories = Array.isArray(cats) ? cats : []; },
      error: () => { this.serviceCategories = []; }
    });

    const req: any = {
      city: this.selectedCity || undefined,
      pageSize: 1000,
      pageIndex: 1,
      searchTerm: this.filters.searchQuery || undefined,
    };
    if (this.selectedEventTypes.length) req.eventTypeId = this.selectedEventTypes[0];
    if (this.selectedCategories.length) req.serviceTypeId = this.selectedCategories[0];

    this.productService.getAll(req).subscribe({
      next: (data) => {
        this.services = Array.isArray(data) ? data : [];
        this.applyServiceFilters();
        this.loading = false;
      },
      error: () => { this.services = []; this.loading = false; }
    });
  }

  applyServiceFilters() {
    let f = [...this.services];
    if (this.selectedCategories.length) {
      const selectedNames = this.serviceCategories
        .filter(cat => this.selectedCategories.includes(cat.id))
        .map(cat => cat.name.toLowerCase());

      f = f.filter(s =>
        (s.serviceTypeId && this.selectedCategories.includes(s.serviceTypeId)) ||
        (s.serviceTypeName && selectedNames.includes(s.serviceTypeName.toLowerCase())) ||
        (s.vendorTypeName && selectedNames.includes(s.vendorTypeName.toLowerCase()))
      );
    }
    f = f.filter(s => (s.price ?? 0) <= this.maxPrice);
    f = f.filter(s => ((s as any).rating || 5) >= this.minRating);

    if (this.sortOption === 'price-asc') f.sort((a, b) => a.price - b.price);
    else if (this.sortOption === 'price-desc') f.sort((a, b) => b.price - a.price);
    else if (this.sortOption === 'rating') f.sort((a, b) => ((b as any).rating || 5) - ((a as any).rating || 5));

    this.filteredServices = f;
    this.serviceCount = f.length;
    this.currentPage = 1;
    this.updateMapMarkers();
  }

  toggleCategory(id: string) {
    const i = this.selectedCategories.indexOf(id);
    if (i > -1) this.selectedCategories.splice(i, 1);
    else this.selectedCategories.push(id);
    this.loadServices();
  }

  toggleEventType(ev: string) {
    const i = this.selectedEventTypes.indexOf(ev);
    if (i > -1) this.selectedEventTypes.splice(i, 1);
    else this.selectedEventTypes.push(ev);
    this.loadServices();
  }

  // ── Shared filter helpers ────────────────────────────────
  togglePanel(panel: string) {
    if (this.activePanel === panel) {
      this.activePanel = null;
    } else {
      this.activePanel = panel;
      if (this.activeTab === 'vendors') {
        this.activeType = this.filters.type;
        this.activeLoc = this.filters.loc;
        this.activeRating = this.filters.rating;
      }
    }
  }

  clearServiceType() { this.selectedCategories = []; this.loadServices(); }

  setMaxPrice(p: number) { this.maxPrice = p; this.applyServiceFilters(); }
  clearMaxPrice() { this.maxPrice = 100000; this.applyServiceFilters(); }

  setRating(r: number) {
    if (this.activeTab === 'vendors') { this.activeRating = r; }
    else { this.minRating = r; this.applyServiceFilters(); }
  }
  clearRating() {
    if (this.activeTab === 'vendors') { this.activeRating = 0; this.updateFilters(); }
    else { this.minRating = 0; this.applyServiceFilters(); }
  }

  clearAllFilters() {
    this.activePanel = null;
    if (this.activeTab === 'vendors') {
      this.filters = { type: '', loc: '', rating: 0, searchQuery: '' };
      this.activeType = ''; this.activeLoc = ''; this.activeRating = 0;
      this.triggerSearch();
    } else {
      this.selectedCategories = [];
      this.selectedEventTypes = [];
      this.maxPrice = 100000;
      this.minRating = 0;
      this.selectedCity = '';
      this.loadServices();
    }
  }

  // ── View & map ───────────────────────────────────────────
  setView(mode: 'grid' | 'list' | 'map') {
    this.viewMode = mode;
    if (mode === 'map') {
      this.initMap();
      if (this.map) setTimeout(() => this.map!.invalidateSize(), 150);
    }
  }

  private destroyMap() {
    if (this.map) { this.map.remove(); this.map = undefined; this.markersLayer = undefined; }
  }

  private initMap() {
    if (this.map) return;
    setTimeout(() => {
      this.map = L.map('explore-map').setView([30.0444, 31.2357], 11);
      L.control.zoom({ position: 'bottomright' }).addTo(this.map);
      L.tileLayer('https://{s}.basemaps.cartocdn.com/rastertiles/voyager/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; OpenStreetMap &copy; CARTO',
        subdomains: 'abcd', maxZoom: 19
      }).addTo(this.map);
      this.markersLayer = L.layerGroup().addTo(this.map);
      this.updateMapMarkers();
    }, 100);
  }

  private updateMapMarkers() {
    if (!this.map || !this.markersLayer) return;
    this.markersLayer.clearLayers();
    const baseLat = 30.0444, baseLng = 31.2357;

    if (this.activeTab === 'vendors') {
      this.displayVendors.forEach((vendor, i) => {
        const areas = vendor.serviceAreas?.length ? vendor.serviceAreas : [{ latitude: 0, longitude: 0 }];
        areas.forEach((area, ai) => {
          let lat = area.latitude || baseLat + Math.sin((i + ai) * 23) * 0.05;
          let lng = area.longitude || baseLng + Math.cos((i + ai) * 29) * 0.05;
          const icon = L.divIcon({
            className: 'custom-map-marker',
            html: `<div style="background:white;border-radius:50px;padding:6px 16px;box-shadow:0 4px 15px rgba(0,0,0,.1);font-weight:600;font-size:.85rem;display:flex;align-items:center;gap:8px;border:1.5px solid #e8e0ec;white-space:nowrap;cursor:pointer;" onmouseover="this.style.borderColor='#c9a84c'" onmouseout="this.style.borderColor='#e8e0ec'">
              <i class="bi bi-shop" style="color:#c9a84c"></i>
              <span style="color:#1e0e2c">${vendor.name}</span>
            </div>`, iconSize: [200, 40], iconAnchor: [100, 20]
          });
          const m = L.marker([lat, lng], { icon }).addTo(this.markersLayer!);
          m.on('click', () => this.ngZone.run(() => this.router.navigate(['/vendor', vendor.id])));
        });
      });
    } else {
      this.filteredServices.forEach((svc, i) => {
        const areas = (svc as any).serviceAreas?.length ? (svc as any).serviceAreas : [{ latitude: 0, longitude: 0 }];
        areas.forEach((area: any, ai: number) => {
          let lat = area.latitude || baseLat + Math.sin((i + ai) * 13) * 0.05;
          let lng = area.longitude || baseLng + Math.cos((i + ai) * 17) * 0.05;
          const price = (svc.price || 0).toLocaleString() + ' EGP';
          const icon = L.divIcon({
            className: 'custom-map-marker',
            html: `<div style="background:white;border-radius:50px;padding:6px 16px;box-shadow:0 4px 15px rgba(0,0,0,.1);font-weight:600;font-size:.85rem;display:flex;align-items:center;gap:8px;border:1.5px solid #e8e0ec;white-space:nowrap;cursor:pointer;" onmouseover="this.style.borderColor='#c9a84c'" onmouseout="this.style.borderColor='#e8e0ec'">
              <i class="bi bi-image" style="color:#c9a84c"></i>
              <span style="color:#1e0e2c">${svc.name}</span>
              <span style="color:#c9a84c">${price}</span>
            </div>`, iconSize: [260, 40], iconAnchor: [130, 20]
          });
          const m = L.marker([lat, lng], { icon }).addTo(this.markersLayer!);
          m.on('click', () => this.ngZone.run(() => this.openPreview(svc)));
        });
      });
    }
  }

  // ── Service preview ──────────────────────────────────────
  openPreview(svc: ApiProduct) {
    this.selectedService = svc;
    this.previewImages = this.getPreviewImages(svc);
    this.activeImageIndex = 0;
    this.showPreview = true;
  }

  closePreview() {
    this.showPreview = false;
    this.selectedService = null;
  }

  bookService(svc: ApiProduct) {
    this.closePreview();
    this.modalService.open('service-detail', svc);
  }

  private getPreviewImages(svc: ApiProduct): string[] {
    if (svc.imageUrls?.length) return svc.imageUrls.filter(u => !!u);
    if (svc.imageUrl) return svc.imageUrl.split(',').map(s => s.trim()).filter(s => !!s);
    return [];
  }

  // ── Compare / wishlist ───────────────────────────────────
  toggleWishlist(svc: ApiProduct, e: Event) {
    e.stopPropagation();
    const i = this.wishlist.indexOf(svc.id);
    if (i > -1) this.wishlist.splice(i, 1); else this.wishlist.push(svc.id);
  }

  isInWishlist(id: string) { return this.wishlist.includes(id); }

  addToCompare(svc: ApiProduct, e: Event) {
    e.stopPropagation();
    if (this.compareList.find(s => s.id === svc.id)) {
      this.compareList = this.compareList.filter(s => s.id !== svc.id);
    } else if (this.compareList.length < 3) {
      this.compareList.push(svc);
    }
    this.showCompareBar = this.compareList.length > 0;
  }

  isInCompare(id: string) { return !!this.compareList.find(s => s.id === id); }

  clearCompare() { this.compareList = []; this.showCompareBar = false; }
}
