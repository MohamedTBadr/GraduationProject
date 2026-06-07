import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../../core/services/auth.service';
import { EventService } from '../../../core/services/event.service';
import { VendorService } from '../../../core/services/vendor.service';
import { EventResponseDto, ApiVendor } from '../../../shared/types/api.interfaces';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { environment } from '../../../../environments/environment';
import { normalizeVendorReport, VendorReportView } from '../../../shared/utils/vendor-report.normalizer';
import { VendorBookingsRefreshService } from '../../../core/services/vendor-bookings-refresh.service';

@Component({
  selector: 'app-vendor-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './vendor-dashboard.component.html',
  styleUrls: ['./vendor-dashboard.component.scss']
})
export class VendorDashboardComponent implements OnInit {
  vendorName: string = 'Vendor';
  vendorId: string | null = null;

  // Stats from vendor/events
  pendingRequests = signal(0);
  averageRating = signal(0);

  upcomingEvents = signal<EventResponseDto[]>([]);
  pendingEvents = signal<any[]>([]);

  // Analytics report from backend
  report: VendorReportView = this.emptyReport();
  loading = true;

  constructor(
    private authService: AuthService,
    private eventService: EventService,
    private vendorService: VendorService,
    private http: HttpClient,
    private toastService: ToastService,
    private bookingsRefresh: VendorBookingsRefreshService
  ) {}

  ngOnInit() {
    const user = this.authService.user();
    if (!user) {
      this.loading = false;
      return;
    }

    this.vendorName = user.name || 'Vendor';
    this.vendorId = user.id;
    this.loadDashboardData();
  }

  private loadDashboardData() {
    if (!this.vendorId) return;

    const headers = new HttpHeaders({ IdempotencyKey: crypto.randomUUID() });

    forkJoin({
      vendor: this.vendorService.getById(this.vendorId).pipe(catchError(() => of(null))),
      events: this.eventService.getForVendor(this.vendorId).pipe(catchError(() => of([]))),
      analytics: this.http
        .post<any>(`${environment.apiUrl}/Dashboard/vendor-report`, {}, { headers })
        .pipe(catchError(() => of(null)))
    }).subscribe({
      next: (data) => {
        this.processVendorStats(data.vendor, data.events as EventResponseDto[]);
        this.report = normalizeVendorReport(data.analytics) ?? this.emptyReport();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading dashboard data', err);
        this.toastService.show('Failed to load dashboard data.', 'error');
        this.loading = false;
      }
    });
  }

  private emptyReport(): VendorReportView {
    return {
      kpis: {
        lifetimeRevenue: 0,
        currentMonthRevenue: 0,
        lastMonthRevenue: 0,
        growthPercentage: 0,
        totalOrders: 0,
        averageOrderValue: 0,
        averageMonthlyRevenue: 0
      },
      insights: { bestMonth: null, worstMonth: null },
      topServices: [],
      revenueHistory: [],
      recentOrders: [],
      recommendations: []
    };
  }

  private normalizeId(id: string | null | undefined): string {
    return id == null ? '' : String(id).trim().toLowerCase();
  }

  private processVendorStats(vendor: ApiVendor | null, events: EventResponseDto[]) {
    this.averageRating.set(vendor?.rating || 0);

    let pendingCount = 0;
    const upcomingById = new Map<string, EventResponseDto>();
    const pending: any[] = [];
    const vendorIdNorm = this.normalizeId(this.vendorId);

    events.forEach(event => {
      event.eventItems?.forEach(item => {
        if (!vendorIdNorm || this.normalizeId(item.vendorId) === vendorIdNorm) {
          if (item.itemStatus === 'Approved' || item.itemStatus === 'Paid') {
            upcomingById.set(event.id, event);
          } else if (item.itemStatus === 'Pending') {
            pendingCount++;
            pending.push({
              client: event.userName,
              eventTitle: event.title,
              date: event.eventDate,
              value: item.price * item.quantity,
              itemId: item.id,
              eventId: event.id
            });
          }
        }
      });
    });

    this.pendingRequests.set(pendingCount);
    this.upcomingEvents.set([...upcomingById.values()].slice(0, 5));
    this.pendingEvents.set(pending.slice(0, 5));
  }

  getHistoryHeight(revenue: number): number {
    if (!this.report?.revenueHistory?.length) return 0;
    const maxRevenue = Math.max(...this.report.revenueHistory.map((h: any) => h.revenue));
    if (maxRevenue === 0) return 10;
    return Math.max(10, Math.round((revenue / maxRevenue) * 100));
  }

  acceptRequest(request: any) {
    this.eventService.approveItem(request.eventId, request.itemId, { approve: true }).subscribe({
      next: () => {
        this.toastService.show('Booking request accepted.', 'success');
        this.loadDashboardData();
        this.bookingsRefresh.notifyRefresh();
      },
      error: (err) => {
        console.error('Error accepting booking request', err);
        this.toastService.show('Failed to accept booking request.', 'error');
      }
    });
  }

  declineRequest(request: any) {
    this.eventService.approveItem(request.eventId, request.itemId, { approve: false, reason: 'Declined' }).subscribe({
      next: () => {
        this.toastService.show('Booking request declined.', 'info');
        this.loadDashboardData();
        this.bookingsRefresh.notifyRefresh();
      },
      error: (err) => {
        console.error('Error declining booking request', err);
        this.toastService.show('Failed to decline booking request.', 'error');
      }
    });
  }
}
