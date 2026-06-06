import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CompanyInquiryService, CompanyInquiryResponse } from '../../../core/services/company-inquiry.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

@Component({
  selector: 'app-company-inquiries',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './company-inquiries.component.html',
  styleUrl: './company-inquiries.component.scss'
})
export class CompanyInquiriesComponent implements OnInit {
  allInquiries: CompanyInquiryResponse[] = [];
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
    this.inquiryService.getAll(1, 500).subscribe({
      next: ({ items }) => {
        this.allInquiries = items;
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load inquiries.', 'error');
        this.loading = false;
      }
    });
  }

  get filteredInquiries(): CompanyInquiryResponse[] {
    if (this.statusFilter === 'All') return this.allInquiries;
    return this.allInquiries.filter(i => i.status === this.statusFilter);
  }

  get paginatedInquiries(): CompanyInquiryResponse[] {
    const start = (this.pageNumber - 1) * this.pageSize;
    return this.filteredInquiries.slice(start, start + this.pageSize);
  }

  get totalCount(): number {
    return this.filteredInquiries.length;
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get pendingCount(): number {
    return this.allInquiries.filter(i => i.status === 'Pending').length;
  }

  setStatusFilter(status: 'All' | 'Pending' | 'Reviewed' | 'Closed'): void {
    this.statusFilter = status;
    this.pageNumber = 1;
  }

  onPageChange(page: number): void {
    this.pageNumber = page;
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
        inquiry.status = status as CompanyInquiryResponse['status'];
        this.toastService.show(`Status updated to ${status}.`, 'success');
        if (this.selectedInquiry?.id === inquiry.id) {
          this.selectedInquiry.status = status as CompanyInquiryResponse['status'];
        }
      },
      error: (err) => this.toastService.show(err?.message ?? 'Failed to update status.', 'error')
    });
  }

  deleteInquiry(id: string): void {
    if (!confirm('Delete this inquiry?')) return;
    this.inquiryService.delete(id).subscribe({
      next: () => {
        this.allInquiries = this.allInquiries.filter(i => i.id !== id);
        this.toastService.show('Inquiry deleted.', 'success');
        if (this.selectedInquiry?.id === id) this.closeModal();
        if (this.pageNumber > this.totalPages) this.pageNumber = this.totalPages;
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
