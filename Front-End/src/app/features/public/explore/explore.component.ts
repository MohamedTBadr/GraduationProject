import { Component, OnInit, OnDestroy, AfterViewInit, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';
import { ApiVendor } from '../../../shared/types/api.interfaces';
import { VendorService } from '../../../core/services/vendor.service';
import * as L from 'leaflet';

@Component({
  selector: 'app-explore',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, VendorCardComponent],
  templateUrl: './explore.component.html',
  styleUrls: ['./explore.component.scss']
})
export class ExploreComponent implements OnInit, OnDestroy, AfterViewInit {
  activePanel: string | null = null;
  sortOption = 'rating';
  vendorCount = 0;
  loading = false;
  viewMode: 'grid' | 'map' = 'grid';

  // Map State
  private map: L.Map | undefined;
  private markersLayer: L.LayerGroup | undefined;

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

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private vendorService: VendorService,
    private ngZone: NgZone
  ) { }

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

  ngAfterViewInit() {
    // Map will be initialized when viewMode changes to 'map'
  }

  ngOnDestroy() {
    if (this.map) {
      this.map.remove();
    }
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
    this.updateMapMarkers();
  }

  setView(mode: 'grid' | 'map') {
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
      this.map = L.map('vendor-map', {
        zoomControl: false // Optional: hide default zoom controls
      }).setView([30.0444, 31.2357], 11); // Cairo coordinates

      // Add zoom control manually to right-bottom
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

    this.displayVendors.forEach((vendor, index) => {
      const areas = vendor.serviceAreas && vendor.serviceAreas.length > 0 ? vendor.serviceAreas : [{ latitude: 0, longitude: 0 }];

      areas.forEach((area, areaIdx) => {
        let lat = area.latitude;
        let lng = area.longitude;

        // If no real coordinates, fallback to pseudo-random around Cairo
        if (lat === 0 && lng === 0) {
          const latOffset = (Math.sin((index + areaIdx) * 23) * 0.05);
          const lngOffset = (Math.cos((index + areaIdx) * 29) * 0.05);
          lat = baseLat + latOffset;
          lng = baseLng + lngOffset;
        }

        const markerIcon = vendor.profilePictureUrl
          ? '<i class="bi bi-shop" style="font-size:1rem;color:#c9a84c"></i>'
          : '<i class="bi bi-geo-alt-fill" style="font-size:1rem;color:#c9a84c"></i>';

        const customIcon = L.divIcon({
          className: 'custom-map-marker',
          html: `<div style="background: white; border-radius: 50px; padding: 6px 16px; box-shadow: 0 4px 15px rgba(0,0,0,0.1); font-weight: 600; font-size: 0.85rem; display: flex; align-items: center; gap: 8px; border: 1.5px solid #e8e4dc; white-space: nowrap; cursor: pointer; transition: transform 0.2s ease, border-color 0.2s ease;" onmouseover="this.style.transform='scale(1.05)'; this.style.borderColor='#c9a84c'" onmouseout="this.style.transform='scale(1)'; this.style.borderColor='#e8e4dc'">
                   ${markerIcon}
                   <span style="color: #1a2540;">${vendor.name}</span>
                 </div>`,
          iconSize: [220, 40],
          iconAnchor: [110, 20]
        });

        const marker = L.marker([lat, lng], { icon: customIcon }).addTo(this.markersLayer!);
        marker.on('click', () => {
          this.ngZone.run(() => {
            this.router.navigate(['/vendor', vendor.id]);
          });
        });
      });
    });
  }
}
