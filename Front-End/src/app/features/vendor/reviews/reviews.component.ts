import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { VendorService } from '../../../core/services/vendor.service';
import { VendorRatingDto } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reviews.component.html',
  styleUrl: './reviews.component.scss'
})
export class ReviewsComponent implements OnInit {
  private authService = inject(AuthService);
  private vendorService = inject(VendorService);
  private toastService = inject(ToastService);

  loading = true;
  averageRating = 0;
  reviews: VendorRatingDto[] = [];

  ngOnInit() {
    const vendorId = this.authService.user()?.id;
    if (!vendorId) {
      this.loading = false;
      return;
    }

    this.vendorService.getDetailsById(vendorId).subscribe({
      next: (details) => {
        this.averageRating = details.rating ?? 0;
        this.reviews = details.vendorRatings ?? [];
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastService.show('Failed to load reviews.', 'error');
      }
    });
  }
}
