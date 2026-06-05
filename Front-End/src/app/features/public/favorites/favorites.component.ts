import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { FavoriteService } from '../../../shared/services/favorite.service';
import { VendorService } from '../../../core/services/vendor.service';
import { ApiVendor } from '../../../shared/types/api.interfaces';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-favorites',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.scss']
})
export class FavoritesComponent implements OnInit {
  favorites: ApiVendor[] = [];
  loading = false;
  pageNumber = 1;
  pageSize = 12;

  private toastService = inject(ToastService);
  private favoriteService = inject(FavoriteService);
  private vendorService = inject(VendorService);

  ngOnInit() {
    this.loadFavorites();
  }

  get paginatedFavorites(): ApiVendor[] {
    const start = (this.pageNumber - 1) * this.pageSize;
    return this.favorites.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.favorites.length / this.pageSize));
  }

  onPageChange(page: number) {
    this.pageNumber = page;
  }

  loadFavorites() {
    const favoriteIds = this.favoriteService.getFavoriteIds();
    if (favoriteIds.length === 0) {
      this.favorites = [];
      return;
    }

    this.loading = true;
    this.vendorService.getAll({ pageSize: 500, pageIndex: 1 }).subscribe({
      next: (vendors) => {
        this.favorites = vendors.filter(v => favoriteIds.includes(v.id));
        this.pageNumber = 1;
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load favorites', 'error');
        this.loading = false;
      }
    });
  }

  removeFromFavorites(id: string, event: Event) {
    event.stopPropagation();
    this.favoriteService.toggleFavorite(id);
    this.favorites = this.favorites.filter(v => v.id !== id);
    if (this.pageNumber > this.totalPages) this.pageNumber = this.totalPages;
    this.toastService.show('Removed from favorites', 'info');
  }

  clearFavorites() {
    const currentFavs = this.favoriteService.getFavoriteIds();
    currentFavs.forEach(id => this.favoriteService.toggleFavorite(id));
    this.favorites = [];
    this.pageNumber = 1;
    this.toastService.show('All favorites cleared', 'info');
  }
}
