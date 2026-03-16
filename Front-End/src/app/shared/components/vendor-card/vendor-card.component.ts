import { Component, Input, inject } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Vendor } from '../../types/vendor.interface';
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
  @Input({ required: true }) vendor!: Vendor;

  favoriteService = inject(FavoriteService);
  compareService = inject(CompareService);
  toastService = inject(ToastService);

  getGradient(id: number): string {
    return `linear-gradient(135deg, hsl(${id * 30 + 200}, 40%, 25%) 0%, hsl(${id * 30 + 220}, 45%, 30%) 100%)`;
  }

  onToggleFavorite(event: Event) {
    event.stopPropagation();
    this.favoriteService.toggleFavorite(this.vendor.id);
    const isFav = this.favoriteService.isFavorite(this.vendor.id);
    this.toastService.show(isFav ? '️ Saved to favorites!' : ' Removed from favorites', isFav ? 'success' : 'info');
  }

  onToggleCompare(event: Event) {
    event.stopPropagation();
    const result = this.compareService.toggleCompare(this.vendor);
    if (result.success) {
      this.toastService.show(result.added ? '️ Added to comparison!' : 'Removed from comparison', 'success');
    } else {
      this.toastService.show(result.message || 'Error adding to comparison', 'error');
    }
  }
}
