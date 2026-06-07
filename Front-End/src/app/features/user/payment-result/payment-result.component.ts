import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { OrderService, OrderResponse } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-payment-result',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './payment-result.component.html',
  styleUrls: ['./payment-result.component.scss']
})
export class PaymentResultComponent implements OnInit {
  success = false;
  loading = true;
  order: OrderResponse | null = null;
  orderId = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    const params = this.route.snapshot.queryParamMap;

    // Paymob redirects to /payment-result?success=true/false&merchant_order_id=<our-guid>
    this.success = params.get('success') === 'true';
    this.orderId =
      params.get('merchant_order_id') ??
      params.get('order_id') ??
      '';

    // Only fetch order details if user is still logged in — avoids auth interceptor
    // logging out the user when the token has expired during payment
    if (!this.orderId || !this.authService.isLoggedIn()) {
      this.loading = false;
      return;
    }

    this.orderService.getOrderById(this.orderId).subscribe({
      next: (order) => {
        this.order = order;
        // Trust backend status over Paymob redirect param
        this.success = order.paymentStatus === 'Paid' || order.paymentStatus === 'Completed';
        this.loading = false;
      },
      error: () => {
        // Show result from query param — don't navigate away
        this.loading = false;
      }
    });
  }

  retry() {
    if (this.orderId) {
      this.router.navigate(['/checkout', this.orderId]);
    } else {
      this.router.navigate(['/user/my-events']);
    }
  }

  goToBookings() {
    this.router.navigate(['/user/my-events']);
  }
}
