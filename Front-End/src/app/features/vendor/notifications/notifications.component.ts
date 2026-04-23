import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppNotification } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss']
})
export class NotificationsComponent implements OnInit {
  notifications: AppNotification[] = [];
  activeTab: string = 'All';
  tabs = ['All', 'Bookings', 'Messages', 'Reviews', 'Finance'];

  constructor(
    private notificationService: NotificationService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadNotifications();
  }

  loadNotifications(): void {
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        this.notifications = data || [];
      },
      error: (err) => {
        console.error('Failed to load notifications', err);
        this.toastService.show('Failed to load notifications', 'error');
      }
    });
  }

  get filteredNotifications(): AppNotification[] {
    if (this.activeTab === 'All') return this.notifications;
    // Map tabs to types
    const typeMapping: { [key: string]: string } = {
      'Bookings': 'booking',
      'Messages': 'message',
      'Reviews': 'review',
      'Finance': 'finance'
    };
    const targetType = typeMapping[this.activeTab];
    return this.notifications.filter(n => n.type === targetType);
  }

  getUnreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  setTab(tab: string): void {
    this.activeTab = tab;
  }

  markAsRead(notification: AppNotification): void {
    if (notification.isRead) return;
    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        notification.isRead = true;
      },
      error: (err) => {
        console.error('Failed to mark notification as read', err);
        this.toastService.show('Failed to mark notification as read', 'error');
      }
    });
  }

  markAllAsRead(): void {
    const unread = this.notifications.filter(n => !n.isRead);
    unread.forEach(n => this.markAsRead(n));
  }
}
