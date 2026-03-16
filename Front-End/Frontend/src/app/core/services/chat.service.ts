import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
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

  /** GET /messages/{otherUserId} */
  getMessages(otherUserId: string): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(`${this.apiUrl}/messages/${otherUserId}`);
  }

  /** GET /conversations */
  getConversations(): Observable<Conversation[]> {
    return this.http.get<Conversation[]>(`${this.apiUrl}/conversations`);
  }

  // ─────────────────────────────────────────────
  // SignalR real-time connection
  // ─────────────────────────────────────────────

  /** Starts a persistent WebSocket connection to the messages hub */
  startConnection(): void {
    if (this.hubConnection) return; // already connected

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalRUrl, {
        accessTokenFactory: () => this.authService.getToken() ?? ''
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    // Listen for incoming messages
    this.hubConnection.on('ReceiveMessage', (message: ChatMessage) => {
      this.messageReceived$.next(message);
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
