import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { EventService } from '../../../core/services/event.service';
import { EventItemResponseDto, EventResponseDto } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { OrderService, OrderResponse } from '../../../core/services/order.service';
import { ProductService } from '../../../core/services/product.service';
import { forkJoin, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import {
  approvedAmount,
  approvedItems,
  budgetCommittedAmount,
  pendingApprovalItems
} from '../../../shared/utils/event-item.utils';

interface DashboardEvent {
  id: string;
  name: string;
  date: string;
  type: string;
  guests: number;
  daysLeft: number;
  vendorsConfirmed: number;
  totalVendors: number;
  vendorProgress: string;
  tasksDone: number;
  totalTasks: number;
  tasksProgress: string;
  budgetUsed: number;
  totalBudget: number;
  budgetProgress: string;
  eventItems: EventItemResponseDto[];
  approvedPayAmount: number;
  approvedPayCount: number;
  pendingPayCount: number;
  hasPendingOrder: boolean;
  canPay: boolean;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  stats = [
    { label: 'Active Events', value: '0', icon: 'calendar3' },
    { label: 'Booked Vendors', value: '0', icon: 'shop' },
    { label: 'Pending Requests', value: '0', icon: 'hourglass-split' },
    { label: 'Avg Budget Used', value: '0%', icon: 'currency-dollar' }
  ];

  events: DashboardEvent[] = [];
  loading = true;
  payingEventId: string | null = null;
  private userOrders: OrderResponse[] = [];

  constructor(
    private router: Router,
    private eventService: EventService,
    private authService: AuthService,
    private toastService: ToastService,
    private orderService: OrderService,
    private productService: ProductService
  ) {}

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    if (!this.authService.isLoggedIn()) {
      this.loading = false;
      return;
    }

    const user = this.authService.user();

    this.eventService.getByUser().pipe(
      switchMap((data: EventResponseDto[]) =>
        this.enrichEventsWithPrices(data).pipe(map(enriched => ({ data, enriched })))
      )
    ).subscribe({
      next: ({ data, enriched }) => {
        if (user?.id) {
          this.orderService.getOrdersByUser(user.id).subscribe({
            next: orders => {
              this.userOrders = orders;
              this.processEventsData(data, enriched);
              this.loading = false;
            },
            error: () => {
              this.processEventsData(data, enriched);
              this.loading = false;
            }
          });
        } else {
          this.processEventsData(data, enriched);
          this.loading = false;
        }
      },
      error: (err) => {
        console.error('Failed to load events', err);
        this.toastService.show('Failed to load dashboard data.', 'error');
        this.loading = false;
      }
    });
  }

  private enrichEventsWithPrices(events: EventResponseDto[]) {
    const itemsNeedingPrice = events.flatMap(ev =>
      (ev.eventItems ?? []).filter(i => i.price <= 0 && i.serviceId)
    );

    if (itemsNeedingPrice.length === 0) {
      return of(events);
    }

    const uniqueServiceIds = [...new Set(itemsNeedingPrice.map(i => i.serviceId!))];

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
        return events.map(ev => ({
          ...ev,
          eventItems: (ev.eventItems ?? []).map(item => ({
            ...item,
            price: item.price > 0 ? item.price : (priceByService.get(item.serviceId ?? '') ?? item.price)
          }))
        }));
      })
    );
  }

  processEventsData(data: EventResponseDto[], enriched: EventResponseDto[]) {
    let totalVendors = 0;
    let pendingVendors = 0;
    let totalBudgetSum = 0;
    let spentBudgetSum = 0;

    this.events = enriched.map(ev => {
      const today = new Date();
      const evDate = new Date(ev.eventDate);
      const diffTime = evDate.getTime() - today.getTime();
      const daysLeft = Math.max(0, Math.ceil(diffTime / (1000 * 60 * 60 * 24)));

      const items = ev.eventItems ?? [];
      const totalEvVendors = items.length;
      const confirmedVendors = items.filter(i => i.itemStatus === 'Approved' || i.itemStatus === 'Paid').length;
      const pendingEvVendors = pendingApprovalItems(items).length;

      totalVendors += confirmedVendors;
      pendingVendors += pendingEvVendors;

      const budgetUsed = budgetCommittedAmount(items);
      totalBudgetSum += ev.totalBudget || 0;
      spentBudgetSum += budgetUsed;

      const approved = approvedItems(items);
      const hasPendingOrder = this.userOrders.some(
        o => o.eventId === ev.id && o.paymentStatus === 'Pending'
      );

      return {
        id: ev.id,
        name: ev.title || 'Untitled Event',
        date: ev.eventDate,
        type: ev.eventTypeName || 'General',
        guests: ev.guestCount || 0,
        daysLeft,
        vendorsConfirmed: confirmedVendors,
        totalVendors: totalEvVendors,
        vendorProgress: totalEvVendors > 0 ? `${confirmedVendors}/${totalEvVendors} Confirmed` : 'No vendors',
        tasksDone: confirmedVendors,
        totalTasks: totalEvVendors > 0 ? totalEvVendors + 2 : 2,
        tasksProgress: 'Mock Tasks',
        budgetUsed,
        totalBudget: ev.totalBudget || 1,
        budgetProgress: ev.totalBudget ? `${Math.round((budgetUsed / ev.totalBudget) * 100)}%` : '0%',
        eventItems: items,
        approvedPayAmount: approvedAmount(items),
        approvedPayCount: approved.length,
        pendingPayCount: pendingApprovalItems(items).length,
        hasPendingOrder,
        canPay: hasPendingOrder || approved.length > 0
      };
    });

    this.stats[0].value = data.length.toString();
    this.stats[1].value = totalVendors.toString();
    this.stats[2].value = pendingVendors.toString();
    this.stats[3].value = totalBudgetSum > 0 ? `${Math.round((spentBudgetSum / totalBudgetSum) * 100)}%` : '0%';
  }

  goToEvent(id: string) {
    this.router.navigate(['/user/my-events'], { queryParams: { id } });
  }

  payNow(ev: DashboardEvent, event: MouseEvent) {
    event.stopPropagation();

    const pendingOrder = this.userOrders.find(
      o => o.eventId === ev.id && o.paymentStatus === 'Pending'
    );
    if (pendingOrder) {
      this.router.navigate(['/checkout', pendingOrder.id]);
      return;
    }

    if (ev.approvedPayCount === 0) {
      this.toastService.show('No approved services to pay for yet.', 'info');
      return;
    }

    const user = this.authService.user();
    if (!user?.id) return;

    this.payingEventId = ev.id;

    this.eventService.getById(ev.id).subscribe({
      next: (fullEvent) => {
        const orderPayload = {
          userId: user.id,
          eventId: ev.id,
          currency: 'EGP',
          shippingAddress: this.getEventShippingAddress(fullEvent.location),
          appointment: new Date(fullEvent.eventDate).toISOString()
        };

        this.orderService.createOrder(orderPayload).subscribe({
          next: (result) => {
            this.payingEventId = null;
            if (result?.id) {
              this.router.navigate(['/checkout', result.id]);
              return;
            }
            this.orderService.findLatestOrderForEvent(user.id, ev.id).subscribe({
              next: (fallback) => {
                if (fallback?.id) {
                  this.router.navigate(['/checkout', fallback.id]);
                } else {
                  this.toastService.show('Order creation failed. Please try again.', 'error');
                }
              },
              error: () => this.toastService.show('Order creation failed. Please try again.', 'error')
            });
          },
          error: () => {
            this.payingEventId = null;
            this.toastService.show('Failed to create order.', 'error');
          }
        });
      },
      error: () => {
        this.payingEventId = null;
        this.toastService.show('Could not load event details.', 'error');
      }
    });
  }

  private getEventShippingAddress(location: EventResponseDto['location']) {
    if (!location?.city) {
      return { street: '', city: 'Cairo', state: 'Cairo', postalCode: '' };
    }
    return {
      street: location.street || '',
      city: location.city,
      state: location.state || location.city,
      postalCode: location.postalCode || ''
    };
  }

  isPaying(ev: DashboardEvent): boolean {
    return this.payingEventId === ev.id;
  }
}
