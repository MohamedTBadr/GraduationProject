import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { OrderService, OrderResponse } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './payment-success.component.html',
  styleUrls: ['./payment-success.component.scss']
})
export class PaymentSuccessComponent implements OnInit {
  order: OrderResponse | null = null;
  loading = true;
  confirmed = false;

  // TODO: update these param names once backend dev confirms what Paymob appends
  private orderId = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService,
    private authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    const params = this.route.snapshot.queryParamMap;
    // Paymob typically sends merchant_order_id — fall back to orderId
    this.orderId =
      params.get('merchant_order_id') ??
      params.get('orderId') ??
      params.get('order_id') ??
      '';

    if (!this.orderId || !this.authService.isLoggedIn()) {
      this.loading = false;
      return;
    }

    this.orderService.getOrderById(this.orderId).subscribe({
      next: (order) => {
        this.order = order;
        this.confirmed = order.paymentStatus === 'Paid' || order.paymentStatus === 'Completed';
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastService.show('Could not load order details. Your payment may still have succeeded.', 'info');
      }
    });
  }

  goToBookings() {
    this.router.navigate(['/user/my-events']);
  }
}
