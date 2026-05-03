import { Component, OnInit, Inject, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FavoriteService } from '../../../shared/services/favorite.service';
import { ModalService } from '../../../shared/services/modal.service';
import { ApiVendor, ApiProduct } from '../../../shared/types/api.interfaces';
import { VendorService } from '../../../core/services/vendor.service';
import { ProductService } from '../../../core/services/product.service';

@Component({
  selector: 'app-vendor-profile',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './vendor-profile.component.html',
  styleUrls: ['./vendor-profile.component.scss']
})
export class VendorProfileComponent implements OnInit {
  activeTab = 'services';
  vendorId: string | null = null;
  vendor: ApiVendor | null = null;
  products: ApiProduct[] = [];
  loading = true;
  error = false;

  favoriteService = inject(FavoriteService);
  modalService = inject(ModalService);

  constructor(
    private route: ActivatedRoute,
    private toastService: ToastService,
    private vendorService: VendorService,
    private productService: ProductService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id && id !== this.vendorId) {
        this.vendorId = id;
        this.resetState();
        this.loadVendorProfile();
        this.loadVendorProducts();
      } else if (id && !this.vendorId) {
        this.vendorId = id;
        this.loadVendorProfile();
        this.loadVendorProducts();
      }
    });
  }

  private resetState() {
    this.loading = true;
    this.error = false;
    this.vendor = null;
    this.products = [];
  }

  loadVendorProfile() {
    if (!this.vendorId) return;
    this.loading = true;
    this.error = false;
    this.vendorService.getById(this.vendorId).subscribe({
      next: (data) => {
        this.vendor = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading vendor profile:', err);
        this.toastService.show('Failed to load profile details. Please try again later.', 'error');
        this.loading = false;
        this.error = true;
      }
    });
  }

  loadVendorProducts() {
    if (!this.vendorId) return;
    this.productService.getByVendor(this.vendorId).subscribe({
      next: (data) => {
        this.products = data || [];
      },
      error: (err) => {
        console.error('Error loading vendor products:', err);
      }
    });
  }

  isFavorite(): boolean {
    if (!this.vendor?.id) return false;
    return this.favoriteService.isFavorite(this.vendor.id);
  }

  toggleFav() {
    if (this.vendor?.id) {
      this.favoriteService.toggleFavorite(this.vendor.id);
      const isFav = this.isFavorite();
      this.toastService.show(isFav ? 'Added to favorites' : 'Removed from favorites', isFav ? 'success' : 'info');
    }
  }

  scrollToServices() {
    this.activeTab = 'services';
    if (isPlatformBrowser(this.platformId)) {
      setTimeout(() => {
        document.getElementById('vp-services')?.scrollIntoView({ behavior: 'smooth' });
      }, 100);
    }
  }

  openServiceModal(product: ApiProduct) {
    this.modalService.open('service-detail', product);
  }

  sendInquiry() {
    this.toastService.show('Inquiry sent! The vendor will reply soon.', 'success');
  }
}
