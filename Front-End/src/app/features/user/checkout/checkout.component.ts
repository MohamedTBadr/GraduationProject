import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { PaymentService } from '../../../core/services/payment.service';
import { OrderService, OrderResponse } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { PaymobFreeResponse } from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss']
})
export class CheckoutComponent implements OnInit {
  orderId = '';
  order: OrderResponse | null = null;
  iframeUrl: SafeResourceUrl | null = null;
  isLoading = false;
  loadingOrder = true;
  userName = '';
  userEmail = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private sanitizer: DomSanitizer,
    private paymentService: PaymentService,
    private orderService: OrderService,
    private authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.orderId = this.route.snapshot.paramMap.get('orderId') ?? '';

    const user = this.authService.user();
    if (user) {
      this.userName = user.name ?? '';
      this.userEmail = user.email ?? '';
    }

    this.orderService.getOrderById(this.orderId).subscribe({
      next: (order) => {
        this.order = order;
        this.loadingOrder = false;

        if (order.paymentStatus === 'Paid' || order.paymentStatus === 'Completed') {
          this.toastService.show('This order is already paid.', 'info');
          this.router.navigate(['/user/bookings']);
        }
      },
      error: () => {
        this.loadingOrder = false;
        this.toastService.show('Could not load order details.', 'error');
        this.router.navigate(['/user/bookings']);
      }
    });
  }

  proceed() {
    if (!this.order || this.order.amount <= 0) {
      this.toastService.show('Invalid order amount. Please contact support.', 'error');
      return;
    }

    this.isLoading = true;

    this.paymentService.initiatePaymob({ orderId: this.orderId }).subscribe({
      next: (res: string | PaymobFreeResponse) => {
        this.isLoading = false;
        if (typeof res === 'string') {
          this.iframeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(res);
          setTimeout(() => {
            document.getElementById('paymob-iframe')?.scrollIntoView({ behavior: 'smooth' });
          }, 100);
        } else if (res.isFree) {
          this.toastService.show(res.message || 'Order is free!', 'success');
          this.router.navigate([res.redirectUrl || '/payment/success']);
        }
      },
      error: () => {
        this.isLoading = false;
        this.toastService.show('Payment initialization failed. Please try again.', 'error');
      }
    });
  }

  goBack() {
    this.router.navigate(['/user/bookings']);
  }
}
