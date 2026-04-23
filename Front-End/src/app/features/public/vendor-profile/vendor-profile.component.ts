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
  activeTab = 'about';
  vendorId: string | null = null;
  vendor: ApiVendor | undefined;
  products: ApiProduct[] = [];
  loading = false;

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
    this.vendorId = this.route.snapshot.paramMap.get('id');
    if (this.vendorId) {
      this.loadVendorProfile();
      this.loadVendorProducts();
    }
  }

  loadVendorProfile() {
    if (!this.vendorId) return;
    this.loading = true;
    this.vendorService.getById(this.vendorId).subscribe({
      next: (data) => {
        this.vendor = data;
        this.loading = false;
      },
      error: (err) => {
        this.toastService.show('Failed to load profile details.', 'error');
        this.loading = false;
      }
    });
  }

  loadVendorProducts() {
    if (!this.vendorId) return;
    this.productService.getByVendor(this.vendorId).subscribe({
      next: (data) => {
        this.products = data;
      },
      error: (err) => {
        console.error('Failed to load vendor products', err);
        this.toastService.show('Failed to load vendor products.', 'error');
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
