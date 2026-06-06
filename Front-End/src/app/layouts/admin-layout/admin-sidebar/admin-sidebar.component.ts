import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { FontAwesomeModule, FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { faChartBar, faEdit, faBuilding, faUsers, faCalendarAlt, faBox, faMoneyBillWave, faCreditCard, faFileAlt, faTag, faTicketAlt, faCrown, faChartPie, faLifeRing } from '@fortawesome/free-solid-svg-icons';
import { VendorService } from '../../../core/services/vendor.service';
import { SupportService } from '../../../core/services/support.service';

@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, FontAwesomeModule],
  templateUrl: './admin-sidebar.component.html',
  styleUrl: './admin-sidebar.component.scss'
})
export class AdminSidebarComponent implements OnInit {
  pendingVendorCount = 0;
  openTicketCount = 0;

  constructor(
    private library: FaIconLibrary,
    private vendorService: VendorService,
    private supportService: SupportService
  ) {
    library.addIcons(
      faChartBar,
      faEdit,
      faBuilding,
      faUsers,
      faCalendarAlt,
      faBox,
      faMoneyBillWave,
      faCreditCard,
      faFileAlt,
      faTag,
      faTicketAlt,
      faCrown,
      faChartPie,
      faLifeRing
    );
  }

  ngOnInit(): void {
    this.vendorService.getAll({ pageIndex: 1, pageSize: 500 }).subscribe({
      next: (vendors) => {
        this.pendingVendorCount = vendors.filter(v => !v.isApproved).length;
      },
      error: () => { this.pendingVendorCount = 0; }
    });

    this.supportService.getStats().subscribe({
      next: (stats) => {
        this.openTicketCount = (stats.open ?? 0) + (stats.in_progress ?? 0);
      },
      error: () => { this.openTicketCount = 0; }
    });
  }
}
