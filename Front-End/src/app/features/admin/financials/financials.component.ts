import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ToastService } from '../../../shared/components/toast/toast.service';

interface RevenuePoint { month?: string; label?: string; revenue: number; }

@Component({
  selector: 'app-financials',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './financials.component.html',
  styleUrls: ['./financials.component.scss']
})
export class FinancialsComponent implements OnInit {
  private http = inject(HttpClient);
  private toastService = inject(ToastService);

  loading = true;
  isDownloadingPdf = false;
  report: any = null;

  lifetimeRevenue       = 0;
  currentMonthRevenue   = 0;
  lastMonthRevenue      = 0;
  growthPercentage      = 0;
  revenueHistory: RevenuePoint[] = [];
  topServices: any[]    = [];
  recentOrders: any[]   = [];

  ngOnInit() { this.loadReport(); }

  loadReport() {
    this.loading = true;
    const reportHeaders = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    this.http.post<any>(`${environment.apiUrl}/dashboard/executive-report`, {}, { headers: reportHeaders }).subscribe({
      next: (res) => {
        const d = res?.value ?? res;
        this.report              = d;
        // Backend nests KPIs under kpIs property
        const kpis = d?.kpIs ?? d?.KPIs ?? d?.kpis ?? d;
        this.lifetimeRevenue     = kpis?.lifetimeRevenue     ?? kpis?.LifetimeRevenue     ?? 0;
        this.currentMonthRevenue = kpis?.currentMonthRevenue ?? kpis?.CurrentMonthRevenue ?? 0;
        this.lastMonthRevenue    = kpis?.lastMonthRevenue    ?? kpis?.LastMonthRevenue    ?? 0;
        this.growthPercentage    = kpis?.growthPercentage    ?? kpis?.GrowthPercentage    ?? 0;
        const hist = d?.revenueHistory ?? d?.RevenueHistory ?? [];
        this.revenueHistory      = Array.isArray(hist) ? hist : [];
        const top = d?.topServices ?? d?.TopServices ?? [];
        this.topServices         = Array.isArray(top) ? top : [];
        const orders = d?.recentOrders ?? d?.RecentOrders ?? [];
        this.recentOrders        = Array.isArray(orders) ? orders : [];
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load financial report', 'error');
        this.loading = false;
      }
    });
  }

  getBarHeight(revenue: number): number {
    if (!this.revenueHistory.length) return 10;
    const max = Math.max(...this.revenueHistory.map(h => h.revenue ?? 0));
    if (max === 0) return 10;
    return Math.max(8, Math.round((revenue / max) * 100));
  }

  formatCurrency(n: number): string {
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(1) + 'M';
    if (n >= 1_000) return (n / 1_000).toFixed(0) + 'K';
    return n.toString();
  }

  get executiveSummary(): string {
    const r = this.report;
    if (!r) return '';
    return r.executiveSummary ?? r.ExecutiveSummary ?? r.aiSummary ?? r.AiSummary ?? '';
  }

  downloadPdf() {
    const reportTab = window.open('about:blank', '_blank');
    if (!reportTab) {
      this.toastService.show('Please allow popups to open the PDF report.', 'error');
      return;
    }

    reportTab.document.write('<p style="font-family:Arial,sans-serif;padding:24px">Generating report PDF...</p>');
    this.isDownloadingPdf = true;
    this.toastService.show('Generating report PDF...', 'info');

    this.http.get(`${environment.apiUrl}/reports/executive/pdf`, {
      responseType: 'blob'
    }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        reportTab.location.href = url;
        setTimeout(() => window.URL.revokeObjectURL(url), 60_000);
        this.isDownloadingPdf = false;
        this.toastService.show('PDF report opened in a new tab.', 'success');
      },
      error: (err) => {
        console.error('Failed to download PDF', err);
        reportTab.close();
        this.toastService.show('Failed to download PDF report.', 'error');
        this.isDownloadingPdf = false;
      }
    });
  }

  sendEmail() {
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    this.http.post(`${environment.apiUrl}/reports/executive/send-email`, {}, { headers }).subscribe({
      next: () => this.toastService.show('Report sent to your email', 'success'),
      error: () => this.toastService.show('Failed to send report', 'error')
    });
  }
}
