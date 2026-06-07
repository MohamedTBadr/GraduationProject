import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ModalService } from '../../../shared/services/modal.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

interface RevenuePoint { month?: string; label?: string; revenue: number; }
interface RecentOrder { id: string; vendorName?: string; amount: number; paymentStatus: string; createdAt: string; }

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  modalService = inject(ModalService);
  private http = inject(HttpClient);
  private toastService = inject(ToastService);

  loading = true;
  isOpeningReport = false;

  lifetimeRevenue = 0;
  currentMonthRevenue = 0;
  growthPercentage = 0;
  totalOrders = 0;
  activeVendors = 0;
  totalCustomers = 0;
  revenueHistory: RevenuePoint[] = [];
  recentOrders: RecentOrder[] = [];

  ngOnInit() {
    // /dashboard/stats has a missing DB view; executive-report returns equivalent data
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    this.http.post<any>(`${environment.apiUrl}/dashboard/executive-report`, {}, { headers }).subscribe({
      next: (res) => {
        const d = res?.value ?? res;
        const kpis    = d?.kpIs ?? d?.KPIs ?? d?.kpis ?? {};
        const metrics = d?.adminMetrics ?? d?.AdminMetrics ?? {};
        this.lifetimeRevenue    = kpis?.lifetimeRevenue    ?? kpis?.LifetimeRevenue    ?? 0;
        this.currentMonthRevenue= kpis?.currentMonthRevenue?? kpis?.CurrentMonthRevenue?? 0;
        this.growthPercentage   = kpis?.growthPercentage   ?? kpis?.GrowthPercentage   ?? 0;
        this.totalOrders        = metrics?.totalOrders     ?? metrics?.TotalOrders     ?? 0;
        this.activeVendors      = metrics?.activeVendors   ?? metrics?.ActiveVendors   ?? 0;
        this.totalCustomers     = metrics?.totalCustomers  ?? metrics?.TotalCustomers  ?? 0;
        const hist = d?.revenueHistory ?? d?.RevenueHistory ?? [];
        this.revenueHistory     = Array.isArray(hist) ? hist : [];
        const orders = d?.recentOrders ?? d?.RecentOrders ?? [];
        this.recentOrders = (Array.isArray(orders) ? orders : []).map((o: any) => ({
          id: o.orderId ?? o.OrderId ?? o.id ?? o.Id ?? '',
          vendorName: o.vendorName ?? o.VendorName ?? o.vendor ?? o.Vendor ?? 'Order',
          amount: o.amount ?? o.Amount ?? 0,
          paymentStatus: o.paymentStatus ?? o.PaymentStatus ?? 'Paid',
          createdAt: o.createdAt ?? o.CreatedAt ?? ''
        }));
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastService.show('Failed to load dashboard data.', 'error');
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

  statusPill(status: string): string {
    const s = (status ?? '').toLowerCase();
    if (s === 'paid' || s === 'confirmed') return 'ap-green';
    if (s === 'pending') return 'ap-amber';
    if (s === 'cancelled' || s === 'failed') return 'ap-red';
    return 'ap-amber';
  }

  openQuickActions() { this.modalService.open('quick-action'); }
  openAddVendor()    { this.modalService.open('add-vendor'); }
  openAddPackage()   { this.modalService.open('add-package'); }
  openScheduleReport(){ this.modalService.open('schedule-report'); }

  openExecutiveReportPdf() {
    const reportTab = window.open('about:blank', '_blank');
    if (!reportTab) {
      this.toastService.show('Please allow popups to open the PDF report.', 'error');
      return;
    }

    reportTab.document.write('<p style="font-family:Arial,sans-serif;padding:24px">Generating report PDF...</p>');
    this.isOpeningReport = true;
    this.toastService.show('Generating report PDF...', 'info');

    this.http.get(`${environment.apiUrl}/reports/executive/pdf`, {
      responseType: 'blob'
    }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        reportTab.location.href = url;
        setTimeout(() => window.URL.revokeObjectURL(url), 60_000);
        this.isOpeningReport = false;
        this.toastService.show('PDF report opened in a new tab.', 'success');
      },
      error: (err) => {
        console.error('Failed to open report PDF', err);
        reportTab.close();
        this.isOpeningReport = false;
        this.toastService.show('Failed to open PDF report.', 'error');
      }
    });
  }
}
