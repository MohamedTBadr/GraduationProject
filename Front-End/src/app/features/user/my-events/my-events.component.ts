import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { EventService } from '../../../core/services/event.service';
import { EventItemResponseDto, EventResponseDto } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { PaymentService } from '../../../core/services/payment.service';
import { EventStudioComponent } from '../event-studio/event-studio.component';
import { ProductService } from '../../../core/services/product.service';
import { OrderService } from '../../../core/services/order.service';
import { VoucherService } from '../../../core/services/voucher.service';

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
  /** Sub-tab under event detail: all line items vs vendor-approved only */
  eventServicesTab: 'all' | 'approved' = 'all';

  // Voucher / discount state
  voucherCode = '';
  appliedVoucherCode = '';
  appliedDiscountPercent = 0;
  isValidatingVoucher = false;

  constructor(
    private route: ActivatedRoute, 
    private router: Router,
    private eventService: EventService,
    private authService: AuthService,
    private toastService: ToastService,
    private paymentService: PaymentService,
    private productService: ProductService,
    private orderService: OrderService,
    private voucherService: VoucherService
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
        if(params['id']) {
            this.activeEventId = params['id'];
            this.loadActiveEventDetails(this.activeEventId);
        }
    });

    this.loadEvents();
  }

  loadEvents() {
    const user = this.authService.user();
    if (!user || (user.role !== 'User' && (user.role as any) !== 'Customer')) {
      this.loading = false;
      return;
    }

    this.eventService.getByUser(user.id).subscribe({
      next: (data: EventResponseDto[]) => {
        this.events = data.map(ev => this.mapEvent(ev));
        if (this.events.length > 0) {
          if (!this.activeEventId || !this.events.find(e => e.id === this.activeEventId)) {
            this.activeEventId = this.events[0].id;
          }
          this.loadActiveEventDetails(this.activeEventId);
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load events:', err);
        this.toastService.show('Failed to load your events.', 'error');
        this.loading = false;
      }
    });
  }

  loadActiveEventDetails(id: string | null) {
    if (!id) return;
    this.eventService.getById(id).subscribe({
      next: (res: any) => {
        const fullEvent = res?.value ?? res;
        const index = this.events.findIndex(e => e.id === id);
        if (index !== -1) {
          this.events[index] = this.mapEvent(fullEvent);
        }
      },
      error: (err) => {
        console.error('Failed to load event details:', err);
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
    return this.events.find(e => e.id === this.activeEventId);
  }

  /** Line items for the active event, filtered by {@link eventServicesTab}. */
  get displayedEventItems(): EventItemResponseDto[] {
    const items = this.activeEvent?.eventItems as EventItemResponseDto[] | undefined;
    if (!items?.length) return [];
    if (this.eventServicesTab === 'approved') {
      return items.filter(i => i.itemStatus === 'Approved');
    }
    return items;
  }

  get hasApprovedServices(): boolean {
    const items = this.activeEvent?.eventItems as EventItemResponseDto[] | undefined;
    return !!items?.some(i => i.itemStatus === 'Approved');
  }

  itemBadgeClass(item: EventItemResponseDto): string {
    const s = (item.itemStatus || '').toLowerCase();
    if (s === 'approved' || s === 'done' || s === 'completed') return 'badge-confirmed';
    if (s === 'rejected') return 'badge-rejected';
    return 'badge-pending';
  }

  get spent() {
    return this.activeEvent?.vendors.reduce((sum: number, v: any) => sum + (v.status !== 'rejected' ? v.price : 0), 0) || 0;
  }

  get budgetPct() {
    if (!this.activeEvent || this.activeEvent.budget === 0) return 0;
    return Math.min(100, Math.round((this.spent / this.activeEvent.budget) * 100));
  }

  get daysLeft() {
    if (!this.activeEvent || !this.activeEvent.date) return 0;
    const diff = new Date(this.activeEvent.date).getTime() - new Date().getTime();
    return Math.max(0, Math.ceil(diff / (1000 * 60 * 60 * 24)));
  }

  switchEvent(id: string) {
    this.activeEventId = id;
    this.eventServicesTab = 'all';
    this.loadActiveEventDetails(id);
  }

  toggleCheck(index: number) {
    if (this.activeEvent) {
      this.activeEvent.checklist[index].done = !this.activeEvent.checklist[index].done;
    }
  }

  openAddEvent() {
    this.router.navigate(['/add-event']);
  }

  openAiStudio() {
    this.showAiStudio = true;
  }

  onAiPlanAccepted(plan: any) {
    if (!this.activeEventId || !plan || !plan.selected_items || plan.selected_items.length === 0) {
      return;
    }

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

      if (!serviceId) {
        errorCount++;
        processNext(index + 1);
        return;
      }

      this.productService.getById(serviceId).subscribe({
        next: (product) => {
          const payload = {
            eventId: this.activeEventId!,
            serviceImage: product.imageUrl || '',
            serviceName: product.name,
            price: product.price,
            vendorId: product.vendorId || '',
            vendorName: product.vendorName || item.vendor || '',
            quantity: 1
          };

          this.eventService.addItem(this.activeEventId!, payload).subscribe({
            next: () => {
              completedCount++;
              processNext(index + 1);
            },
            error: (err) => {
              console.error(`Failed to add item ${serviceId}:`, err);
              errorCount++;
              processNext(index + 1);
            }
          });
        },
        error: (err) => {
          console.error(`Failed to fetch service ${serviceId}:`, err);
          errorCount++;
          processNext(index + 1);
        }
      });
    };

    processNext(0);
  }

  applyVoucher() {
    if (!this.voucherCode.trim()) return;
    this.isValidatingVoucher = true;
    this.voucherService.validateVoucher(this.voucherCode.trim()).subscribe({
      next: (res) => {
        this.isValidatingVoucher = false;
        if (res.isValid && res.discountPercent) {
          this.appliedVoucherCode = this.voucherCode.trim();
          this.appliedDiscountPercent = res.discountPercent;
          this.toastService.show(`${res.discountPercent}% discount applied!`, 'success');
        } else {
          this.appliedVoucherCode = '';
          this.appliedDiscountPercent = 0;
          this.toastService.show(res.errorMessage || 'Invalid or expired voucher.', 'error');
        }
      },
      error: () => {
        this.isValidatingVoucher = false;
        this.toastService.show('Could not validate voucher. Try again.', 'error');
      }
    });
  }

  removeVoucher() {
    this.appliedVoucherCode = '';
    this.appliedDiscountPercent = 0;
    this.voucherCode = '';
    this.toastService.show('Voucher removed.', 'info');
  }

  get discountedAmount(): number {
    if (this.appliedDiscountPercent === 0) return this.spent * 0.25;
    const full = this.spent * 0.25;
    return full - (full * this.appliedDiscountPercent / 100);
  }

  payDeposit() {
    if (!this.activeEvent || this.spent === 0 || !this.hasApprovedServices) return;
    
    this.isPaying = true;
    const user = this.authService.user();
    
    const nameParts = user?.name ? user.name.split(' ') : ['User', 'Name'];

    const orderPayload = {
      userId: user?.id || '',
      eventId: this.activeEvent.id,
      currency: 'EGP',
      voucherCode: this.appliedVoucherCode || undefined,
      shippingAddress: {
        street: 'Default Street',
        city: 'Cairo',
        state: 'Cairo',
        postalCode: '12345'
      },
      appointment: new Date(this.activeEvent.date).toISOString()
    };

    this.orderService.createOrder(orderPayload).subscribe({
      next: (result) => {
        if (!result.isSuccess || !result.value?.id) {
          this.isPaying = false;
          this.toastService.show('Order creation failed. Please try again.', 'error');
          return;
        }
        this.paymentService.initiatePaymob({
          amount: this.discountedAmount,
          billing: {
            first_name: nameParts[0] || 'User',
            last_name: nameParts[1] || 'Name',
            email: user?.email || 'test@example.com',
            phone_number: '+201234567890'
          },
          orderId: result.value.id,
          voucherCode: this.appliedVoucherCode || undefined
        }).subscribe({
          next: (res) => {
            this.isPaying = false;
            window.open(res.iframeUrl, '_blank');
          },
          error: (err) => {
            this.isPaying = false;
            this.toastService.show('Failed to initialize payment gateway.', 'error');
            console.error(err);
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
}


