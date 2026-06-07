import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { PaymentService } from '../../../core/services/payment.service';
import { OrderService, OrderResponse } from '../../../core/services/order.service';
import { EventService } from '../../../core/services/event.service';
import { ProductService } from '../../../core/services/product.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { PaymobFreeResponse, EventItemResponseDto } from '../../../shared/types/api.interfaces';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import {
  approvedItems,
  itemLineTotal,
  itemStatusLabel,
  pendingApprovalItems
} from '../../../shared/utils/event-item.utils';

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
  eventItems: EventItemResponseDto[] = [];
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
    private eventService: EventService,
    private productService: ProductService,
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
          this.router.navigate(['/user/my-events'], { queryParams: { id: order.eventId } });
          return;
        }

        if (order.eventId) {
          this.eventService.getById(order.eventId).subscribe({
            next: (ev) => {
              this.enrichEventItems(ev.eventItems ?? []).subscribe(items => {
                this.eventItems = items;
              });
            },
            error: () => { this.eventItems = []; }
          });
        }
      },
      error: () => {
        this.loadingOrder = false;
        this.toastService.show('Could not load order details.', 'error');
        this.router.navigate(['/user/my-events']);
      }
    });
  }

  get payableItems(): EventItemResponseDto[] {
    return approvedItems(this.eventItems);
  }

  get pendingItems(): EventItemResponseDto[] {
    return pendingApprovalItems(this.eventItems);
  }

  itemLineTotal(item: EventItemResponseDto): number {
    return itemLineTotal(item);
  }

  itemStatusLabel(item: EventItemResponseDto): string {
    return itemStatusLabel(item);
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
          const target = res.redirectUrl || '/payment/success';
          this.router.navigate([target], {
            queryParams: { orderId: this.orderId, merchant_order_id: this.orderId }
          });
        }
      },
      error: () => {
        this.isLoading = false;
        this.toastService.show('Payment initialization failed. Please try again.', 'error');
      }
    });
  }

  goBack() {
    const eventId = this.order?.eventId;
    if (eventId) {
      this.router.navigate(['/user/my-events'], { queryParams: { id: eventId } });
    } else {
      this.router.navigate(['/user/my-events']);
    }
  }

  private enrichEventItems(items: EventItemResponseDto[]) {
    const needsFetch = items.filter(i => i.price <= 0 && i.serviceId);
    if (needsFetch.length === 0) {
      return of(items);
    }

    const uniqueServiceIds = [...new Set(needsFetch.map(i => i.serviceId!))];
    return forkJoin(
      uniqueServiceIds.map(serviceId =>
        this.productService.getById(serviceId).pipe(
          map(product => ({ serviceId, price: product.price ?? 0 })),
          catchError(() => of({ serviceId, price: 0 }))
        )
      )
    ).pipe(
      map(priceRows => {
        const priceByService = new Map(priceRows.map(r => [r.serviceId, r.price]));
        return items.map(item => ({
          ...item,
          price: item.price > 0 ? item.price : (priceByService.get(item.serviceId ?? '') ?? item.price)
        }));
      })
    );
  }
}
