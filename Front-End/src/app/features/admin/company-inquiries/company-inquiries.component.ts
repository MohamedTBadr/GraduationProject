import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CompanyInquiryService, CompanyInquiryResponse } from '../../../core/services/company-inquiry.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-company-inquiries',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './company-inquiries.component.html',
  styleUrl: './company-inquiries.component.scss'
})
export class CompanyInquiriesComponent implements OnInit {
  inquiries: CompanyInquiryResponse[] = [];
  totalCount = 0;
  pageNumber = 1;
  pageSize = 15;
  loading = false;

  statusFilter: 'All' | 'Pending' | 'Reviewed' | 'Closed' = 'All';

  selectedInquiry: CompanyInquiryResponse | null = null;
  isModalOpen = false;

  readonly statusTabs: ('All' | 'Pending' | 'Reviewed' | 'Closed')[] = ['All', 'Pending', 'Reviewed', 'Closed'];
  readonly statuses: ('Pending' | 'Reviewed' | 'Closed')[] = ['Pending', 'Reviewed', 'Closed'];

  constructor(
    private inquiryService: CompanyInquiryService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadInquiries();
  }

  loadInquiries(): void {
    this.loading = true;
    this.inquiryService.getAll(this.pageNumber, this.pageSize).subscribe({
      next: ({ items, totalCount }) => {
        this.inquiries = items;
        this.totalCount = totalCount;
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load inquiries.', 'error');
        this.loading = false;
      }
    });
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get filteredInquiries(): CompanyInquiryResponse[] {
    if (this.statusFilter === 'All') return this.inquiries;
    return this.inquiries.filter(i => i.status === this.statusFilter);
  }

  get pendingCount(): number {
    return this.inquiries.filter(i => i.status === 'Pending').length;
  }

  changePage(delta: number): void {
    this.pageNumber += delta;
    this.loadInquiries();
  }

  openDetails(inquiry: CompanyInquiryResponse): void {
    this.selectedInquiry = { ...inquiry };
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
    this.selectedInquiry = null;
  }

  updateStatus(inquiry: CompanyInquiryResponse, status: string): void {
    this.inquiryService.updateStatus(inquiry, status).subscribe({
      next: () => {
        inquiry.status = status as any;
        this.toastService.show(`Status updated to ${status}.`, 'success');
        if (this.selectedInquiry?.id === inquiry.id) {
          this.selectedInquiry.status = status as any;
        }
      },
      error: () => this.toastService.show('Failed to update status.', 'error')
    });
  }

  deleteInquiry(id: string): void {
    if (!confirm('Delete this inquiry?')) return;
    this.inquiryService.delete(id).subscribe({
      next: () => {
        this.inquiries = this.inquiries.filter(i => i.id !== id);
        this.totalCount = Math.max(0, this.totalCount - 1);
        this.toastService.show('Inquiry deleted.', 'success');
        if (this.selectedInquiry?.id === id) this.closeModal();
      },
      error: () => this.toastService.show('Failed to delete inquiry.', 'error')
    });
  }

  statusPillClass(status: string): string {
    if (status === 'Reviewed') return 'ap-gold';
    if (status === 'Closed') return 'ap-green';
    return 'ap-blue';
  }
}
