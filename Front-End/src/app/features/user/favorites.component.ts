import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ToastService } from '../../shared/components/toast/toast.service';
import { FavoriteService } from '../../shared/services/favorite.service';
import { VendorService } from '../../core/services/vendor.service';
import { ApiVendor } from '../../shared/types/api.interfaces';

@Component({
  selector: 'app-favorites',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './favorites.component.html',
  styleUrl: './favorites.component.scss'
})
export class FavoritesComponent implements OnInit {
  favorites: ApiVendor[] = [];
  loading = false;

  private toastService = inject(ToastService);
  private favoriteService = inject(FavoriteService);
  private vendorService = inject(VendorService);

  ngOnInit() {
    this.loadFavorites();
  }

  loadFavorites() {
    const favoriteIds = this.favoriteService.getFavoriteIds();
    if (favoriteIds.length === 0) {
      this.favorites = [];
      return;
    }

    this.loading = true;
    this.vendorService.getAll().subscribe({
      next: (vendors) => {
        this.favorites = vendors.filter(v => favoriteIds.includes(v.id));
        this.loading = false;
      },
      error: (err) => {
        this.toastService.show('Failed to load favorites', 'error');
        this.loading = false;
        console.error('Error loading vendors for favorites:', err);
      }
    });
  }

  removeFromFavorites(id: string, event: Event) {
    event.stopPropagation();
    event.preventDefault();
    this.favoriteService.toggleFavorite(id);
    this.favorites = this.favorites.filter(v => v.id !== id);
    this.toastService.show('Removed from favorites', 'info');
  }

  clearFavorites() {
    const currentFavs = this.favoriteService.getFavoriteIds();
    currentFavs.forEach(id => this.favoriteService.toggleFavorite(id));
    this.favorites = [];
    this.toastService.show('All favorites cleared', 'info');
  }
}
