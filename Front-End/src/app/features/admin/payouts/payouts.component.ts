import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-payouts',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './payouts.component.html',
  styleUrl: './payouts.component.scss'
})
export class PayoutsComponent {
  // No backend payout endpoints exist yet.
  // Required: GET/POST /api/payouts, PATCH /api/payouts/{id}/approve, etc.
}
