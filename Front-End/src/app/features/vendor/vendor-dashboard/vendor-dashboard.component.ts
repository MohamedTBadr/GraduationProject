import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { EventService } from '../../../core/services/event.service';
import { VendorService } from '../../../core/services/vendor.service';
import { ProductService } from '../../../core/services/product.service';
import { EventResponseDto, ApiVendor, ApiProduct } from '../../../shared/types/api.interfaces';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-vendor-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './vendor-dashboard.component.html',
  styleUrls: ['./vendor-dashboard.component.scss']
})
export class VendorDashboardComponent implements OnInit {
  vendorName: string = 'Vendor';
  vendorId: string | null = null;
  
  // Stats
  totalBookings = signal(0);
  pendingRequests = signal(0);
  totalRevenue = signal(0);
  averageRating = signal(0);
  
  upcomingEvents = signal<EventResponseDto[]>([]);
  pendingEvents = signal<any[]>([]); // Items waiting for approval

  constructor(
    private authService: AuthService,
    private eventService: EventService,
    private vendorService: VendorService,
    private productService: ProductService
  ) {}

  ngOnInit() {
    const user = this.authService.user();
    if (user) {
      this.vendorName = user.name || 'Vendor';
      this.vendorId = user.id;
      this.loadDashboardData();
    }
  }

  private loadDashboardData() {
    if (!this.vendorId) return;

    // In a real scenario, we'd fetch events and filter items belonging to this vendor
    // Or call a dedicated statistics endpoint if available.
    // For now, we fetch events related to the user and vendor profile.
    
    forkJoin({
      vendor: this.vendorService.getById(this.vendorId),
      events: this.eventService.getByUser(this.vendorId)
    }).subscribe({
      next: (data) => {
        this.processVendorStats(data.vendor, data.events);
      },
      error: (err) => {
        console.error('Error loading dashboard data', err);
      }
    });
  }

  private processVendorStats(vendor: ApiVendor, events: EventResponseDto[]) {
    this.averageRating.set(vendor.rating || 0);
    
    let bookingsCount = 0;
    let pendingCount = 0;
    let revenue = 0;
    const upcoming: EventResponseDto[] = [];
    const pending: any[] = [];

    events.forEach(event => {
      event.eventItems.forEach(item => {
        if (item.vendorId === this.vendorId) {
          if (item.itemStatus === 'Approved') {
            bookingsCount++;
            revenue += item.price * item.quantity;
            upcoming.push(event);
          } else if (item.itemStatus === 'Pending') {
            pendingCount++;
            pending.push({
              client: event.userName,
              eventTitle: event.title,
              date: event.eventDate,
              value: item.price * item.quantity,
              itemId: item.id
            });
          }
        }
      });
    });

    this.totalBookings.set(bookingsCount);
    this.pendingRequests.set(pendingCount);
    this.totalRevenue.set(revenue);
    this.upcomingEvents.set(upcoming.slice(0, 5)); // Show top 5
    this.pendingEvents.set(pending.slice(0, 5));
  }

  acceptRequest(itemId: string) {
    this.eventService.updateItemStatus(itemId, 'Approved').subscribe(() => {
      this.loadDashboardData();
    });
  }

  declineRequest(itemId: string) {
    this.eventService.updateItemStatus(itemId, 'Rejected').subscribe(() => {
      this.loadDashboardData();
    });
  }
}
