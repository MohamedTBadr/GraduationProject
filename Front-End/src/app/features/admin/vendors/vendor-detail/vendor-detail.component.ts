import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { VendorService } from '../../../../core/services/vendor.service';
import { ApiVendor } from '../../../../shared/types/api.interfaces';
import { ToastService } from '../../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-vendor-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './vendor-detail.component.html',
  styleUrls: ['./vendor-detail.component.scss']
})
export class VendorDetailComponent implements OnInit {
  vendor: ApiVendor | null = null;
  loading = false;
  activeTab: 'info' | 'services' | 'history' = 'info';

  constructor(
    private route: ActivatedRoute,
    private vendorService: VendorService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadVendor(id);
    }
  }

  loadVendor(id: string) {
    this.loading = true;
    this.vendorService.getById(id).subscribe({
      next: (data) => {
        this.vendor = data;
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load vendor details', 'error');
        this.loading = false;
        this.router.navigate(['/admin/vendors']);
      }
    });
  }

  approveVendor() {
    if (!this.vendor) return;
    if (confirm(`Are you sure you want to approve ${this.vendor.name}?`)) {
      this.vendorService.approve(this.vendor.id).subscribe({
        next: () => {
          this.toastService.show(`${this.vendor?.name} approved!`, 'success');
          this.loadVendor(this.vendor!.id);
        },
        error: () => this.toastService.show('Approval failed', 'error')
      });
    }
  }

  suspendVendor() {
    if (!this.vendor) return;
    const action = this.vendor.status === 'suspended' ? 'unsuspend' : 'suspend';
    if (confirm(`Are you sure you want to ${action} ${this.vendor.name}?`)) {
      // Assuming delete acts as suspend in this business logic or there's a dedicated suspend
      this.vendorService.delete(this.vendor.id).subscribe({
        next: () => {
          this.toastService.show(`Vendor ${action}ed!`, 'success');
          this.loadVendor(this.vendor!.id);
        },
        error: () => this.toastService.show(`Action failed`, 'error')
      });
    }
  }
}
