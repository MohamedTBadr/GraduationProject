import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reviews.component.html',
  styleUrl: './reviews.component.scss'
})
export class ReviewsComponent {
  // Backend GET /api/vendor/{id}/ratings endpoint is not yet implemented.
  // Reviews can be submitted by users (POST /api/Review) but cannot be retrieved per-vendor yet.
  readonly backendPending = true;
}
