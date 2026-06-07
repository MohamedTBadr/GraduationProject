import { Injectable, inject, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { NotificationService } from './notification.service';
import { ToastService } from '../../shared/components/toast/toast.service';
import { AppNotification } from '../../shared/types/api.interfaces';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private eventSource: EventSource | null = null;
  private sseTimeoutRef: any = null;
  /** Circuit-breaker: stop SSE reconnects after this many consecutive failures */
  private readonly SSE_MAX_RETRIES = 3;
  private sseRetryCount = 0;

  // Signals/Streams for consumers
  public notifications = signal<AppNotification[]>([]);
  public unreadCount = signal<number>(0);
  public chatMessageReceived = new Subject<any>();

  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private notificationService = inject(NotificationService);

  /** Start realtime connections and load notification inbox from REST. */
  sessionBootstrap(): void {
    this.startConnections();
    this.refreshNotifications();
  }

  /** GET /notifications → sync shared signals used by navbar, sidebars, and pages. */
  refreshNotifications(): void {
    if (!this.authService.getToken()) return;

    this.notificationService.getNotifications().subscribe({
      next: (data) => {
        const list = data || [];
        this.notifications.set(list);
        this.unreadCount.set(list.filter(n => !n.isRead).length);
      },
      error: () => {
        this.toastService.show('Could not load notifications', 'error');
      }
    });
  }

  startConnections() {
    const token = this.authService.getToken();
    if (!token) return;

    const state = this.hubConnection?.state;
    if (
      state === signalR.HubConnectionState.Connected ||
      state === signalR.HubConnectionState.Connecting ||
      state === signalR.HubConnectionState.Reconnecting
    ) {
      return;
    }

    if (this.hubConnection) {
      this.hubConnection.stop().catch(() => {});
      this.hubConnection = null;
    }

    const connectionUrl = environment.signalRUrl;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(connectionUrl, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveMessage', (message: any) => {
      this.chatMessageReceived.next(message);
    });

    this.hubConnection.on('ReceiveNotification', (notification: unknown) => {
      this.pushNotification(notification);
    });

    this.hubConnection.on('UserPresence', (_presence: any) => {
      // Ignore
    });

    this.hubConnection.start()
      .then(() => console.log('SignalR Hub connected via SignalRService'))
      .catch(err => {
        console.error('Error while starting Hub connection:', err);
        this.hubConnection = null;
        // If 401 Unauthorized or negotiation fails, trigger token refresh & reconnect
        if (err.statusCode === 401 || (err.message && err.message.includes('401'))) {
          console.log('[SignalRService] SignalR negotiate 401, refreshing token...');
          this.authService.refreshTokenOnce().subscribe({
            next: () => this.startConnections()
          });
        }
      });

    // Start SSE for Notifications (with circuit-breaker)
    this.sseRetryCount = 0;
    this.startSseNotifications();
  }

  /** Resolves when the hub is connected, or rejects after timeout */
  ensureHubConnected(timeoutMs = 15000): Promise<void> {
    this.startConnections();

    return new Promise((resolve, reject) => {
      const startedAt = Date.now();

      const check = () => {
        const hub = this.hubConnection;
        if (hub?.state === signalR.HubConnectionState.Connected) {
          resolve();
          return;
        }
        if (Date.now() - startedAt >= timeoutMs) {
          reject(new Error('SignalR hub connection timed out'));
          return;
        }
        setTimeout(check, 100);
      };

      check();
    });
  }

  /** Invokes a hub method on the shared connection */
  async invokeHub(method: string, ...args: unknown[]): Promise<void> {
    await this.ensureHubConnected();
    await this.hubConnection!.invoke(method, ...args);
  }

  private startSseNotifications() {
    if (this.eventSource) return;

    // Circuit-breaker: if SSE has failed too many times, stop retrying silently.
    // SignalR's ReceiveNotification already handles push notifications.
    if (this.sseRetryCount >= this.SSE_MAX_RETRIES) {
      console.warn(
        `[SignalRService] SSE circuit-breaker tripped after ${this.SSE_MAX_RETRIES} failures. ` +
        `Relying on SignalR for notifications.`
      );
      return;
    }

    // Clear any existing retry timeouts to avoid concurrency leaks
    if (this.sseTimeoutRef) {
      clearTimeout(this.sseTimeoutRef);
      this.sseTimeoutRef = null;
    }

    const currentToken = this.authService.getToken();
    if (!currentToken) return;

    const sseUrl = `${environment.apiUrl}/notifications/stream?access_token=${currentToken}`;
    this.eventSource = new EventSource(sseUrl);

    this.eventSource.onopen = () => {
      // Reset circuit-breaker counter on successful connection
      this.sseRetryCount = 0;
    };

    this.eventSource.onmessage = (event) => {
      try {
        this.pushNotification(JSON.parse(event.data));
      } catch (err) {
        console.error('Error parsing SSE notification:', err);
      }
    };

    this.eventSource.onerror = (_error) => {
      this.eventSource?.close();
      this.eventSource = null;
      this.sseRetryCount++;

      // Trip the circuit-breaker — SignalR handles notifications anyway
      if (this.sseRetryCount >= this.SSE_MAX_RETRIES) {
        console.warn(
          `[SignalRService] SSE failed ${this.sseRetryCount} times consecutively. ` +
          `Stopping SSE retries. SignalR will handle notifications.`
        );
        return;
      }

      if (this.sseTimeoutRef) {
        clearTimeout(this.sseTimeoutRef);
      }

      // Exponential backoff: 5 s → 10 s → 20 s (capped at 30 s)
      const delay = Math.min(5000 * Math.pow(2, this.sseRetryCount - 1), 30000);
      console.log(
        `[SignalRService] SSE error (attempt ${this.sseRetryCount}/${this.SSE_MAX_RETRIES}), ` +
        `retrying in ${delay / 1000}s...`
      );

      this.authService.refreshTokenOnce().subscribe({
        next: () => {
          this.sseTimeoutRef = setTimeout(() => this.startSseNotifications(), delay);
        },
        error: () => {
          this.sseTimeoutRef = setTimeout(() => this.startSseNotifications(), delay * 2);
        }
      });
    };
  }

  stopConnections() {
    if (this.sseTimeoutRef) {
      clearTimeout(this.sseTimeoutRef);
      this.sseTimeoutRef = null;
    }
    this.sseRetryCount = 0;
    this.hubConnection?.stop();
    this.hubConnection = null;
    this.eventSource?.close();
    this.eventSource = null;
    this.notifications.set([]);
    this.unreadCount.set(0);
  }

  private pushNotification(raw: unknown): void {
    const notification = this.notificationService.normalizeNotification(raw);
    if (!notification) return;

    const exists = this.notifications().some(n => n.id === notification.id);
    if (exists) return;

    this.toastService.show(`New Notification: ${notification.title}`, 'info');
    this.notifications.update(list => [{ ...notification, isLive: true }, ...list]);
    if (!notification.isRead) {
      this.unreadCount.update(c => c + 1);
    }
  }
}
