import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { VendorService } from '../../../core/services/vendor.service';
import { ApiVendor } from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-vendor-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './vendor-sidebar.component.html',
  styleUrl: './vendor-sidebar.component.scss'
})

export class VendorSidebarComponent implements OnInit {
  vendorName : string | null = null;
  vendorCategory : string | null = null;
  vendorLocation : string | null = null;

  constructor(
    private authService: AuthService,
    private vendorService: VendorService
  ) {}

  ngOnInit() {
    const user = this.authService.user();
    if (user) {
      this.vendorName = user.name;
      this.vendorService.getById(user.id).subscribe({
        next: (vendor: ApiVendor) => {
          this.vendorCategory = vendor.vendorTypeName || 'Vendor';
          this.vendorLocation = vendor.location || '';
        },
        error: (err: unknown) => {
          console.error('Error loading vendor profile info:', err);
          this.vendorCategory = 'Vendor';
          this.vendorLocation = '';
        }
      });
    }
  }
}
