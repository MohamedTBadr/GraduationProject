import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { environment } from '../../../../environments/environment';
import { normalizeVendorReport, VendorReportView } from '../../../shared/utils/vendor-report.normalizer';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.scss']
})
export class AnalyticsComponent implements OnInit {
  report: VendorReportView | null = null;

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
  loading = true;

  constructor(
    private http: HttpClient,
    private toastService: ToastService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadAnalytics();
  }

  loadAnalytics() {
    this.loading = true;
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    this.http.post<any>(`${environment.apiUrl}/Dashboard/vendor-report`, {}, { headers }).subscribe({
      next: (data) => {
        this.report = normalizeVendorReport(data) ?? this.emptyReport();
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load vendor analytics', err);
        this.toastService.show('Failed to load vendor analytics data.', 'error');
        this.loading = false;
      }
    });
  }

  getHistoryHeight(revenue: number): number {
    if (!this.report || !this.report.revenueHistory || this.report.revenueHistory.length === 0) return 0;
    const maxRevenue = Math.max(...this.report.revenueHistory.map((h: any) => h.revenue));
    if (maxRevenue === 0) return 10;
    return Math.max(10, Math.round((revenue / maxRevenue) * 100));
  }
}
