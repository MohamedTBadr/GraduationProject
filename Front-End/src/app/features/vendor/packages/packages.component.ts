import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PackageService, ApiPackage } from '../../../core/services/package.service';
import { ProductService } from '../../../core/services/product.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ApiProduct } from '../../../shared/types/api.interfaces';

interface DisplayPackage {
  id: string;
  icon: string;
  name: string;
  status: 'Active' | 'Paused';
  pricePrefix: string;
  price: number;
  description: string;
  services: string[];
  serviceIds: string[];
  savings: string;
  bookings: number;
}

@Component({
  selector: 'app-packages',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './packages.component.html',
  styleUrls: ['./packages.component.scss']
})
export class PackagesComponent implements OnInit {
  packages: DisplayPackage[] = [];
  vendorServices: ApiProduct[] = [];
  vendorId = '';
  loading = false;

  activeTab: 'Active' | 'Paused' = 'Active';

  get filteredPackages() {
    return this.packages.filter(p => p.status === this.activeTab);
  }

  getActiveCount(): number {
    return this.packages.filter(p => p.status === 'Active').length;
  }

  getPausedCount(): number {
    return this.packages.filter(p => p.status === 'Paused').length;
  }

  selectedServicesIds: string[] = [];

  isModalOpen = false;
  editingPackageId: string | null = null;
  packageForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private packageService: PackageService,
    private productService: ProductService,
    private authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.initForm();
    const user = this.authService.user();
    if (user) {
      this.vendorId = user.id;
      this.loadPackages();
      this.loadVendorServices();
    }
  }

  initForm(): void {
    this.packageForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      price: ['', [Validators.required, Validators.min(0)]],
      priceType: ['Fixed', Validators.required]
    });
  }

  loadPackages(): void {
    this.loading = true;
    this.packageService.getByVendor(this.vendorId).subscribe({
      next: (pkgs) => {
        this.packages = pkgs.map(p => this.mapToDisplay(p));
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load packages.', 'error');
        this.loading = false;
      }
    });
  }

  loadVendorServices(): void {
    this.productService.getByVendor(this.vendorId).subscribe({
      next: (services) => { this.vendorServices = services; },
      error: () => this.toastService.show('Failed to load services for packages.', 'error')
    });
  }

  private mapToDisplay(p: ApiPackage): DisplayPackage {
    return {
      id: p.id,
      icon: '📦',
      name: p.name,
      status: 'Active',
      pricePrefix: '',
      price: p.price,
      description: p.description ?? '',
      services: (p.services ?? []).map(s => s.name),
      serviceIds: (p.services ?? []).map(s => s.id),
      savings: p.discount > 0 ? `Save ${p.discount}% on this bundle` : 'Value-packed deal',
      bookings: 0
    };
  }

  openCreateModal(pkgToEdit?: DisplayPackage): void {
    if (pkgToEdit && pkgToEdit.id) {
      this.editingPackageId = pkgToEdit.id;
      if (!this.packageForm) this.initForm();
      this.packageForm.patchValue({
        name: pkgToEdit.name,
        description: pkgToEdit.description,
        price: pkgToEdit.price,
        priceType: pkgToEdit.pricePrefix === 'From' ? 'Starting From' : 'Fixed'
      });
      this.selectedServicesIds = [...pkgToEdit.serviceIds];
    } else {
      this.editingPackageId = null;
      this.initForm();
      this.selectedServicesIds = [];
    }
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  toggleServiceSelection(serviceId: string): void {
    const index = this.selectedServicesIds.indexOf(serviceId);
    if (index > -1) {
      this.selectedServicesIds.splice(index, 1);
    } else {
      this.selectedServicesIds.push(serviceId);
    }
  }

  isServiceSelected(serviceId: string): boolean {
    return this.selectedServicesIds.includes(serviceId);
  }

  createPackage(): void {
    if (!this.packageForm.valid) {
      this.packageForm.markAllAsTouched();
      return;
    }
    const formVal = this.packageForm.value;
    const dto = {
      name: formVal.name as string,
      description: formVal.description as string,
      price: Number(formVal.price),
      discount: 0,
      serviceIds: this.selectedServicesIds
    };

    if (this.editingPackageId !== null) {
      this.packageService.update(this.editingPackageId, { ...dto, vendorId: this.vendorId }).subscribe({
        next: () => {
          this.toastService.show('Package updated successfully.', 'success');
          this.loadPackages();
          this.closeModal();
        },
        error: () => this.toastService.show('Failed to update package.', 'error')
      });
    } else {
      this.packageService.create(dto).subscribe({
        next: () => {
          this.toastService.show('Package created successfully.', 'success');
          this.loadPackages();
          this.closeModal();
        },
        error: () => this.toastService.show('Failed to create package.', 'error')
      });
    }
  }

  togglePackageStatus(pkg: DisplayPackage): void {
    pkg.status = pkg.status === 'Active' ? 'Paused' : 'Active';
    this.activeTab = pkg.status;
  }

  deletePackage(id: string): void {
    if (confirm('Are you sure you want to delete this package?')) {
      this.packageService.delete(id).subscribe({
        next: () => {
          this.toastService.show('Package deleted.', 'success');
          this.packages = this.packages.filter(p => p.id !== id);
        },
        error: () => this.toastService.show('Failed to delete package.', 'error')
      });
    }
  }
}
