import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class FavoriteService {
  private favorites = new Set<number>();
  favoritesCount = signal(0);

  constructor() {
    this.loadFavorites();
  }

  private loadFavorites() {
    const saved = localStorage.getItem('eventora_favorites');
    if (saved) {
      const ids = JSON.parse(saved);
      ids.forEach((id: number) => this.favorites.add(id));
      this.favoritesCount.set(this.favorites.size);
    }
  }

  private saveFavorites() {
    localStorage.setItem('eventora_favorites', JSON.stringify(Array.from(this.favorites)));
    this.favoritesCount.set(this.favorites.size);
  }

  toggleFavorite(id: number) {
    if (this.favorites.has(id)) {
      this.favorites.delete(id);
    } else {
      this.favorites.add(id);
    }
    this.saveFavorites();
  }

  isFavorite(id: number): boolean {
    return this.favorites.has(id);
  }

  getFavoriteIds(): number[] {
    return Array.from(this.favorites);
  }
}
