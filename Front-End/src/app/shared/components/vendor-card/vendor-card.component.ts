import { Component, Input, inject } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiVendor } from '../../types/api.interfaces';
import { formatVendorLocation } from '../../utils/location.utils';
import { FavoriteService } from '../../services/favorite.service';
import { CompareService } from '../../services/compare.service';
import { ToastService } from '../toast/toast.service';

@Component({
  selector: 'app-vendor-card',
  standalone: true,
  imports: [CommonModule, RouterLink, DecimalPipe],
  templateUrl: './vendor-card.component.html',
  styleUrls: ['./vendor-card.component.scss']
})
export class VendorCardComponent {
  @Input({ required: true }) vendor!: ApiVendor;

  favoriteService = inject(FavoriteService);
  compareService = inject(CompareService);
  toastService = inject(ToastService);

  getGradient(id: string): string {
    const numId = parseInt(id, 16) % 10 || 1; // Basic hash for variety
    return `linear-gradient(135deg, hsl(${numId * 30 + 200}, 40%, 25%) 0%, hsl(${numId * 30 + 220}, 45%, 30%) 100%)`;
  }

  isFavorite(): boolean {
    return this.favoriteService.isFavorite(this.vendor.id);
  }

  onToggleFavorite(event: Event) {
    event.stopPropagation();
    this.favoriteService.toggleFavorite(this.vendor.id);
    const isFav = this.isFavorite();
    this.toastService.show(isFav ? 'Saved to favorites!' : 'Removed from favorites', isFav ? 'success' : 'info');
  }

  get locationLabel(): string {
    return formatVendorLocation(this.vendor);
  }

  onToggleCompare(event: Event) {
    event.stopPropagation();
    const result = this.compareService.toggleCompare(this.vendor);
    if (result.success) {
      this.toastService.show(result.added ? 'Added to comparison!' : 'Removed from comparison', 'success');
    } else {
      this.toastService.show(result.message || 'Error adding to comparison', 'error');
    }
  }
}
