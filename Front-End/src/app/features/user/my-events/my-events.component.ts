import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { EventService } from '../../../core/services/event.service';
import { EventItemResponseDto, EventResponseDto } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { OrderService, OrderResponse } from '../../../core/services/order.service';
import { EventStudioComponent } from '../event-studio/event-studio.component';

@Component({
  selector: 'app-my-events',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, EventStudioComponent],
  templateUrl: './my-events.component.html',
  styleUrls: ['./my-events.component.scss']
})
export class MyEventsComponent implements OnInit {
  events: any[] = [];
  activeEventId: string | null = null;
  loading = true;
  showAiStudio = false;
  isPaying = false;
  eventServicesTab: 'all' | 'approved' = 'all';

  /** All orders for this user — used to detect existing pending orders */
  private userOrders: OrderResponse[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private eventService: EventService,
    private authService: AuthService,
    private toastService: ToastService,
    private orderService: OrderService
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      if (params['id']) {
        this.activeEventId = this.normalizeId(params['id']);
      }
    });
    this.loadEvents();
  }

  private normalizeId(id: string | null | undefined): string | null {
    return id ? String(id).trim().toLowerCase() : null;
  }

  isActiveEvent(id: string | null | undefined): boolean {
    return this.sameId(id, this.activeEventId);
  }

  private sameId(a: string | null | undefined, b: string | null | undefined): boolean {
    const left = this.normalizeId(a);
    const right = this.normalizeId(b);
    return !!left && !!right && left === right;
  }

  loadEvents() {
    if (!this.authService.isLoggedIn()) {
      this.loading = false;
      return;
    }

    const user = this.authService.user();
    this.loading = true;

    this.eventService.getByUser().subscribe({
      next: (data: EventResponseDto[]) => {
        this.events = data.map(ev => this.mapEvent(ev));
        if (this.events.length > 0) {
          const requestedId = this.normalizeId(this.activeEventId);
          const match = requestedId
            ? this.events.find(e => this.sameId(e.id, requestedId))
            : null;

          this.activeEventId = match?.id ?? this.events[0].id;
          this.loadActiveEventDetails(this.activeEventId);
          // if (this.activeEventId) this.loadCollaborators(this.activeEventId);
        } else {
          this.activeEventId = null;
        }
        this.loading = false;

        if (user?.id) {
          this.orderService.getOrdersByUser(user.id).subscribe({
            next: orders => { this.userOrders = orders; },
            error: () => this.toastService.show('Failed to load your order history.', 'error')
          });
        }
      },
      error: (err) => {
        console.error('Failed to load events:', err);
        this.toastService.show('Failed to load your events.', 'error');
        this.loading = false;
      }
    });
  }

  loadActiveEventDetails(id: string | null) {
    const eventId = this.normalizeId(id);
    if (!eventId) return;

    this.eventService.getById(eventId).subscribe({
      next: (fullEvent) => {
        const mapped = this.mapEvent(fullEvent);
        const index = this.events.findIndex(e => this.sameId(e.id, eventId));
        if (index !== -1) {
          this.events = [
            ...this.events.slice(0, index),
            mapped,
            ...this.events.slice(index + 1)
          ];
        } else {
          this.events = [...this.events, mapped];
        }
        if (!this.activeEventId || !this.events.some(e => this.sameId(e.id, this.activeEventId))) {
          this.activeEventId = mapped.id;
        }
      },
      error: (err) => {
        console.error('Failed to load event details:', err);
        this.toastService.show('Failed to load event details.', 'error');
      }
    });
  }

  mapEvent(ev: EventResponseDto): any {
    const mappedVendors = (ev.eventItems || []).map(item => ({
      emoji: '',
      name: item.vendorName || 'Vendor',
      type: item.serviceName || 'Service',
      price: item.price || 0,
      status: item.itemStatus?.toLowerCase() || 'pending'
    }));

    const defaultChecklist = [
      { text: 'Book venue', done: mappedVendors.some(v => v.type.toLowerCase().includes('venue') && v.status === 'confirmed') },
      { text: 'Choose decor vendor', done: mappedVendors.some(v => v.type.toLowerCase().includes('decor') && v.status === 'confirmed') },
      { text: 'Setup guest list', done: false },
      { text: 'Send invitations', done: false }
    ];

    return {
      id: ev.id,
      name: ev.title || 'Untitled Event',
      date: ev.eventDate,
      type: ev.eventTypeName || 'General',
      guests: ev.guestCount || 0,
      budget: ev.totalBudget || 0,
      vendors: mappedVendors,
      checklist: defaultChecklist,
      status: ev.eventStatus || 'Pending',
      eventItems: ev.eventItems || []
    };
  }

  get activeEvent() {
    return this.events.find(e => this.sameId(e.id, this.activeEventId));
  }

  get displayedEventItems(): EventItemResponseDto[] {
    const items = this.activeEvent?.eventItems as EventItemResponseDto[] | undefined;
    if (!items?.length) return [];
    if (this.eventServicesTab === 'approved') {
      return items.filter(i => i.itemStatus === 'Approved');
    }
    return items;
  }

  // ─── Partial payment getters ───────────────────────────────────────────────

  /** Items the vendor has approved but the user hasn't paid yet. */
  get approvedItems(): EventItemResponseDto[] {
    return (this.activeEvent?.eventItems as EventItemResponseDto[] ?? [])
      .filter(i => i.itemStatus === 'Approved');
  }

  /** Items still waiting for vendor response. */
  get pendingItems(): EventItemResponseDto[] {
    return (this.activeEvent?.eventItems as EventItemResponseDto[] ?? [])
      .filter(i => i.itemStatus === 'Pending');
  }

  /** Items already paid in a previous payment round. */
  get paidItems(): EventItemResponseDto[] {
    return (this.activeEvent?.eventItems as EventItemResponseDto[] ?? [])
      .filter(i => i.itemStatus === 'Paid' || i.itemStatus === 'Done' || i.itemStatus === 'Completed');
  }

  /** Total for the currently-approved (unpaid) items. */
  get approvedAmount(): number {
    return this.approvedItems.reduce((sum, i) => sum + (i.price * (i.quantity ?? 1)), 0);
  }

  /** True when there is already an unpaid pending order for the active event. */
  get existingPendingOrder(): OrderResponse | null {
    if (!this.activeEventId) return null;
    return this.userOrders.find(
      o => this.sameId(o.eventId, this.activeEventId) && o.paymentStatus === 'Pending'
    ) ?? null;
  }

  // ──────────────────────────────────────────────────────────────────────────

  itemBadgeClass(item: EventItemResponseDto): string {
    const s = (item.itemStatus || '').toLowerCase();
    if (s === 'approved') return 'badge-confirmed';
    if (s === 'paid' || s === 'done' || s === 'completed') return 'badge-paid';
    if (s === 'rejected') return 'badge-rejected';
    return 'badge-pending';
  }

  itemStatusLabel(item: EventItemResponseDto): string {
    const s = item.itemStatus || '';
    if (s === 'Approved') return 'Approved — Ready to Pay';
    if (s === 'Paid') return 'Paid';
    if (s === 'Done') return 'Done';
    if (s === 'Completed') return 'Completed';
    if (s === 'Rejected') return 'Rejected';
    return 'Awaiting Confirmation';
  }

  get spent() {
    return this.activeEvent?.vendors.reduce(
      (sum: number, v: any) => sum + (v.status !== 'rejected' ? v.price : 0), 0
    ) || 0;
  }

  get budgetPct() {
    if (!this.activeEvent || this.activeEvent.budget === 0) return 0;
    return Math.min(100, Math.round((this.spent / this.activeEvent.budget) * 100));
  }

  get daysLeft() {
    if (!this.activeEvent?.date) return 0;
    const diff = new Date(this.activeEvent.date).getTime() - new Date().getTime();
    return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
  }

  switchEvent(id: string) {
    this.activeEventId = this.normalizeId(id);
    this.eventServicesTab = 'all';
    this.loadActiveEventDetails(this.activeEventId);
    // if (this.activeEventId) this.loadCollaborators(this.activeEventId);
  }

  toggleCheck(index: number) {
    if (this.activeEvent) {
      this.activeEvent.checklist[index].done = !this.activeEvent.checklist[index].done;
    }
  }

  openAddEvent() { this.router.navigate(['/add-event']); }
  openAiStudio() { this.showAiStudio = true; }

  payNow() {
    // If a pending order already exists for this event, resume it
    if (this.existingPendingOrder) {
      this.router.navigate(['/checkout', this.existingPendingOrder.id]);
      return;
    }

    if (this.approvedItems.length === 0) return;

    this.isPaying = true;
    const user = this.authService.user();

    const orderPayload = {
      userId: user?.id || '',
      eventId: this.activeEvent.id,
      currency: 'EGP',
      shippingAddress: { street: 'NA', city: 'Cairo', state: 'Cairo', postalCode: '12345' },
      appointment: new Date(this.activeEvent.date).toISOString()
    };

    this.orderService.createOrder(orderPayload).subscribe({
      next: (result) => {
        if (result?.id) {
          this.finishCheckout(result);
          return;
        }
        // Fallback when API succeeds but omits id in the response body
        const userId = user?.id;
        if (!userId) {
          this.isPaying = false;
          this.toastService.show('Order creation failed. Please try again.', 'error');
          return;
        }
        this.orderService.findLatestOrderForEvent(userId, this.activeEvent.id).subscribe({
          next: (fallback) => {
            if (fallback?.id) {
              this.finishCheckout(fallback);
            } else {
              this.isPaying = false;
              this.toastService.show('Order creation failed. Please try again.', 'error');
            }
          },
          error: () => {
            this.isPaying = false;
            this.toastService.show('Order creation failed. Please try again.', 'error');
          }
        });
      },
      error: (err) => {
        this.isPaying = false;
        this.toastService.show('Failed to create order.', 'error');
        console.error(err);
      }
    });
  }

  private finishCheckout(order: OrderResponse) {
    this.isPaying = false;
    if (!this.userOrders.some(o => o.id === order.id)) {
      this.userOrders = [...this.userOrders, order];
    }
    this.router.navigate(['/checkout', order.id]);
  }

  // ── Collaborators (on hold) ───────────────────────────────────────────────
  // collaborators: EventCollaboratorDto[] = [];
  // collaboratorInput = '';
  // collaboratorRole: 0 | 1 = 0;
  // collaboratorsLoading = false;
  //
  // loadCollaborators(eventId: string): void { ... }
  // addCollaborator(): void { ... }
  // removeCollaborator(userId: string): void { ... }
  // collaboratorRoleLabel(role: 0 | 1): string { ... }
  // ──────────────────────────────────────────────────────────────────────────

  onAiPlanAccepted(plan: any) {
    if (!this.activeEventId || !plan?.selected_items?.length) return;

    this.toastService.show('Adding recommended vendors to your event...', 'info');
    this.loading = true;

    let completedCount = 0;
    let errorCount = 0;
    const items = plan.selected_items;

    const processNext = (index: number) => {
      if (index >= items.length) {
        this.loading = false;
        if (errorCount === 0) {
          this.toastService.show('All vendors added successfully!', 'success');
        } else {
          this.toastService.show(`Added ${completedCount} vendors. ${errorCount} failed.`, 'info');
        }
        this.loadEvents();
        return;
      }

      const item = items[index];
      const serviceId = item.ServiceId || item.serviceId;

      if (!serviceId) { errorCount++; processNext(index + 1); return; }

      this.eventService.addItem(this.activeEventId!, { eventId: this.activeEventId!, serviceId, quantity: 1 }).subscribe({
        next: () => { completedCount++; processNext(index + 1); },
        error: (err) => { console.error(`Failed to add item ${serviceId}:`, err); errorCount++; processNext(index + 1); }
      });
    };

    processNext(0);
  }
}
