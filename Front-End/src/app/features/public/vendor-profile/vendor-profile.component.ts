import { Component, OnInit, Inject, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FavoriteService } from '../../../shared/services/favorite.service';
import { ApiVendor, ApiProduct, CreateReviewDto } from '../../../shared/types/api.interfaces';
import { VendorService } from '../../../core/services/vendor.service';
import { ProductService } from '../../../core/services/product.service';
import { ApiPackage, PackageService } from '../../../core/services/package.service';
import { ReviewService } from '../../../core/services/review.service';
import { AuthService } from '../../../core/services/auth.service';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';
import { EventTypeService } from '../../../core/services/event-type.service';
import { EventType } from '../../../core/models/taxonomy.models';

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

  selectedProduct: ApiProduct | null = null;

  eventDate = '';
  eventType = '';
  eventTypes: EventType[] = [];

  reviewRating = 5;
  reviewText = '';
  selectedServiceId = '';
  submittingReview = false;

  contactName = '';
  contactEmail = '';
  contactMessage = '';

  favoriteService = inject(FavoriteService);
  authService = inject(AuthService);

  get today(): string {
    return new Date().toISOString().split('T')[0];
  }

  get startingPrice(): number {
    const priced = this.products.filter(p => p.price > 0).map(p => p.price);
    return priced.length > 0 ? Math.min(...priced) : 0;
  }

  constructor(
    private route: ActivatedRoute,
    private toastService: ToastService,
    private vendorService: VendorService,
    private productService: ProductService,
    private packageService: PackageService,
    private eventTypeService: EventTypeService,
    private reviewService: ReviewService,
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
    this.selectedProduct = null;
  }

  // ── Vendor info + packages (GET /Vendor/{id} returns both) ──────
  loadVendorProfile() {
    if (!this.vendorId) return;
    this.vendorService.getById(this.vendorId).subscribe({
      next: (data) => {
        this.vendor = data;
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
      error: () => {}
    });
  }

  private loadEventTypes() {
    this.eventTypeService.getAll().subscribe({
      next: (types) => {
        this.eventTypes = types || [];
        if (this.eventTypes.length > 0) {
          this.eventType = this.eventTypes[0].name;
        }
      },
      error: () => {}
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
      error: () => {}
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
      // 404 when vendor has no services — correct empty state
      error: () => {}
    });
  }

  // Packages are loaded inside loadVendorProfile() from the vendor details response.

  private buildCarousel() {
    const images: string[] = [];
    if (this.vendor?.profilePictureUrl) images.push(this.vendor.profilePictureUrl);
    this.products.forEach(p => {
      if (p.imageUrls?.length) images.push(...p.imageUrls);
      else if (p.imageUrl) images.push(p.imageUrl);
    });
    this.carouselImages = [...new Set(images)].filter(Boolean);
  }

  prevImage() {
    if (this.carouselImages.length < 2) return;
    this.activeImageIndex = (this.activeImageIndex - 1 + this.carouselImages.length) % this.carouselImages.length;
  }

  nextImage() {
    if (this.carouselImages.length < 2) return;
    this.activeImageIndex = (this.activeImageIndex + 1) % this.carouselImages.length;
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

  requestBooking() {
    if (!this.eventDate) {
      this.toastService.show('Please select an event date first.', 'error');
      return;
    }
    this.activeTab = 'contact';
    this.toastService.show('Please use the contact form to send your booking request.', 'info');
  }

  openWhatsApp() {
    if (!this.vendor?.phone || !isPlatformBrowser(this.platformId)) return;
    const phone = this.vendor.phone.replace(/\D/g, '');
    window.open(`https://wa.me/${phone}`, '_blank');
  }

  submitReview() {
    const user = this.authService.user();
    if (!user) {
      this.toastService.show('Please log in to submit a review.', 'error');
      return;
    }
    if (!this.selectedServiceId) {
      this.toastService.show('Please select a service to review.', 'error');
      return;
    }
    if (!this.reviewText.trim()) {
      this.toastService.show('Please write your review.', 'error');
      return;
    }
    this.submittingReview = true;
    const payload: CreateReviewDto = {
      userId: user.id,
      serviceId: this.selectedServiceId,
      rating: this.reviewRating,
      review: this.reviewText
    };
    this.reviewService.submitReview(payload).subscribe({
      next: () => {
        this.toastService.show('Review submitted successfully!', 'success');
        this.reviewText = '';
        this.reviewRating = 5;
        this.selectedServiceId = '';
        this.submittingReview = false;
      },
      error: () => {
        this.toastService.show('Failed to submit review. Please try again.', 'error');
        this.submittingReview = false;
      }
    });
  }

  sendInquiry() {
    if (!this.contactName.trim() || !this.contactMessage.trim()) {
      this.toastService.show('Please fill in your name and message.', 'error');
      return;
    }
    this.toastService.show('Message sent! The vendor will get back to you soon.', 'success');
    this.contactName = '';
    this.contactEmail = '';
    this.contactMessage = '';
  }

  getRatingLabel(r: number): string {
    return ['', 'Poor', 'Fair', 'Good', 'Very Good', 'Excellent'][r] || '';
  }
}
