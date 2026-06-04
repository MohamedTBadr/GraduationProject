import { Component, OnInit, Inject, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FavoriteService } from '../../../shared/services/favorite.service';
import { ApiVendor, ApiProduct, CreateReviewDto } from '../../../shared/types/api.interfaces';
import { VendorService } from '../../../core/services/vendor.service';
import { ProductService } from '../../../core/services/product.service';
import { PackageService, ApiPackage } from '../../../core/services/package.service';
import { ReviewService } from '../../../core/services/review.service';
import { AuthService } from '../../../core/services/auth.service';
import { VendorCardComponent } from '../../../shared/components/vendor-card/vendor-card.component';

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
  eventType = 'Wedding';

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
    private reviewService: ReviewService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  ngOnInit() {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id) {
        this.vendorId = id;
        this.resetState();
        this.loadVendorProfile();
        this.loadVendorProducts();
        this.loadVendorPackages();
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
  // GET /Package?vendorId= is auth-restricted (vendors only see own packages),
  // but GET /Vendor/{id} returns packages for anyone — use it as the source.
  loadVendorProfile() {
    if (!this.vendorId) return;
    this.vendorService.getById(this.vendorId).subscribe({
      next: (data) => {
        this.vendor = data;
        this.loading = false;

        // VendorDetailsDTO includes Packages — PascalCase because the /Vendor/{id}
        // controller returns result.Value directly (not wrapped in Result<T>).
        const raw = data as any;
        const rawPkgs: any[] = raw.Packages || raw.packages || [];
        this.packages = rawPkgs.map((p: any) => ({
          id: p.Id || p.id || '',
          name: p.Name || p.name || '',
          description: p.Description || p.description || '',
          price: +(p.Price ?? p.price ?? 0),
          discount: +(p.Discount ?? p.discount ?? 0),
          services: p.Services || p.services || [],
          vendorId: p.VendorId || p.vendorId || ''
        }));

        this.buildCarousel();
      },
      error: () => {
        this.loading = false;
        this.error = true;
        this.toastService.show('Failed to load vendor profile.', 'error');
      }
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

  // ── Package fallback: only runs when the logged-in user IS the vendor ───
  // Keeps the call alive so the vendor can see their own packages if the
  // vendor-details response somehow doesn't include them.
  loadVendorPackages() {
    if (!this.vendorId || this.packages.length > 0) return;
    this.packageService.getByVendor(this.vendorId).subscribe({
      next: (data) => { if (data?.length) this.packages = data; },
      error: () => {}
    });
  }

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
