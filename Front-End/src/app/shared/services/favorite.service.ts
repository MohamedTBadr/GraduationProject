import { Injectable, signal } from '@angular/core';

/**
 * Client-side favorites (localStorage). No server favorites API exists yet —
 * see Front-End/ON_HOLD.md for backend handoff when sync is needed.
 */
@Injectable({
  providedIn: 'root'
})
export class FavoriteService {
  private favorites = new Set<string>();
  favoritesCount = signal(0);

  constructor() {
    this.loadFavorites();
  }

  private loadFavorites() {
    const saved = localStorage.getItem('eventora_favorites');
    if (saved) {
      const ids = JSON.parse(saved);
      ids.forEach((id: string) => this.favorites.add(String(id)));
      this.favoritesCount.set(this.favorites.size);
    }
  }

  private saveFavorites() {
    localStorage.setItem('eventora_favorites', JSON.stringify(Array.from(this.favorites)));
    this.favoritesCount.set(this.favorites.size);
  }

  toggleFavorite(id: string) {
    if (this.favorites.has(id)) {
      this.favorites.delete(id);
    } else {
      this.favorites.add(id);
    }
    this.saveFavorites();
  }

  isFavorite(id: string): boolean {
    return this.favorites.has(id);
  }

  getFavoriteIds(): string[] {
    return Array.from(this.favorites);
  }
}
