import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PackageService, ApiPackage } from '../../../core/services/package.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-packages',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './packages.component.html',
  styleUrl: './packages.component.scss'
})
export class PackagesComponent implements OnInit {
  packages: ApiPackage[] = [];
  loading = false;

  constructor(
    private packageService: PackageService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadPackages();
  }

  loadPackages(): void {
    this.loading = true;
    this.packageService.getAll().subscribe({
      next: (pkgs) => {
        this.packages = pkgs;
        this.loading = false;
      },
      error: () => {
        this.toastService.show('Failed to load packages.', 'error');
        this.loading = false;
      }
    });
  }

  deletePackage(id: string): void {
    if (!confirm('Remove this package from the platform?')) return;
    this.packageService.delete(id).subscribe({
      next: () => {
        this.packages = this.packages.filter(p => p.id !== id);
        this.toastService.show('Package removed.', 'success');
      },
      error: () => this.toastService.show('Failed to remove package.', 'error')
    });
  }
}
