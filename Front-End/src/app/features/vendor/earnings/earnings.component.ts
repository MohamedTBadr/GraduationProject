import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ToastService } from '../../../shared/components/toast/toast.service';

interface RevenuePoint { month: string; revenue: number; }

@Component({
  selector: 'app-earnings',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './earnings.component.html',
  styleUrl: './earnings.component.scss'
})
export class EarningsComponent implements OnInit {
  private http = inject(HttpClient);
  private toastService = inject(ToastService);

  loading = true;

  lifetimeRevenue     = 0;
  currentMonthRevenue = 0;
  growthPercentage    = 0;
  totalOrders         = 0;
  commission          = 0;
  netEarned           = 0;
  revenueHistory: RevenuePoint[] = [];
  recentOrders: any[] = [];

  private readonly COMMISSION_RATE = 0.1;

  ngOnInit() { this.loadReport(); }

  loadReport() {
    this.loading = true;
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    this.http.post<any>(`${environment.apiUrl}/Dashboard/vendor-report`, {}, { headers }).subscribe({
      next: (res) => {
        const d = res?.value ?? res;
        // Backend nests KPIs under kpIs; totalOrders is also inside kpIs
        const kpis = d?.kpIs ?? d?.KPIs ?? d?.kpis ?? {};
        this.lifetimeRevenue     = kpis?.lifetimeRevenue     ?? kpis?.LifetimeRevenue     ?? 0;
        this.currentMonthRevenue = kpis?.currentMonthRevenue ?? kpis?.CurrentMonthRevenue ?? 0;
        this.growthPercentage    = kpis?.growthPercentage    ?? kpis?.GrowthPercentage    ?? 0;
        this.totalOrders         = kpis?.totalOrders         ?? kpis?.TotalOrders         ?? 0;
        this.commission          = Math.round(this.lifetimeRevenue * this.COMMISSION_RATE);
        this.netEarned           = this.lifetimeRevenue - this.commission;
        const hist = d?.revenueHistory ?? d?.RevenueHistory ?? [];
        this.revenueHistory      = Array.isArray(hist) ? hist : [];
        const orders = d?.recentOrders ?? d?.RecentOrders ?? [];
        this.recentOrders        = Array.isArray(orders) ? orders : [];
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load earnings data', 'error');
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

  formatK(n: number): string {
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(1) + 'M';
    if (n >= 1_000) return (n / 1_000).toFixed(0) + 'K';
    return n.toString();
  }
}
