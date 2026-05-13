import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppNotification } from '../../../shared/types/api.interfaces';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { NotificationService } from '../../../core/services/notification.service';
import { SignalRService } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './notifications.component.html',
  styleUrls: ['./notifications.component.scss']
})
export class NotificationsComponent implements OnInit {
  activeTab: string = 'All';
  tabs = ['All', 'Bookings', 'Messages', 'Reviews', 'Finance'];

  constructor(
    private notificationService: NotificationService,
    private toastService: ToastService,
    public signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    // Only load if we don't have notifications yet or to refresh
    if (this.signalRService.notifications().length === 0) {
      this.loadNotifications();
    }
  }

  loadNotifications(): void {
    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        this.signalRService.notifications.set(data || []);
        // Update unread count based on loaded data
        const unreadCount = (data || []).filter(n => !n.isRead).length;
        this.signalRService.unreadCount.set(unreadCount);
      },
      error: (err) => {
        console.error('Failed to load notifications', err);
        this.toastService.show('Failed to load notifications', 'error');
      }
    });
  }

  get notifications(): AppNotification[] {
    return this.signalRService.notifications();
  }

  get filteredNotifications(): AppNotification[] {
    const all = this.notifications;
    if (this.activeTab === 'All') return all;
    // Map tabs to types
    const typeMapping: { [key: string]: string } = {
      'Bookings': 'booking',
      'Messages': 'message',
      'Reviews': 'review',
      'Finance': 'finance'
    };
    const targetType = typeMapping[this.activeTab];
    return all.filter(n => n.type === targetType);
  }

  getUnreadCount(): number {
    return this.signalRService.unreadCount();
  }

  setTab(tab: string): void {
    this.activeTab = tab;
  }

  markAsRead(notification: AppNotification): void {
    if (notification.isRead) return;
    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        // Update both the notification object and the global signal count
        notification.isRead = true;
        this.signalRService.unreadCount.update(c => Math.max(0, c - 1));
        
        // Ensure the signal triggers a UI update if needed (though modifying the object might work, updating signal is cleaner)
        this.signalRService.notifications.update(n => [...n]);
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
