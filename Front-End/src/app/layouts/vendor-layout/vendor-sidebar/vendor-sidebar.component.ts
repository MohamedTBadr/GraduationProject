import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { VendorService } from '../../../core/services/vendor.service';
import { EventService } from '../../../core/services/event.service';
import { ChatService } from '../../../core/services/chat.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { ApiVendor, EventResponseDto } from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-vendor-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './vendor-sidebar.component.html',
  styleUrl: './vendor-sidebar.component.scss'
})
export class VendorSidebarComponent implements OnInit {
  vendorName: string | null = null;
  vendorCategory: string | null = null;
  vendorLocation: string | null = null;
  pendingBookingsCount = 0;
  unreadMessagesCount = 0;
  unreadNotificationsCount = 0;

  private vendorId: string | null = null;

  constructor(
    private authService: AuthService,
    private vendorService: VendorService,
    private eventService: EventService,
    private chatService: ChatService,
    private notificationService: NotificationService,
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
        this.vendorLocation = vendor.location || '';
      },
      error: () => {
        this.vendorCategory = 'Vendor';
        this.vendorLocation = '';
      }
    });

    this.loadPendingBookings();
    this.loadUnreadMessages();
    this.loadUnreadNotifications();
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

  private countPendingBookings(events: EventResponseDto[]): number {
    let count = 0;
    events.forEach(event => {
      event.eventItems.forEach(item => {
        if (item.vendorId === this.vendorId && item.itemStatus === 'Pending') {
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

  private loadUnreadNotifications(): void {
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        this.unreadNotificationsCount = (data || []).filter(n => !n.isRead).length;
        this.signalRService.unreadCount.set(this.unreadNotificationsCount);
      },
      error: () => { this.unreadNotificationsCount = 0; }
    });
  }
}
