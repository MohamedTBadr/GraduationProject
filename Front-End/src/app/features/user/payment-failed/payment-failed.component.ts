import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-payment-failed',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './payment-failed.component.html',
  styleUrls: ['./payment-failed.component.scss']
})
export class PaymentFailedComponent implements OnInit {
  // TODO: update param names once backend dev confirms Paymob redirect params
  orderId = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    const params = this.route.snapshot.queryParamMap;
    this.orderId =
      params.get('merchant_order_id') ??
      params.get('orderId') ??
      params.get('order_id') ??
      '';
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
