import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppNotification } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { PaginationComponent } from '../../../shared/components/pagination/pagination.component';

const TYPE = {
  ACCOUNT_ACCEPTED:    0,
  ACCOUNT_SUSPENDED:   1,
  ORDER_PLACED:        2,
  ORDER_REJECTED:      3,
  PAYMENT_REJECTED:    4,
  PAYMENT_ACCEPTED:    5,
  ORDER_CANCELLED:     6,
  ORDER_COMPLETED:     7,
  EVENT_STATUS_UPDATED:  8,
  EVENT_STATUS_DELETED:  9,
  EVENT_ITEM_APPROVED:  10,
  EVENT_ITEM_REJECTED:  11,
  EVENT_COMPLETED:      12
} as const;

const TAB_TYPES: Record<string, number[]> = {
  Bookings: [TYPE.ORDER_PLACED, TYPE.ORDER_REJECTED, TYPE.ORDER_CANCELLED, TYPE.ORDER_COMPLETED,
             TYPE.EVENT_ITEM_APPROVED, TYPE.EVENT_ITEM_REJECTED],
  Events:   [TYPE.EVENT_STATUS_UPDATED, TYPE.EVENT_STATUS_DELETED, TYPE.EVENT_COMPLETED],
  Payments: [TYPE.PAYMENT_ACCEPTED, TYPE.PAYMENT_REJECTED],
  Account:  [TYPE.ACCOUNT_ACCEPTED, TYPE.ACCOUNT_SUSPENDED]
};

@Component({
  selector: 'app-user-notifications',
  standalone: true,
  imports: [CommonModule, PaginationComponent],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss'
})
export class UserNotificationsComponent implements OnInit {
  activeTab = 'All';
  tabs = ['All', 'Bookings', 'Events', 'Payments', 'Account'];
  pageNumber = 1;
  pageSize = 12;

  constructor(
    private notificationService: NotificationService,
    private toastService: ToastService,
    public signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    if (this.signalRService.notifications().length === 0) {
      this.loadNotifications();
    }
  }

  loadNotifications(): void {
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        this.signalRService.notifications.set(data || []);
        this.signalRService.unreadCount.set((data || []).filter(n => !n.isRead).length);
      },
      error: () => this.toastService.show('Failed to load notifications', 'error')
    });
  }

  get notifications(): AppNotification[] {
    return this.signalRService.notifications();
  }

  get filteredNotifications(): AppNotification[] {
    const types = TAB_TYPES[this.activeTab];
    if (!types) return this.notifications;
    return this.notifications.filter(n => types.includes(n.type ?? -1));
  }

  get paginatedNotifications(): AppNotification[] {
    const start = (this.pageNumber - 1) * this.pageSize;
    return this.filteredNotifications.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.filteredNotifications.length / this.pageSize));
  }

  tabUnreadCount(tab: string): number {
    const unread = this.notifications.filter(n => !n.isRead);
    const types = TAB_TYPES[tab];
    if (!types) return unread.length;
    return unread.filter(n => types.includes(n.type ?? -1)).length;
  }

  setTab(tab: string): void {
    this.activeTab = tab;
    this.pageNumber = 1;
  }

  onPageChange(page: number): void {
    this.pageNumber = page;
  }

  getTypeIcon(type?: number): string {
    switch (type) {
      case TYPE.ORDER_PLACED:
      case TYPE.ORDER_REJECTED:
      case TYPE.ORDER_CANCELLED:
      case TYPE.ORDER_COMPLETED:       return 'bi-calendar3';
      case TYPE.EVENT_ITEM_APPROVED:
      case TYPE.EVENT_ITEM_REJECTED:   return 'bi-calendar-check';
      case TYPE.EVENT_STATUS_UPDATED:
      case TYPE.EVENT_STATUS_DELETED:
      case TYPE.EVENT_COMPLETED:       return 'bi-calendar-event';
      case TYPE.PAYMENT_ACCEPTED:      return 'bi-credit-card';
      case TYPE.PAYMENT_REJECTED:      return 'bi-credit-card-2-back';
      case TYPE.ACCOUNT_ACCEPTED:      return 'bi-person-check';
      case TYPE.ACCOUNT_SUSPENDED:     return 'bi-person-slash';
      default:                         return 'bi-bell';
    }
  }

  getTypeColor(type?: number): string {
    switch (type) {
      case TYPE.PAYMENT_ACCEPTED:
      case TYPE.EVENT_ITEM_APPROVED:
      case TYPE.ORDER_COMPLETED:
      case TYPE.ACCOUNT_ACCEPTED:    return 'var(--gold, #c9a84c)';
      case TYPE.PAYMENT_REJECTED:
      case TYPE.ORDER_REJECTED:
      case TYPE.ORDER_CANCELLED:
      case TYPE.ACCOUNT_SUSPENDED:
      case TYPE.EVENT_ITEM_REJECTED: return '#ef4444';
      default:                       return '';
    }
  }

  markAsRead(notification: AppNotification): void {
    if (notification.isRead) return;
    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        notification.isRead = true;
        this.signalRService.unreadCount.update(c => Math.max(0, c - 1));
        this.signalRService.notifications.update(n => [...n]);
      },
      error: () => this.toastService.show('Failed to mark as read', 'error')
    });
  }

  markAllAsRead(): void {
    this.notifications.filter(n => !n.isRead).forEach(n => this.markAsRead(n));
  }
}
