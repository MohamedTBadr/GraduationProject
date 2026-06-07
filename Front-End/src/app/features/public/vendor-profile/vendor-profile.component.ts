import { Component, OnInit, Inject, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FavoriteService } from '../../../shared/services/favorite.service';
import { ApiVendor, ApiProduct, VendorRatingDto } from '../../../shared/types/api.interfaces';
import { VendorService } from '../../../core/services/vendor.service';
import { ProductService } from '../../../core/services/product.service';
import { ApiPackage, PackageService } from '../../../core/services/package.service';
import { AuthService } from '../../../core/services/auth.service';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';
import { EventTypeService } from '../../../core/services/event-type.service';
import { EventType } from '../../../core/models/taxonomy.models';
import { ModalService } from '../../../shared/services/modal.service';
import { ChatLaunchService } from '../../../core/services/chat-launch.service';
import { formatVendorLocation } from '../../../shared/utils/location.utils';
import { cssBackgroundImage, getProductImageUrls, getServiceImagesFromRaw } from '../../../shared/utils/image.utils';
import { VendorDetails } from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-vendor-profile',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, VendorCardComponent],
  templateUrl: './vendor-profile.component.html',
  styleUrls: ['./vendor-profile.component.scss']
})
export class VendorProfileComponent implements OnInit {
  activeTab = 'services';
  vendorId: string | null = null;
  vendor: ApiVendor | null = null;

  // Services and packages loaded independently from their own endpoints
  products: ApiProduct[] = [];
  packages: ApiPackage[] = [];

  similarVendors: ApiVendor[] = [];
  loading = true;
  error = false;

  carouselImages: string[] = [];
  activeImageIndex = 0;
  private embeddedServices: any[] = [];

  vendorReviews: VendorRatingDto[] = [];

  contactMessage = '';
  inquiryEventDate = '';
  inquiryEventType = '';
  inquiryGuestCount: number | null = null;
  eventTypes: EventType[] = [];
  selectedPackage: ApiPackage | null = null;
  startingChat = false;

  favoriteService = inject(FavoriteService);
  authService = inject(AuthService);
  private modalService = inject(ModalService);
  private chatLaunchService = inject(ChatLaunchService);

  get today(): string {
    return new Date().toISOString().split('T')[0];
  }

  get startingPrice(): number {
    const priced = this.products.filter(p => p.price > 0).map(p => p.price);
    return priced.length > 0 ? Math.min(...priced) : 0;
  }

  get locationLabel(): string {
    return formatVendorLocation(this.vendor);
  }

