import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ChatService } from '../../../core/services/chat.service';
import { ChatMessage, Conversation } from '../../../shared/types/api.interfaces';
import { Subscription, firstValueFrom } from 'rxjs';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ChatLaunchService } from '../../../core/services/chat-launch.service';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent implements OnInit, OnDestroy {
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;

  conversations: Conversation[] = [];
  selectedConversation: Conversation | null = null;
  messages: ChatMessage[] = [];
  newMessageText = '';
  currentUserId = '';
  searchTerm = '';
  loadingConversations = false;
  loadingMessages = false;

  private messageSub?: Subscription;

  private pendingVendorId: string | null = null;
  private pendingVendorName: string | null = null;
  private pendingInitialMessage: string | null = null;
  private sendingInitialMessage = false;

  constructor(
    private authService: AuthService,
    private chatService: ChatService,
    private chatLaunchService: ChatLaunchService,
    private toastService: ToastService,
    private route: ActivatedRoute
  ) {}

  get filteredConversations(): Conversation[] {
    if (!this.searchTerm.trim()) return this.conversations;
    const term = this.searchTerm.toLowerCase();
    return this.conversations.filter(c =>
      c.userName?.toLowerCase().includes(term) ||
      c.lastMessage?.toLowerCase().includes(term)
    );
  }

  ngOnInit(): void {
    this.currentUserId = this.authService.user()?.id || '';
    this.applyPendingLaunch();
    this.applyRouteQueryParams();

    this.route.queryParams.subscribe(params => {
      if (params['vendorId']) {
        this.pendingVendorId = String(params['vendorId']);
        this.pendingVendorName = params['vendorName'] || null;
      }
    });

    // Start SignalR connection for real-time messages
    this.chatService.startConnection();

    this.loadConversations();

    // Subscribe to real-time incoming messages
    this.messageSub = this.chatService.onMessageReceived$.subscribe((msg) => {
      if (
        this.selectedConversation &&
        this.isSameThread(msg, this.selectedConversation.userId)
      ) {
        if (!this.messages.some(m => m.id === msg.id)) {
          this.messages.push(msg);
          this.scrollToBottom();
        }
      }
      // Refresh sidebar to update last message preview
      this.loadConversations(false);
    });
  }

  private isSameThread(msg: ChatMessage, otherUserId: string): boolean {
    const other = otherUserId.toLowerCase();
    return (
      msg.senderId.toLowerCase() === other ||
      msg.receiverId.toLowerCase() === other
    );
  }

  isOutgoing(msg: ChatMessage): boolean {
    return msg.senderId.toLowerCase() === this.currentUserId.toLowerCase();
  }

  private applyPendingLaunch(): void {
    const launch = this.chatLaunchService.consumePending();
    if (!launch) return;

    this.pendingVendorId = launch.vendorId;
    this.pendingVendorName = launch.vendorName ?? null;
    this.pendingInitialMessage = launch.initialMessage;
  }

  private applyRouteQueryParams(): void {
    const params = this.route.snapshot.queryParams;
    if (params['vendorId']) {
      this.pendingVendorId = String(params['vendorId']);
      this.pendingVendorName = params['vendorName'] || null;
    }
  }

  private sameUserId(a: string, b: string): boolean {
    return a.toLowerCase() === b.toLowerCase();
  }

  ngOnDestroy(): void {
    this.messageSub?.unsubscribe();
  }

  loadConversations(showLoader = true): void {
    if (showLoader) this.loadingConversations = true;
    this.chatService.getConversations().subscribe({
      next: (data) => {
        this.conversations = data || [];
        this.loadingConversations = false;

        // Auto-select vendor if navigated from My Bookings (existing or new thread)
        if (this.pendingVendorId) {
          const vendorId = this.pendingVendorId;
          const vendorName = this.pendingVendorName;
          this.pendingVendorId = null;
          this.pendingVendorName = null;

          const match = this.conversations.find(c => this.sameUserId(c.userId, vendorId));
          if (match) {
            this.selectChat(match);
            return;
          }

          const newConversation: Conversation = {
            userId: vendorId,
            userName: vendorName || 'Vendor',
            unreadCount: 0,
          };
          this.conversations.unshift(newConversation);
          this.selectChat(newConversation);
          return;
        }

        // Auto-select first conversation if none selected
        if (!this.selectedConversation && this.conversations.length > 0) {
          this.selectChat(this.conversations[0]);
        }
      },
      error: (err) => {
        this.loadingConversations = false;
        console.error('Failed to load conversations', err);
        this.toastService.show('Failed to load conversations', 'error');
      }
    });
  }

  selectChat(chat: Conversation) {
    this.selectedConversation = chat;
    this.loadingMessages = true;
    this.messages = [];
    this.chatService.getMessages(chat.userId).subscribe({
      next: (msgs) => {
        this.messages = msgs || [];
        chat.unreadCount = 0;
        this.chatService.markConversationAsRead(this.messages, this.currentUserId);
        this.loadingMessages = false;
        this.scrollToBottom();
        this.sendPendingInitialMessage();
      },
      error: (err) => {
        this.loadingMessages = false;
        console.error('Failed to load messages', err);
        this.toastService.show('Failed to load messages', 'error');
        this.sendPendingInitialMessage();
      }
    });
  }

  private sendPendingInitialMessage(): void {
    const text = this.pendingInitialMessage?.trim();
    if (!text || !this.selectedConversation || this.sendingInitialMessage) return;

    const receiverId = this.selectedConversation.userId;
    this.pendingInitialMessage = null;
    this.sendingInitialMessage = true;

    void this.chatService.ensureConnected()
      .then(() => this.chatService.sendMessage(receiverId, text))
      .then(() => firstValueFrom(this.chatService.getMessages(receiverId)))
      .then((msgs) => {
        this.messages = msgs || [];
        this.scrollToBottom();
        this.loadConversations(false);
      })
      .catch((err) => {
        console.error('Failed to send initial message', err);
        this.pendingInitialMessage = text;
        this.newMessageText = text;
        this.toastService.show(
          err?.message?.includes('timed out')
            ? 'Chat connection is still starting — tap send to try again'
            : 'Failed to send message — tap send to try again',
          'error'
        );
      })
      .finally(() => {
        this.sendingInitialMessage = false;
      });
  }

  sendMessage() {
    const text = this.newMessageText.trim();
    if (!text || !this.selectedConversation || this.sendingInitialMessage) return;

    const receiverId = this.selectedConversation.userId;
    this.chatService.sendMessage(receiverId, text)
      .then(() => firstValueFrom(this.chatService.getMessages(receiverId)))
      .then((msgs) => {
        this.messages = msgs || [];
        this.newMessageText = '';
        this.scrollToBottom();
        this.loadConversations(false);
      })
      .catch((err) => {
        console.error('Error sending message', err);
        this.toastService.show(
          err?.message?.includes('timed out')
            ? 'Chat connection is still starting — please try again'
            : 'Failed to send message',
          'error'
        );
      });
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.scrollContainer) {
        this.scrollContainer.nativeElement.scrollTop =
          this.scrollContainer.nativeElement.scrollHeight;
      }
    }, 80);
  }
}
