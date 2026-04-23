import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { ChatMessage, Conversation } from '../../shared/types/api.interfaces';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ChatService implements OnDestroy {
  private readonly apiUrl = environment.apiUrl;
  private hubConnection: signalR.HubConnection | null = null;

  /** Emits whenever a new real-time message arrives via SignalR */
  private messageReceived$ = new Subject<ChatMessage>();
  onMessageReceived$ = this.messageReceived$.asObservable();

  constructor(private http: HttpClient, private authService: AuthService) {}

  // ─────────────────────────────────────────────
  // REST endpoints
  // ─────────────────────────────────────────────

  /** GET /Chat/messages/{otherUserId} */
  getMessages(otherUserId: string): Observable<ChatMessage[]> {
    return this.http.get<any>(`${this.apiUrl}/Chat/messages/${otherUserId}`).pipe(
      map(res => res.value || res)
    );
  }

  /** GET /Chat/conversations */
  getConversations(): Observable<Conversation[]> {
    return this.http.get<any>(`${this.apiUrl}/Chat/conversations`).pipe(
      map(res => res.value || res)
    );
  }

  // ─────────────────────────────────────────────
  // SignalR real-time connection
  // ─────────────────────────────────────────────

  /** Starts a persistent WebSocket connection to the messages hub */
  startConnection(): void {
    if (this.hubConnection) return; // already connected

    const token = this.authService.getToken() ?? '';
    const connectionUrl = `${environment.signalRUrl}?accessToken=${token}`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(connectionUrl, {
        accessTokenFactory: () => token,
        headers: { 'IdempotencyKey': crypto.randomUUID ? crypto.randomUUID() : Date.now().toString() }
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    // Listen for incoming messages
    this.hubConnection.on('ReceiveMessage', (message: ChatMessage) => {
      this.messageReceived$.next(message);
    });

    // Listen for presence events to suppress warnings
    this.hubConnection.on('UserPresence', (presence: any) => {
      // console.log('[ChatService] User presence updated:', presence);
    });

    this.hubConnection
      .start()
      .then(() => console.log('[ChatService] SignalR connected'))
      .catch((err) => console.error('[ChatService] SignalR connection error:', err));
  }

  /** Sends a message through the SignalR hub */
  sendMessage(receiverId: string, content: string): Promise<void> {
    if (!this.hubConnection) {
      return Promise.reject(new Error('SignalR not connected. Call startConnection() first.'));
    }
    return this.hubConnection.invoke('SendMessage', receiverId, content);
  }

  /** Stops the SignalR connection and cleans up */
  stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop().then(() => {
        console.log('[ChatService] SignalR disconnected');
        this.hubConnection = null;
      });
    }
  }

  ngOnDestroy(): void {
    this.stopConnection();
    this.messageReceived$.complete();
  }
}