  get messagePlaceholder(): string {
    if (this.selectedPackage) {
      return `Hi, I'm interested in the "${this.selectedPackage.name}" package. Tell us about your event...`;
    }
    return 'Describe your event and what you need...';
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private toastService: ToastService,
    private vendorService: VendorService,
    private productService: ProductService,
    private packageService: PackageService,
    private eventTypeService: EventTypeService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  ngOnInit() {
    this.loadEventTypes();
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.vendorId = id;
        this.resetState();
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
    this.packages = [];
    this.similarVendors = [];
    this.carouselImages = [];
    this.activeImageIndex = 0;
    this.embeddedServices = [];
  }

  // ── Vendor info + packages (GET /Vendor/{id} returns both) ──────
  loadVendorProfile() {
    if (!this.vendorId) return;
    this.vendorService.getDetailsById(this.vendorId).subscribe({
      next: (data: VendorDetails) => {
        this.vendor = data;
        this.vendorReviews = data.vendorRatings ?? [];
        this.embeddedServices = data.embeddedServices ?? [];
        this.loading = false;

        this.loadVendorPackages();
        this.buildCarousel();
        this.loadSimilarVendors();
      },
      error: () => {
        this.loading = false;
        this.error = true;
        this.toastService.show('Failed to load vendor profile.', 'error');
      }
    });
  }

  private loadVendorPackages() {
    if (!this.vendorId) return;
    this.packageService.getByVendor(this.vendorId).subscribe({
      next: (data) => {
        this.packages = data || [];
      },
      error: () => this.toastService.show('Failed to load vendor packages.', 'error')
    });
  }

  private loadEventTypes() {
    this.eventTypeService.getAll().subscribe({
      next: (types) => {
        this.eventTypes = types || [];
        if (this.eventTypes.length > 0 && !this.inquiryEventType) {
          this.inquiryEventType = this.eventTypes[0].name;
        }
      },
      error: () => this.toastService.show('Failed to load event types.', 'error')
    });
  }

  private loadSimilarVendors() {
    const filters = this.vendor?.vendorTypeId
      ? { vendorTypeId: this.vendor.vendorTypeId, pageSize: 5 }
      : { pageSize: 5 };
    this.vendorService.getAll(filters).subscribe({
      next: (vendors) => {
        this.similarVendors = vendors
          .filter(v => v.id !== this.vendorId && v.id !== this.vendor?.id)
          .slice(0, 4);
      },
      error: () => this.toastService.show('Failed to load similar vendors.', 'error')
    });
  }

  // ── Services via GET /Service/by-vendor/{id} ─────────────────
  loadVendorProducts() {
    if (!this.vendorId) return;
    this.productService.getByVendor(this.vendorId).subscribe({
      next: (data) => {
        this.products = data || [];
        this.buildCarousel();
      },
      error: (err) => {
        if (err?.status !== 404) {
          this.toastService.show('Failed to load vendor services.', 'error');
        }
      }
    });
  }

  // Packages are loaded inside loadVendorProfile() from the vendor details response.

  private buildCarousel() {
    const images: string[] = [];
    if (this.vendor?.profilePictureUrl) images.push(this.vendor.profilePictureUrl);
    this.embeddedServices.forEach(s => images.push(...getServiceImagesFromRaw(s)));
    this.products.forEach(p => images.push(...getProductImageUrls(p)));
    this.carouselImages = [...new Set(images)].filter(Boolean);
    if (this.activeImageIndex >= this.carouselImages.length) {
      this.activeImageIndex = 0;
    }
  }

  heroBackground(url: string): string {
    return cssBackgroundImage(url);
  }

  goToImage(index: number) {
    if (index < 0 || index >= this.carouselImages.length) return;
    this.activeImageIndex = index;
  }

  prevImage() {
    if (this.carouselImages.length < 2) return;
    this.goToImage((this.activeImageIndex - 1 + this.carouselImages.length) % this.carouselImages.length);
  }

  nextImage() {
    if (this.carouselImages.length < 2) return;
    this.goToImage((this.activeImageIndex + 1) % this.carouselImages.length);
  }

  isFavorite(): boolean {
    const id = this.vendor?.id || this.vendorId;
    return !!id && this.favoriteService.isFavorite(id);
  }

  toggleFav() {
    const id = this.vendor?.id || this.vendorId;
    if (!id) return;
    this.favoriteService.toggleFavorite(id);
    const isFav = this.isFavorite();
    this.toastService.show(isFav ? 'Added to favorites' : 'Removed from favorites', isFav ? 'success' : 'info');
  }

  inquireAboutPackage(pkg: ApiPackage) {
    this.selectedPackage = pkg;
    this.contactMessage = '';
    this.inquiryEventDate = '';
    this.inquiryGuestCount = null;
    this.activeTab = 'contact';
  }

  private buildChatMessage(): string {
    const lines: string[] = [];
    const title = this.selectedPackage ? 'Package Inquiry' : 'Event Inquiry';

    lines.push(title);
    lines.push('');

    if (this.selectedPackage) {
      lines.push(`Package: ${this.selectedPackage.name}`);
    }
    if (this.inquiryEventType) {
      lines.push(`Event type: ${this.inquiryEventType}`);
    }
    if (this.inquiryEventDate) {
      lines.push(`Event date: ${new Date(this.inquiryEventDate).toLocaleDateString('en-GB', {
        day: 'numeric', month: 'long', year: 'numeric'
      })}`);
    }
    if (this.inquiryGuestCount && this.inquiryGuestCount > 0) {
      lines.push(`Guests: ${this.inquiryGuestCount}`);
    }

    const message = this.contactMessage.trim();
    if (message) {
      lines.push('');
      lines.push('Message:');
      lines.push(message);
    }

    return lines.join('\n');
  }

  startVendorChat() {
    const message = this.contactMessage.trim();
    if (!message) {
      this.toastService.show('Please enter a message.', 'error');
      return;
    }

    if (!this.authService.isLoggedIn()) {
      this.modalService.open('login');
      return;
    }

    const vendorId = this.vendor?.id || this.vendorId;
    const vendorName = this.vendor?.name;
    if (!vendorId) {
      this.toastService.show('Vendor contact information is not available.', 'error');
      return;
    }

    const fullMessage = this.buildChatMessage();

    this.chatLaunchService.setPending(vendorId, vendorName, fullMessage);

    this.startingChat = true;
    this.router.navigate(['/user/messages'], {
      queryParams: {
        vendorId,
        vendorName,
      },
    }).finally(() => {
      this.startingChat = false;
      this.contactMessage = '';
      this.inquiryEventDate = '';
      this.inquiryGuestCount = null;
      this.selectedPackage = null;
    });
  }

  openWhatsApp() {
    if (!this.vendor?.phone || !isPlatformBrowser(this.platformId)) return;
    const phone = this.vendor.phone.replace(/\D/g, '');
    window.open(`https://wa.me/${phone}`, '_blank');
  }

  openServiceBooking(product: ApiProduct) {
    this.modalService.open('service-detail', {
      ...product,
      vendorId: product.vendorId || this.vendorId || this.vendor?.id,
      vendorName: product.vendorName || this.vendor?.name
    });
  }
}
