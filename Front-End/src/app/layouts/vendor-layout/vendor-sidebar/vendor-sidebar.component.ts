import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, NavigationEnd } from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { AuthService } from '../../../core/services/auth.service';
import { VendorService } from '../../../core/services/vendor.service';
import { EventService } from '../../../core/services/event.service';
import { ChatService } from '../../../core/services/chat.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { ApiVendor, EventResponseDto } from '../../../shared/types/api.interfaces';
import { formatVendorLocation } from '../../../shared/utils/location.utils';
import { VendorBookingsRefreshService } from '../../../core/services/vendor-bookings-refresh.service';

@Component({
  selector: 'app-vendor-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './vendor-sidebar.component.html',
  styleUrl: './vendor-sidebar.component.scss'
})
export class VendorSidebarComponent implements OnInit, OnDestroy {
  vendorName: string | null = null;
  vendorCategory: string | null = null;
  vendorLocation: string | null = null;
  pendingBookingsCount = 0;
  unreadMessagesCount = 0;

  private vendorId: string | null = null;
  private refreshSub?: Subscription;

  constructor(
    private authService: AuthService,
    private vendorService: VendorService,
    private eventService: EventService,
    private chatService: ChatService,
    private router: Router,
    private bookingsRefresh: VendorBookingsRefreshService,
    public signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    const user = this.authService.user();
    if (!user) return;

    this.vendorId = user.id;
    this.vendorName = user.name;

    this.vendorService.getById(user.id).subscribe({
      next: (vendor: ApiVendor) => {
        this.vendorCategory = vendor.vendorTypeName || 'Vendor';
        this.vendorLocation = formatVendorLocation(vendor);
      },
      error: () => {
        this.vendorCategory = 'Vendor';
        this.vendorLocation = '';
      }
    });

    this.loadPendingBookings();
    this.loadUnreadMessages();

    this.refreshSub = this.bookingsRefresh.refresh$.subscribe(() => this.loadPendingBookings());
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe(() => this.loadPendingBookings());
  }

  ngOnDestroy(): void {
    this.refreshSub?.unsubscribe();
  }

  private loadPendingBookings(): void {
    if (!this.vendorId) return;
    this.eventService.getForVendor(this.vendorId).subscribe({
      next: (events) => {
        this.pendingBookingsCount = this.countPendingBookings(events);
      },
      error: () => { this.pendingBookingsCount = 0; }
    });
  }

  private normalizeId(id: string | null | undefined): string {
    return id == null ? '' : String(id).trim().toLowerCase();
  }

  private countPendingBookings(events: EventResponseDto[]): number {
    let count = 0;
    const vendorIdNorm = this.normalizeId(this.vendorId);
    events.forEach(event => {
      event.eventItems.forEach(item => {
        if (this.normalizeId(item.vendorId) === vendorIdNorm && item.itemStatus === 'Pending') {
          count++;
        }
      });
    });
    return count;
  }

  private loadUnreadMessages(): void {
    this.chatService.getConversations().subscribe({
      next: (conversations) => {
        this.unreadMessagesCount = (conversations || []).reduce(
          (sum, c) => sum + (c.unreadCount ?? 0),
          0
        );
      },
      error: () => { this.unreadMessagesCount = 0; }
    });
  }
}
