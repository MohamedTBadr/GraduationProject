import { Component, OnInit, OnDestroy, AfterViewInit, NgZone, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';
import { ApiVendor, ApiProduct, ServiceType } from '../../../shared/types/api.interfaces';
import { VendorType, EventType } from '../../../core/models/taxonomy.models';
import { VendorService } from '../../../core/services/vendor.service';
import { ProductService } from '../../../core/services/product.service';
import { ServiceTypeService } from '../../../core/services/service-type.service';
import { VendorTypeService } from '../../../core/services/vendor-type.service';
import { EventTypeService } from '../../../core/services/event-type.service';
import { ModalService } from '../../../shared/services/modal.service';
import { CompareService } from '../../../shared/services/compare.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { Subject, forkJoin, of, takeUntil } from 'rxjs';
import { debounceTime, distinctUntilChanged, tap } from 'rxjs/operators';
import { ServiceAreaDTO } from '../../../shared/types/api.interfaces';
import * as L from 'leaflet';

const EGYPT_CITIES = ['Cairo', 'New Cairo', 'Giza', 'Alexandria', 'North Coast', 'Mansoura'];
const MAX_PRICE_ANY = 100000;

@Component({
  selector: 'app-explore',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, VendorCardComponent, PaginationComponent],
  templateUrl: './explore.component.html',
  styleUrls: ['./explore.component.scss']
})
export class ExploreComponent implements OnInit, OnDestroy, AfterViewInit {
  private destroy$ = new Subject<void>();
  private searchSubject = new Subject<string>();
  private fetchSeq = 0;

  activeTab: 'vendors' | 'services' = 'services';
  activePanel: string | null = null;
  sortOption = 'rating';
  loading = false;
  viewMode: 'grid' | 'list' | 'map' = 'grid';

  private map: L.Map | undefined;
  private markersLayer: L.LayerGroup | undefined;

  // Taxonomy from backend
  vendorTypes: VendorType[] = [];
  serviceCategories: ServiceType[] = [];
  eventTypes: EventType[] = [];
  readonly cities = EGYPT_CITIES;
  readonly priceOptions = [5000, 15000, 30000, 50000, MAX_PRICE_ANY];
  readonly ratingOptions = [3, 4, 4.5, 4.8];

  // Single-select filters
  selectedVendorTypeId: string | null = null;
  selectedServiceTypeId: string | null = null;
  selectedEventTypeId: string | null = null;
  selectedLocation = '';
  selectedCity = '';
  minRating = 0;
  maxPrice = MAX_PRICE_ANY;
  searchQuery = '';

  // Results
  displayVendors: ApiVendor[] = [];
  filteredServices: ApiProduct[] = [];
  vendorCount = 0;
  serviceCount = 0;

  currentPage = 1;
  totalPages = 1;
  readonly pageSize = 12;

  showPreview = false;
  selectedService: ApiProduct | null = null;
  previewImages: string[] = [];
  activeImageIndex = 0;

  wishlist: string[] = [];
  private vendorRatingsById = new Map<string, number>();

