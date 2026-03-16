import { Component, OnInit, Inject, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FavoriteService } from '../../../shared/services/favorite.service';
import { ModalService } from '../../../shared/services/modal.service';
import { MOCK_VENDORS } from '../../../shared/data/mock-vendors.data';
import { Vendor } from '../../../shared/types/vendor.interface';

@Component({
  selector: 'app-vendor-profile',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './vendor-profile.component.html',
  styleUrls: ['./vendor-profile.component.scss']
})
export class VendorProfileComponent implements OnInit {
  activeTab = 'about';
  vendorId: number | null = null;
  vendor: Vendor | undefined;

  favoriteService = inject(FavoriteService);
  modalService = inject(ModalService);

  constructor(
    private route: ActivatedRoute,
    private toastService: ToastService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { }

  ngOnInit() {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.vendorId = parseInt(idParam, 10);
      this.vendor = MOCK_VENDORS.find(v => v.id === this.vendorId);
    }
  }

  toggleFav() {
    if (this.vendor) {
      this.favoriteService.toggleFavorite(this.vendor.id);
      const isFav = this.favoriteService.isFavorite(this.vendor.id);
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

  openServiceModal(service: any) {
    this.modalService.open('service-detail', service);
  }

  openPackageModal(pkg: any) {
    this.modalService.open('service-detail', pkg);
  }

  sendInquiry() {
    this.toastService.show('Inquiry sent! The vendor will reply soon.', 'success');
  }
}
