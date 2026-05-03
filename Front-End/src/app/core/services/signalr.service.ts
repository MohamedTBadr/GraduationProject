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
        accessTokenFactory: () => token,
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
        headers: { 'IdempotencyKey': crypto.randomUUID ? crypto.randomUUID() : Date.now().toString() }
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => console.log('SignalR Hub connected via SignalRService'))
      .catch(err => console.error('Error while starting Hub connection:', err));

    this.hubConnection.on('ReceiveMessage', (user: string, message: string) => {
      // Basic global toast for new chat message if we want it
      this.unreadCount.update(c => c + 1);
    });

    this.hubConnection.on('ReceiveNotification', (notification: AppNotification) => {
      this.toastService.show(`New Notification: ${notification.title}`, 'info');
      this.notifications.update(n => [notification, ...n]);
      this.unreadCount.update(c => c + 1);
    });

    this.hubConnection.on('UserPresence', (presence: any) => {
      // Ignore
    });
  }

  stopConnections() {
    this.hubConnection?.stop();
    this.hubConnection = null;
  }
}