  compareService = inject(CompareService);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private vendorService: VendorService,
    private productService: ProductService,
    private serviceTypeService: ServiceTypeService,
    private vendorTypeService: VendorTypeService,
    private eventTypeService: EventTypeService,
    private modalService: ModalService,
    private toastService: ToastService,
    private ngZone: NgZone
  ) {}

  ngOnInit() {
    const routeData = this.route.snapshot.data;
    if (routeData['tab'] === 'services') this.activeTab = 'services';

    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      takeUntil(this.destroy$)
    ).subscribe(q => {
      this.searchQuery = q;
      this.currentPage = 1;
      this.syncUrl();
      this.loadData();
    });

    this.loadTaxonomies().then(() => {
      this.route.queryParams.pipe(
        distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
        takeUntil(this.destroy$)
      ).subscribe(params => this.applyRouteParams(params, routeData));
    });
  }

  ngAfterViewInit() {}
  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.map) this.map.remove();
  }

  private loadTaxonomies(): Promise<void> {
    return new Promise(resolve => {
      forkJoin({
        vendorTypes: this.vendorTypeService.getAll(),
        serviceTypes: this.serviceTypeService.getAll(),
        eventTypes: this.eventTypeService.getAll()
      }).subscribe({
        next: ({ vendorTypes, serviceTypes, eventTypes }) => {
          this.vendorTypes = Array.isArray(vendorTypes) ? vendorTypes : [];
          this.serviceCategories = Array.isArray(serviceTypes) ? serviceTypes : [];
          this.eventTypes = Array.isArray(eventTypes) ? eventTypes : [];
          resolve();
        },
        error: () => {
          this.toastService.show('Failed to load filter options.', 'error');
          resolve();
        }
      });
    });
  }

  private applyRouteParams(params: Record<string, string>, routeData: Record<string, unknown>) {
    if (params['tab'] === 'vendors') {
      this.activeTab = 'vendors';
    } else if (!routeData['tab']) {
      this.activeTab = 'services';
    }

    this.searchQuery = params['q'] || '';
    this.currentPage = Math.max(1, parseInt(params['page'] || '1', 10) || 1);

    const typeParam = params['category'] || params['type'] || params['serviceCategory'] || '';
    if (typeParam) {
      if (params['tab'] === 'vendors') {
        this.selectedVendorTypeId = this.resolveVendorTypeId(typeParam);
      } else {
        const eventId = this.resolveEventTypeId(typeParam);
        if (eventId) {
          this.selectedEventTypeId = eventId;
          this.activeTab = 'services';
        } else {
          this.selectedServiceTypeId = this.resolveServiceTypeId(typeParam);
        }
      }
    }

    const eventParam = params['eventType'] || params['eventTypeId'] || '';
    if (eventParam) {
      this.selectedEventTypeId = this.resolveEventTypeId(eventParam);
      if (this.selectedEventTypeId) this.activeTab = 'services';
    }

    const openServiceId = params['openServiceId'];
    if (openServiceId) {
      this.productService.getById(openServiceId).subscribe({
        next: (svc) => {
          if (svc) {
            this.activeTab = 'services';
            this.modalService.open('service-detail', svc);
          }
        }
      });
    }

    this.loadData();
  }

  onSearchInput(value: string) {
    this.searchSubject.next(value.trim());
  }

  switchTab(tab: 'vendors' | 'services') {
    if (this.activeTab === tab) return;
    this.activeTab = tab;
    this.activePanel = null;
    this.viewMode = 'grid';
    this.destroyMap();
    this.currentPage = 1;
    if (tab === 'vendors') {
      this.selectedServiceTypeId = null;
      this.selectedEventTypeId = null;
      this.selectedCity = '';
      this.maxPrice = MAX_PRICE_ANY;
    } else {
      this.selectedVendorTypeId = null;
      this.selectedLocation = '';
    }
    this.syncUrl();
    this.loadData();
  }

  loadData() {
    if (this.activeTab === 'vendors') this.loadVendors();
    else this.loadServices();
  }

  /** Backend city filter 500s when Region is null — filter service areas client-side instead. */
  private needsClientSideVendorFiltering(): boolean {
    return !!this.selectedLocation || this.minRating > 0;
  }

  private needsClientSideServiceFiltering(): boolean {
    return !!this.selectedCity
      || this.minRating > 0
      || (!!this.selectedEventTypeId && this.maxPrice < MAX_PRICE_ANY);
  }

  private matchesCity(areas: ServiceAreaDTO[] | undefined, city: string): boolean {
    if (!city) return true;
    const needle = city.trim().toLowerCase();
    if (!areas?.length) return false;
    return areas.some((area) => {
      const c = (area.city ?? '').trim().toLowerCase();
      const r = (area.region ?? '').trim().toLowerCase();
      return c === needle || r === needle || c.includes(needle) || r.includes(needle);
    });
  }

  private loadVendorRatingsForServices() {
    if (!this.minRating && this.sortOption !== 'rating') {
      return of(null);
    }
    return this.vendorService.getAll({ pageIndex: 1, pageSize: 500 }).pipe(
      tap((vendors) => {
        this.vendorRatingsById.clear();
        vendors.forEach((v) => this.vendorRatingsById.set(v.id, v.rating ?? 0));
      })
    );
  }

  private getServiceRating(svc: ApiProduct): number {
    if (svc.rating != null && svc.rating > 0) return svc.rating;
    if (svc.vendorId && this.vendorRatingsById.has(svc.vendorId)) {
      return this.vendorRatingsById.get(svc.vendorId)!;
    }
    return 0;
  }

  private applyServiceFilters(items: ApiProduct[]): ApiProduct[] {
    let result = items;
    if (this.selectedCity) {
      result = result.filter((s) => this.matchesCity(s.serviceAreas, this.selectedCity));
    }
    if (this.maxPrice < MAX_PRICE_ANY) {
      result = result.filter((s) => (s.price ?? 0) <= this.maxPrice);
    }
    if (this.minRating > 0) {
      result = result.filter((s) => this.getServiceRating(s) >= this.minRating);
    }
    return result;
  }

  private getSortParams(): { sortBy?: string; isDescending?: boolean } {
    if (this.sortOption === 'price-asc') return { sortBy: 'price', isDescending: false };
    if (this.sortOption === 'price-desc') return { sortBy: 'price', isDescending: true };
    return { sortBy: 'rating', isDescending: true };
  }

  loadVendors() {
    const seq = ++this.fetchSeq;
    this.loading = true;

    const bulk = this.needsClientSideVendorFiltering();
    const sort = this.getSortParams();

    this.vendorService.getAllPaged({
      pageIndex: bulk ? 1 : this.currentPage,
      pageSize: bulk ? 500 : this.pageSize,
      searchTerm: this.searchQuery || undefined,
      vendorTypeId: this.selectedVendorTypeId || undefined,
      sortBy: sort.sortBy,
      isDescending: sort.isDescending
    }).subscribe({
      next: (result) => {
        if (seq !== this.fetchSeq) return;
        let items = result.items;
        if (this.selectedLocation) {
          items = items.filter((v) => this.matchesCity(v.serviceAreas, this.selectedLocation));
        }
        if (this.minRating > 0) {
          items = items.filter((v) => (v.rating || 0) >= this.minRating);
        }
        if (bulk) {
          this.vendorCount = items.length;
          this.totalPages = Math.max(1, Math.ceil(items.length / this.pageSize));
          const start = (this.currentPage - 1) * this.pageSize;
          this.displayVendors = items.slice(start, start + this.pageSize);
        } else {
          this.displayVendors = items;
          this.vendorCount = result.totalCount;
          this.totalPages = result.totalPages;
        }
        this.loading = false;
        this.updateMapMarkers();
      },
      error: () => {
        if (seq !== this.fetchSeq) return;
        this.displayVendors = [];
        this.vendorCount = 0;
        this.totalPages = 1;
        this.loading = false;
        this.toastService.show('Failed to load vendors. Please try again.', 'error');
      }
    });
  }

  loadServices() {
    const seq = ++this.fetchSeq;
    this.loading = true;

    const bulk = this.needsClientSideServiceFiltering();
    const sort = this.getSortParams();
    const useEventTypeEndpoint = !!this.selectedEventTypeId;
    const baseReq = {
      pageIndex: bulk ? 1 : this.currentPage,
      pageSize: bulk ? 500 : this.pageSize,
      searchTerm: this.searchQuery || undefined,
      serviceTypeId: this.selectedServiceTypeId || undefined,
      maxPrice: (!useEventTypeEndpoint && this.maxPrice < MAX_PRICE_ANY) ? this.maxPrice : undefined,
      // Rating sort is applied client-side; omit sort params so authenticated
      // customers don't hit a backend bug with isDescending-only requests.
      sortBy: sort.sortBy === 'rating' ? undefined : sort.sortBy,
      isDescending: sort.sortBy === 'rating' ? undefined : sort.isDescending
    };

    const request$ = useEventTypeEndpoint
      ? this.productService.getByEventTypePaged(this.selectedEventTypeId!, baseReq)
      : this.productService.getAllPaged(baseReq);

    forkJoin({
      ratings: this.loadVendorRatingsForServices(),
      result: request$
    }).subscribe({
      next: ({ result }) => {
        if (seq !== this.fetchSeq) return;
        let items: ApiProduct[] = result.items.map((s) => ({
          ...s,
          rating: this.getServiceRating(s)
        }));

        if (bulk) {
          items = this.applyServiceFilters(items);
          if (sort.sortBy === 'rating') {
            items.sort((a, b) => (b.rating ?? 0) - (a.rating ?? 0));
          }
          this.serviceCount = items.length;
          this.totalPages = Math.max(1, Math.ceil(items.length / this.pageSize));
          const start = (this.currentPage - 1) * this.pageSize;
          this.filteredServices = items.slice(start, start + this.pageSize);
        } else {
          if (sort.sortBy === 'rating') {
            items.sort((a, b) => (b.rating ?? 0) - (a.rating ?? 0));
          }
          this.filteredServices = items;
          this.serviceCount = result.totalCount;
          this.totalPages = result.totalPages;
        }
        this.loading = false;
        this.updateMapMarkers();
      },
      error: () => {
        if (seq !== this.fetchSeq) return;
        this.filteredServices = [];
        this.serviceCount = 0;
        this.totalPages = 1;
        this.loading = false;
        this.toastService.show('Failed to load services. Please try again.', 'error');
      }
    });
  }

  // ── Single-select filter handlers (auto-apply) ─────────────
  selectVendorType(id: string) {
    this.selectedVendorTypeId = this.selectedVendorTypeId === id ? null : id;
    this.closePanelAndReload();
  }

  selectServiceType(id: string) {
    this.selectedServiceTypeId = this.selectedServiceTypeId === id ? null : id;
    this.closePanelAndReload();
  }

  selectEventType(id: string) {
    this.selectedEventTypeId = this.selectedEventTypeId === id ? null : id;
    this.closePanelAndReload();
  }

  selectLocation(city: string) {
    this.selectedLocation = this.selectedLocation === city ? '' : city;
    this.closePanelAndReload();
  }

  selectCity(city: string) {
    this.selectedCity = this.selectedCity === city ? '' : city;
    this.closePanelAndReload();
  }

  selectRating(r: number) {
    this.minRating = this.minRating === r ? 0 : r;
    this.closePanelAndReload();
  }

  selectMaxPrice(p: number) {
    this.maxPrice = this.maxPrice === p ? MAX_PRICE_ANY : p;
    this.closePanelAndReload();
  }

  onSortChange() {
    this.currentPage = 1;
    this.loadData();
  }

  private closePanelAndReload() {
    this.activePanel = null;
    this.currentPage = 1;
    this.syncUrl();
    this.loadData();
  }

  clearVendorType() { this.selectedVendorTypeId = null; this.closePanelAndReload(); }
  clearServiceType() { this.selectedServiceTypeId = null; this.closePanelAndReload(); }
  clearEventType() { this.selectedEventTypeId = null; this.closePanelAndReload(); }
  clearLocation() { this.selectedLocation = ''; this.closePanelAndReload(); }
  clearCity() { this.selectedCity = ''; this.closePanelAndReload(); }
  clearRating() { this.minRating = 0; this.closePanelAndReload(); }
  clearMaxPrice() { this.maxPrice = MAX_PRICE_ANY; this.closePanelAndReload(); }

  clearAllFilters() {
    this.activePanel = null;
    this.selectedVendorTypeId = null;
    this.selectedServiceTypeId = null;
    this.selectedEventTypeId = null;
    this.selectedLocation = '';
    this.selectedCity = '';
    this.minRating = 0;
    this.maxPrice = MAX_PRICE_ANY;
    this.searchQuery = '';
    this.currentPage = 1;
    this.syncUrl();
    this.loadData();
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.syncUrl();
    this.loadData();
  }

  private syncUrl() {
    const queryParams: Record<string, string | null> = {
      tab: this.activeTab === 'vendors' ? 'vendors' : null,
      q: this.searchQuery || null,
      page: this.currentPage > 1 ? String(this.currentPage) : null,
      type: null,
      category: null,
      serviceCategory: null,
      eventType: null,
      eventTypeId: null
    };

    if (this.activeTab === 'vendors' && this.selectedVendorTypeId) {
      queryParams['type'] = this.vendorTypeName(this.selectedVendorTypeId);
    }
    if (this.activeTab === 'services' && this.selectedServiceTypeId) {
      queryParams['serviceCategory'] = this.serviceTypeName(this.selectedServiceTypeId);
    }
    if (this.selectedEventTypeId) {
      queryParams['eventTypeId'] = this.selectedEventTypeId;
    }

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  // ── Label helpers ──────────────────────────────────────────
  vendorTypeName(id: string | null): string {
    if (!id) return '';
    return this.vendorTypes.find(t => t.id === id)?.name ?? '';
  }

  serviceTypeName(id: string | null): string {
    if (!id) return '';
    return this.serviceCategories.find(t => t.id === id)?.name ?? '';
  }

  eventTypeName(id: string | null): string {
    if (!id) return '';
    return this.eventTypes.find(t => t.id === id)?.name ?? '';
  }

  private normalizeTaxonomyName(name?: string | null): string {
    return (name ?? '').toLowerCase();
  }

  private resolveVendorTypeId(value: string): string | null {
    if (!value) return null;
    if (this.isGuid(value)) return value;
    const needle = value.toLowerCase();
    const match = this.vendorTypes.find(t => {
      const name = this.normalizeTaxonomyName(t.name);
      return name === needle || name.includes(needle);
    });
    return match?.id ?? null;
  }

  private resolveServiceTypeId(value: string): string | null {
    if (!value) return null;
    if (this.isGuid(value)) return value;
    const normalized = value.toLowerCase() === 'decor' ? 'decoration' : value.toLowerCase();
    const match = this.serviceCategories.find(t => {
      const name = this.normalizeTaxonomyName(t.name);
      return name === normalized || name.includes(normalized) || normalized.includes(name);
    });
    return match?.id ?? null;
  }

  private resolveEventTypeId(value: string): string | null {
    if (!value) return null;
    if (this.isGuid(value)) return value;
    const needle = value.toLowerCase();
    const match = this.eventTypes.find(t => {
      const name = this.normalizeTaxonomyName(t.name);
      return name === needle || name.includes(needle) || needle.includes(name);
    });
    return match?.id ?? null;
  }

  private isGuid(value: string): boolean {
    return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value);
  }

  togglePanel(panel: string) {
    this.activePanel = this.activePanel === panel ? null : panel;
  }

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
          const lat = area.latitude || baseLat + Math.sin((i + ai) * 23) * 0.05;
          const lng = area.longitude || baseLng + Math.cos((i + ai) * 29) * 0.05;
          const icon = L.divIcon({
            className: 'custom-map-marker',
            html: `<div style="background:white;border-radius:50px;padding:6px 16px;box-shadow:0 4px 15px rgba(0,0,0,.1);font-weight:600;font-size:.85rem;display:flex;align-items:center;gap:8px;border:1.5px solid #e8e0ec;white-space:nowrap;cursor:pointer;">
              <i class="bi bi-shop" style="color:#c9a84c"></i><span style="color:#1e0e2c">${vendor.name}</span></div>`,
            iconSize: [200, 40], iconAnchor: [100, 20]
          });
          const m = L.marker([lat, lng], { icon }).addTo(this.markersLayer!);
          m.on('click', () => this.ngZone.run(() => this.router.navigate(['/vendor', vendor.id])));
        });
      });
    } else {
      this.filteredServices.forEach((svc, i) => {
        const areas = (svc as any).serviceAreas?.length ? (svc as any).serviceAreas : [{ latitude: 0, longitude: 0 }];
        areas.forEach((area: any, ai: number) => {
          const lat = area.latitude || baseLat + Math.sin((i + ai) * 13) * 0.05;
          const lng = area.longitude || baseLng + Math.cos((i + ai) * 17) * 0.05;
          const price = (svc.price || 0).toLocaleString() + ' EGP';
          const icon = L.divIcon({
            className: 'custom-map-marker',
            html: `<div style="background:white;border-radius:50px;padding:6px 16px;box-shadow:0 4px 15px rgba(0,0,0,.1);font-weight:600;font-size:.85rem;display:flex;align-items:center;gap:8px;border:1.5px solid #e8e0ec;white-space:nowrap;cursor:pointer;">
              <i class="bi bi-image" style="color:#c9a84c"></i><span style="color:#1e0e2c">${svc.name}</span><span style="color:#c9a84c">${price}</span></div>`,
            iconSize: [260, 40], iconAnchor: [130, 20]
          });
          const m = L.marker([lat, lng], { icon }).addTo(this.markersLayer!);
          m.on('click', () => this.ngZone.run(() => this.openPreview(svc)));
        });
      });
    }
  }

  openPreview(svc: ApiProduct) {
    this.selectedService = svc;
    this.previewImages = this.getPreviewImages(svc);
    this.activeImageIndex = 0;
    this.showPreview = true;
  }

  closePreview() { this.showPreview = false; this.selectedService = null; }

  bookService(svc: ApiProduct) {
    this.closePreview();
    this.modalService.open('service-detail', svc);
  }

  private getPreviewImages(svc: ApiProduct): string[] {
    if (svc.imageUrls?.length) return svc.imageUrls.filter(u => !!u);
    if (svc.imageUrl) return svc.imageUrl.split(',').map(s => s.trim()).filter(s => !!s);
    return [];
  }

  toggleWishlist(svc: ApiProduct, e: Event) {
    e.stopPropagation();
    const i = this.wishlist.indexOf(svc.id);
    if (i > -1) this.wishlist.splice(i, 1); else this.wishlist.push(svc.id);
  }

  isInWishlist(id: string) { return this.wishlist.includes(id); }

  addToCompare(svc: ApiProduct, e: Event) {
    e.stopPropagation();
    const result = this.compareService.toggleServiceCompare(svc);
    if (result.success) {
      this.toastService.show(
        result.added ? 'Added to comparison!' : 'Removed from comparison',
        'success'
      );
    } else {
      this.toastService.show(result.message || 'Cannot add more services', 'error');
    }
  }

  isInCompare(id: string) { return this.compareService.isServiceInCompare(id); }
  clearServiceCompare() { this.compareService.clearServiceCompare(); }
}
