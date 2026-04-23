import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';
import { EventService } from '../../../core/services/event.service';
import { EventResponseDto } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { PaymentService } from '../../../core/services/payment.service';
import { EventStudioComponent } from '../event-studio/event-studio.component';

@Component({
  selector: 'app-my-events',
  standalone: true,
  imports: [CommonModule, RouterLink, EventStudioComponent],
  templateUrl: './my-events.component.html',
  styleUrls: ['./my-events.component.scss']
})
export class MyEventsComponent implements OnInit {
  events: any[] = [];
  activeEventId: string | null = null;
  loading = true;
  showAiStudio = false;
  isPaying = false;

  constructor(
    private route: ActivatedRoute, 
    private router: Router,
    private eventService: EventService,
    private authService: AuthService,
    private toastService: ToastService,
    private paymentService: PaymentService
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
        if(params['id']) {
            this.activeEventId = params['id'];
        }
    });

    this.loadEvents();
  }

  loadEvents() {
    const user = this.authService.user();
    if (!user || user.role !== 'User') {
      this.loading = false;
      return;
    }

    this.eventService.getByUser(user.id).subscribe({
      next: (data: EventResponseDto[]) => {
        this.events = data.map(ev => this.mapEvent(ev));
        if (this.events.length > 0 && !this.events.find(e => e.id === this.activeEventId)) {
          this.activeEventId = this.events[0].id;
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

  mapEvent(ev: EventResponseDto): any {
    const mappedVendors = (ev.eventItems || []).map(item => ({
      emoji: '🏪', 
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
      status: ev.eventStatus || 'Pending'
    };
  }

  get activeEvent() {
    return this.events.find(e => e.id === this.activeEventId);
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
    this.loadEvents();
  }

  payDeposit() {
    if (!this.activeEvent || this.spent === 0) return;
    
    this.isPaying = true;
    const user = this.authService.user();
    
    // Deposit is 25% of total spent for the event items
    const depositAmount = this.spent * 0.25;

    const nameParts = user?.name ? user.name.split(' ') : ['User', 'Name'];

    this.paymentService.initiatePaymob({
      amount: depositAmount,
      billing: {
        first_name: nameParts[0] || 'User',
        last_name: nameParts[1] || 'Name',
        email: user?.email || 'test@example.com',
        phone_number: '+201234567890'
      }
    }).subscribe({
      next: (res) => {
        this.isPaying = false;
        // The user selected to open the URL in a new tab
        window.open(res.iframeUrl, '_blank');
      },
      error: (err) => {
        this.isPaying = false;
        this.toastService.show('Failed to initialize payment gateway.', 'error');
        console.error(err);
      }
    });
  }
}
