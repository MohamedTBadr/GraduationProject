import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { VendorService } from '../../../core/services/vendor.service';
import { ApiVendor } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-vendors',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './vendors.component.html',
  styleUrls: ['./vendors.component.scss']
})
export class VendorsComponent implements OnInit {
  vendors: ApiVendor[] = [];
  loading = false;
  activeTab: 'all' | 'pending' | 'active' | 'suspended' = 'all';

  searchQuery: string = '';
  pageNumber: number = 1;
  pageSize: number = 10;

  constructor(
    private vendorService: VendorService,
    private toastService: ToastService
  ) {}

  ngOnInit() {
    this.loadVendors();
  }

  loadVendors() {
    this.loading = true;
    this.vendorService.getAll().subscribe({
      next: (data: any) => {
        this.vendors = data;
        this.resetPagination();
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load vendors', 'error');
        this.loading = false;
      }
    });
  }

  get filteredVendors(): ApiVendor[] {
    let result = this.vendors;

    if (this.activeTab === 'pending') {
      result = result.filter(v => !v.isApproved);
    } else if (this.activeTab === 'active') {
      result = result.filter(v => v.isApproved && v.status === 'active');
    } else if (this.activeTab === 'suspended') {
      result = result.filter(v => v.status === 'suspended');
    }

    if (this.searchQuery && this.searchQuery.trim().length > 0) {
      const q = this.searchQuery.toLowerCase();
      result = result.filter(v => 
        (v.name || '').toLowerCase().includes(q) || 
        (v.vendorTypeName || '').toLowerCase().includes(q) ||
        (v.location || '').toLowerCase().includes(q)
      );
    }

    return result;
  }

  get paginatedVendors(): ApiVendor[] {
    const start = (this.pageNumber - 1) * this.pageSize;
    return this.filteredVendors.slice(start, start + this.pageSize);
  }

  get totalCount(): number {
    return this.filteredVendors.length;
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get pendingCount(): number {
    return this.vendors.filter(v => !v.isApproved).length;
  }
  
  get activeCount(): number {
    return this.vendors.filter(v => v.isApproved && v.status === 'active').length;
  }

  get suspendedCount(): number {
    return this.vendors.filter(v => v.status === 'suspended').length;
  }

  setTab(tab: 'all' | 'pending' | 'active' | 'suspended') {
    this.activeTab = tab;
    this.resetPagination();
  }

  onSearchChange() {
    this.resetPagination();
  }

  resetPagination() {
    this.pageNumber = 1;
  }

  changePage(delta: number) {
    const newPage = this.pageNumber + delta;
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.pageNumber = newPage;
    }
  }

  approveVendor(vendor: ApiVendor) {
    if (confirm(`Are you sure you want to approve ${vendor.name}?`)) {
      this.loading = true;
      this.vendorService.approve(vendor.id).subscribe({
        next: () => {
          this.toastService.show(`${vendor.name} has been approved.`, 'success');
          this.loadVendors();
        },
        error: (err) => {
          this.toastService.show('Error approving vendor. Please try again.', 'error');
          this.loading = false;
        }
      });
    }
  }

  deleteVendor(vendor: ApiVendor) {
    const action = vendor.status === 'suspended' ? 'delete' : 'suspend';
    if (confirm(`Are you sure you want to ${action} ${vendor.name}?`)) {
      this.loading = true;
      this.vendorService.delete(vendor.id).subscribe({
        next: () => {
          this.toastService.show(`Vendor ${vendor.name} has been ${action}ed.`, 'success');
          this.loadVendors();
        },
        error: (err) => {
          this.toastService.show(`Error attempting to ${action} vendor.`, 'error');
          this.loading = false;
        }
      });
    }
  }
}
