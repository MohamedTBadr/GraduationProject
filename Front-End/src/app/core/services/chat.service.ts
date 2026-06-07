import { Injectable, OnDestroy, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject, Subscription } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ChatMessage, Conversation } from '../../shared/types/api.interfaces';
import { SignalRService } from './signalr.service';

@Injectable({ providedIn: 'root' })
export class ChatService implements OnDestroy {
  private readonly apiUrl = environment.apiUrl;
  private readonly http = inject(HttpClient);
  private readonly signalR = inject(SignalRService);

  /** Emits whenever a new real-time message arrives via SignalR */
  private messageReceived$ = new Subject<ChatMessage>();
  onMessageReceived$ = this.messageReceived$.asObservable();
  private chatBridgeSub?: Subscription;

  // ─────────────────────────────────────────────
  // REST endpoints
  // ─────────────────────────────────────────────

  /** GET /Chat/messages/{otherUserId} */
  getMessages(otherUserId: string): Observable<ChatMessage[]> {
    return this.http.get<any>(`${this.apiUrl}/Chat/messages/${otherUserId}`).pipe(
      map(res => {
        const list = (res.value || res) as unknown[];
        return (list || [])
          .map(item => this.normalizeMessage(item as Record<string, unknown>))
          .sort(
            (a, b) =>
              new Date(a.sentAt || 0).getTime() - new Date(b.sentAt || 0).getTime()
          );
      })
    );
  }

  /** GET /Chat/conversations */
  getConversations(): Observable<Conversation[]> {
    return this.http.get<any>(`${this.apiUrl}/Chat/conversations`).pipe(
      map(res => {
        const list = (res.value || res) as unknown[];
        return (list || []).map(item =>
          this.normalizeConversation(item as Record<string, unknown>)
        );
      })
    );
  }

  /** Maps API ConversationDto (otherUserId, nested lastMessage) to UI Conversation */
  private normalizeConversation(raw: Record<string, unknown>): Conversation {
    if (raw['userId']) {
      return raw as unknown as Conversation;
    }

    const lastMsg = raw['lastMessage'] as Record<string, unknown> | string | null | undefined;
    const lastMessageText =
      typeof lastMsg === 'string'
        ? lastMsg
        : lastMsg
          ? this.normalizeMessage(lastMsg).content
          : undefined;
    const lastMessageAt =
      typeof lastMsg === 'object' && lastMsg?.['sentAt']
        ? String(lastMsg['sentAt'])
        : (raw['lastMessageAt'] as string | undefined);

    return {
      userId: String(raw['otherUserId'] ?? raw['userId'] ?? ''),
      userName: (raw['otherUserName'] ?? raw['userName']) as string | undefined,
      lastMessage: lastMessageText,
      lastMessageAt,
      unreadCount: (raw['unreadCount'] as number | undefined) ?? 0,
    };
  }

  private normalizeMessage(raw: Record<string, unknown>): ChatMessage {
    return {
      id: String(raw['id'] ?? raw['Id'] ?? ''),
      senderId: String(raw['senderId'] ?? raw['SenderId'] ?? ''),
      receiverId: String(raw['receiverId'] ?? raw['ReceiverId'] ?? ''),
      content: String(raw['content'] ?? raw['Content'] ?? ''),
      sentAt: (raw['sentAt'] ?? raw['SentAt']) as string | undefined,
      isRead: (raw['isRead'] ?? raw['IsRead']) as boolean | undefined,
    };
  }

  // ─────────────────────────────────────────────
  // SignalR (shared hub via SignalRService)
  // ─────────────────────────────────────────────

  /** Ensures the shared chat hub is connected and bridges incoming messages */
  startConnection(): void {
    this.signalR.startConnections();

    if (!this.chatBridgeSub) {
      this.chatBridgeSub = this.signalR.chatMessageReceived.subscribe((msg) => {
        this.messageReceived$.next(this.normalizeMessage(msg as Record<string, unknown>));
      });
    }
  }

  /** Sends a message through the SignalR hub */
  async sendMessage(receiverId: string, content: string): Promise<void> {
    await this.signalR.ensureHubConnected();
    await this.signalR.invokeHub('SendMessage', receiverId, content);
  }

  /** Marks a message as read via the SignalR hub */
  async markAsRead(messageId: string): Promise<void> {
    if (!messageId) return;
    await this.signalR.ensureHubConnected();
    await this.signalR.invokeHub('MarkAsRead', messageId);
  }

  /** Marks all unread messages in a thread as read for the current user */
  markConversationAsRead(messages: ChatMessage[], currentUserId: string): void {
    const readerId = currentUserId.toLowerCase();
    messages
      .filter(m => m.id && m.receiverId.toLowerCase() === readerId && !m.isRead)
      .forEach(m => {
        this.markAsRead(m.id!).catch(() => {});
      });
  }

  /** Waits until the shared hub is ready (for deep-link auto-send flows) */
  ensureConnected(): Promise<void> {
    this.startConnection();
    return this.signalR.ensureHubConnected();
  }

  ngOnDestroy(): void {
    this.chatBridgeSub?.unsubscribe();
    this.messageReceived$.complete();
  }
}
