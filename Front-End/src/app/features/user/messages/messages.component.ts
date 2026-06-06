import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ChatService } from '../../../core/services/chat.service';
import { ChatMessage, Conversation } from '../../../shared/types/api.interfaces';
import { Subscription } from 'rxjs';
import { ToastService } from '../../../shared/components/toast/toast.service';

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

  constructor(
    private authService: AuthService,
    private chatService: ChatService,
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

    // Check for vendorId deep-link from My Bookings
    this.route.queryParams.subscribe(params => {
      if (params['vendorId']) {
        this.pendingVendorId = params['vendorId'];
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
        (msg.senderId === this.selectedConversation.userId ||
          msg.receiverId === this.selectedConversation.userId)
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

          const match = this.conversations.find(c => c.userId === vendorId);
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
      },
      error: (err) => {
        this.loadingMessages = false;
        console.error('Failed to load messages', err);
        this.toastService.show('Failed to load messages', 'error');
      }
    });
  }

  sendMessage() {
    const text = this.newMessageText.trim();
    if (!text || !this.selectedConversation) return;

    this.chatService.sendMessage(this.selectedConversation.userId, text)
      .then(() => {
        // Server echoes message back via 'ReceiveMessage' on hub
        this.newMessageText = '';
        this.scrollToBottom();
      })
      .catch((err) => {
        console.error('Error sending message', err);
        this.toastService.show('Failed to send message', 'error');
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
