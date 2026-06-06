import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

interface PortfolioImage {
  url: string;
  serviceName: string;
}

@Component({
  selector: 'app-portfolio',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './portfolio.component.html',
  styleUrls: ['./portfolio.component.scss']
})
export class PortfolioComponent implements OnInit {
  private productService = inject(ProductService);
  private authService    = inject(AuthService);
  private toastService   = inject(ToastService);

  loading = true;
  images: PortfolioImage[] = [];
  selectedImage: PortfolioImage | null = null;
  vendorId = '';

  ngOnInit() {
    this.vendorId = this.authService.user()?.id ?? '';
    if (!this.vendorId) { this.loading = false; return; }

    this.productService.getByVendor(this.vendorId).subscribe({
      next: services => {
        this.images = services.flatMap(s =>
          (s.imageUrls ?? (s.imageUrl ? [s.imageUrl] : [])).map(url => ({
            url,
            serviceName: s.name
          }))
        );
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load portfolio', 'error');
        this.loading = false;
      }
    });
  }

  openLightbox(img: PortfolioImage) { this.selectedImage = img; }
  closeLightbox() { this.selectedImage = null; }
}
