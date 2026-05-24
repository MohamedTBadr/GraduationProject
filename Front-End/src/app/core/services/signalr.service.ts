import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
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

  constructor(private authService: AuthService, private toastService: ToastService) {}

  startConnections() {
    const token = this.authService.getToken();
    if (!token) return;

    if (this.hubConnection) return;

    const connectionUrl = environment.signalRUrl;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(connectionUrl, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('SignalR Hub connected via SignalRService'))
      .catch(err => {
        console.error('Error while starting Hub connection:', err);
        // If 401 Unauthorized or negotiation fails, trigger token refresh & reconnect
        if (err.statusCode === 401 || (err.message && err.message.includes('401'))) {
          console.log('[SignalRService] SignalR negotiate 401, refreshing token...');
          this.authService.refreshTokenOnce().subscribe({
            next: () => {
              this.hubConnection = null;
              this.startConnections();
            }
          });
        }
      });

    this.hubConnection.on('ReceiveMessage', (message: any) => {
      this.unreadCount.update(c => c + 1);
      this.chatMessageReceived.next(message);
    });

    this.hubConnection.on('ReceiveNotification', (notification: AppNotification) => {
      this.toastService.show(`New Notification: ${notification.title}`, 'info');
      this.notifications.update(n => [notification, ...n]);
      this.unreadCount.update(c => c + 1);
    });

    this.hubConnection.on('UserPresence', (_presence: any) => {
      // Ignore
    });

    // Start SSE for Notifications (with circuit-breaker)
    this.sseRetryCount = 0;
    this.startSseNotifications();
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
        const notification: AppNotification = JSON.parse(event.data);
        this.toastService.show(`New Notification: ${notification.title}`, 'info');
        this.notifications.update(n => [notification, ...n]);
        this.unreadCount.update(c => c + 1);
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
  }
}
