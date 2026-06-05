import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  report: any = null;
  loading = true;
  isDownloadingPdf = false;
  isSendingEmail = false;

  constructor(
    private http: HttpClient,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadReport();
  }

  loadReport() {
    this.loading = true;
    // /reports/executive has an unregistered DI service on the backend;
    // /dashboard/executive-report returns equivalent data
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
    this.http.post<any>(`${environment.apiUrl}/dashboard/executive-report`, {}, { headers }).subscribe({
      next: (data) => {
        const d = data?.value ?? data;
        // Normalize kpIs -> kpis for template compatibility
        this.report = { ...d, kpis: d?.kpIs ?? d?.KPIs ?? d?.kpis ?? {} };
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load executive report', err);
        this.toastService.show('Failed to load executive report data.', 'error');
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

  downloadPdf() {
    this.isDownloadingPdf = true;
    this.toastService.show('Generating report PDF...', 'info');
    
    this.http.get(`${environment.apiUrl}/reports/executive/pdf`, {
      responseType: 'blob'
    }).subscribe({
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
    const headers = new HttpHeaders({ 'IdempotencyKey': crypto.randomUUID() });
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
}
