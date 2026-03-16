import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-favorites',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './favorites.component.html',
  styleUrls: ['./favorites.component.scss']
})
export class FavoritesComponent {

  mockFavorites = [
    { id: 1, name: 'White Rose Decor', category: 'Decoration', location: 'New Cairo', rating: 4.9, price: 5000, icon: '' },
    { id: 3, name: 'Studio Lens', category: 'Photography', location: 'Giza', rating: 4.9, price: 3000, icon: '' }
  ];

  constructor(private toastService: ToastService) { }

  removeFromFavorites(id: number, event: Event) {
    event.stopPropagation();
    this.mockFavorites = this.mockFavorites.filter(v => v.id !== id);
    this.toastService.show('Removed from favorites', 'info');
  }

  clearFavorites() {
    this.mockFavorites = [];
    this.toastService.show('All favorites cleared', 'info');
  }
}
