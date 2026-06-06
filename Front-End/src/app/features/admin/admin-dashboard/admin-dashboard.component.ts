import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ModalService } from '../../../shared/services/modal.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  modalService = inject(ModalService);
  private http = inject(HttpClient);
  private toastService = inject(ToastService);

  report: any = null;
  loading = true;
  isDownloadingPdf = false;
  isSendingEmail = false;

  ngOnInit() {
    this.loadReport();
  }

  loadReport() {
    this.loading = true;
    const headers = new HttpHeaders({ IdempotencyKey: crypto.randomUUID() });
    this.http.post<any>(`${environment.apiUrl}/dashboard/executive-report`, {}, { headers }).subscribe({
      next: (data) => {
        const d = data?.value ?? data;
        this.report = { ...d, kpis: d?.kpIs ?? d?.KPIs ?? d?.kpis ?? {} };
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.toastService.show('Failed to load dashboard data.', 'error');
      }
    });
  }

  getHistoryHeight(revenue: number): number {
    if (!this.report?.revenueHistory?.length) return 0;
    const maxRevenue = Math.max(...this.report.revenueHistory.map((h: any) => h.revenue));
    if (maxRevenue === 0) return 10;
    return Math.max(10, Math.round((revenue / maxRevenue) * 100));
  }

  formatCurrency(n: number): string {
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(1) + 'M';
    if (n >= 1_000) return (n / 1_000).toFixed(0) + 'K';
    return n.toString();
  }

  downloadPdf() {
    this.isDownloadingPdf = true;
    this.toastService.show('Generating report PDF...', 'info');

    this.http.get(`${environment.apiUrl}/reports/executive/pdf`, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `executive-report-${new Date().toISOString().substring(0, 7)}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.isDownloadingPdf = false;
        this.toastService.show('PDF downloaded successfully.', 'success');
      },
      error: (err) => {
        console.error('Failed to download PDF', err);
        this.toastService.show('Failed to download PDF report.', 'error');
        this.isDownloadingPdf = false;
      }
    });
  }

  emailReport() {
    this.isSendingEmail = true;
    this.toastService.show('Scheduling report email...', 'info');
    const headers = new HttpHeaders({ IdempotencyKey: crypto.randomUUID() });
    this.http.post<any>(`${environment.apiUrl}/reports/executive/send-email`, {}, { headers }).subscribe({
      next: (res) => {
        this.toastService.show(res.message || 'Report scheduled! Check your email shortly.', 'success');
        this.isSendingEmail = false;
      },
      error: (err) => {
        console.error('Failed to send email', err);
        this.toastService.show('Failed to schedule email report.', 'error');
        this.isSendingEmail = false;
      }
    });
  }

  openQuickActions() {
    this.modalService.open('quick-action');
  }
}
