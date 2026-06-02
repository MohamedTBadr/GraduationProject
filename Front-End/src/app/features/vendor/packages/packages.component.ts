import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

export interface VendorServiceItem {
  id: number;
  name: string;
}

export interface PackageItem {
  id: number;
  icon: string;
  name: string;
  status: 'Active' | 'Paused';
  pricePrefix: string;
  price: number;
  description: string;
  services: string[];
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
  packages: PackageItem[] = [
    {
      id: 1,
      icon: 'heart',
      name: 'Full Wedding Package',
      status: 'Active',
      pricePrefix: 'From',
      price: 32000,
      description: 'Complete wedding florals — ceremony to reception. Stage arch, table centerpieces, bridal suite, entrance arrangement, setup, teardown, and coordinator included.',
      services: ['Wedding Stage Floral', 'Table Centerpieces', 'Bridal Bouquet'],
      savings: 'Save 5,000 EGP vs. individual booking',
      bookings: 12
    },
    {
      id: 2,
      icon: 'heart-fill',
      name: 'Engagement Bundle',
      status: 'Active',
      pricePrefix: 'From',
      price: 14000,
      description: 'Full engagement room transformation: arch, backdrop, dessert table styling, candle arrangement, and petal floor design.',
      services: ['Engagement Styling', 'Dessert Table Setup'],
      savings: 'Save 2,500 EGP vs. individual booking',
      bookings: 7
    },
    {
      id: 3,
      icon: 'gift',
      name: 'Birthday Bloom',
      status: 'Paused',
      pricePrefix: 'From',
      price: 6500,
      description: 'Birthday decor bundle: balloon art, themed florals, and dessert table styling. Available in any theme.',
      services: ['Balloon Art', 'Dessert Table Setup'],
      savings: 'Save 1,200 EGP vs. individual booking',
      bookings: 4
    }
  ];

  vendorServices: VendorServiceItem[] = [
    { id: 1, name: 'Wedding Stage Floral' },
    { id: 2, name: 'Table Centerpieces' },
    { id: 3, name: 'Balloon Art' },
    { id: 4, name: 'Engagement Styling' },
    { id: 5, name: 'Bridal Bouquet' },
    { id: 6, name: 'Dessert Table Setup' }
  ];

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

  selectedServicesIds: number[] = [];

  isModalOpen = false;
  editingPackageId: number | null = null;
  packageForm!: FormGroup;

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initForm();
  }

  initForm(): void {
    this.packageForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      price: ['', [Validators.required, Validators.min(0)]],
      priceType: ['Fixed', Validators.required]
    });
  }

  openCreateModal(pkgToEdit?: any): void {
    try {
      console.log('openCreateModal', pkgToEdit);
      // Check if what was passed is actually a package object
      if (pkgToEdit && typeof pkgToEdit.id !== 'undefined') {
        this.editingPackageId = pkgToEdit.id;
        
        // Ensure form is initialized
        if (!this.packageForm) this.initForm();

        this.packageForm.patchValue({
          name: pkgToEdit.name || '',
          description: pkgToEdit.description || '',
          price: pkgToEdit.price || 0,
          priceType: pkgToEdit.pricePrefix === 'From' ? 'Starting From' : 'Fixed'
        });
        
        // Safely map names back to IDs
        const serviceList = pkgToEdit.services || [];
        this.selectedServicesIds = serviceList
            .map((srvName: string) => this.vendorServices.find(vs => vs.name === srvName)?.id)
            .filter((id: any) => id !== undefined) as number[];
      } else {
        this.editingPackageId = null;
        this.initForm();
        this.selectedServicesIds = [];
      }
      
      this.isModalOpen = true;
    } catch (e: any) {
      alert('Error opening modal: ' + e.message);
      console.error(e);
    }
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  toggleServiceSelection(serviceId: number): void {
    const index = this.selectedServicesIds.indexOf(serviceId);
    if (index > -1) {
      this.selectedServicesIds.splice(index, 1);
    } else {
      this.selectedServicesIds.push(serviceId);
    }
  }

  isServiceSelected(serviceId: number): boolean {
    return this.selectedServicesIds.includes(serviceId);
  }

  createPackage(): void {
    if (this.packageForm.valid && this.selectedServicesIds.length > 0) {
      const formVal = this.packageForm.value;
      const resolvedServices = this.vendorServices.filter(s => this.selectedServicesIds.includes(s.id)).map(s => s.name);
      
      if (this.editingPackageId !== null) {
        // Update existing item
        const idx = this.packages.findIndex(p => p.id === this.editingPackageId);
        if (idx !== -1) {
          this.packages[idx] = {
            ...this.packages[idx],
            name: formVal.name,
            pricePrefix: formVal.priceType === 'Starting From' ? 'From' : '',
            price: formVal.price,
            description: formVal.description,
            services: resolvedServices
          };
        }
      } else {
        // Create new item
        const newPackage: PackageItem = {
          id: Date.now(),
          icon: 'stars', 
          name: formVal.name,
          status: 'Active',
          pricePrefix: formVal.priceType === 'Starting From' ? 'From' : '',
          price: formVal.price,
          description: formVal.description,
          services: resolvedServices,
          savings: 'Value-packed deal',
          bookings: 0
        };

        this.packages.unshift(newPackage);
      }
      
      this.closeModal();
    } else {
      // Force display validation errors if they missed something
      this.packageForm.markAllAsTouched();
    }
  }

  togglePackageStatus(pkg: PackageItem): void {
    if (pkg.status === 'Active') {
      pkg.status = 'Paused';
      this.activeTab = 'Paused';
    } else {
      pkg.status = 'Active';
      this.activeTab = 'Active';
    }
  }

  deletePackage(id: number): void {
    if (confirm('Are you sure you want to delete this package?')) {
      this.packages = this.packages.filter(p => p.id !== id);
    }
  }
}


