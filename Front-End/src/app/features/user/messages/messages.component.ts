import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
  
  private messageSub?: Subscription;

  constructor(
    private authService: AuthService,
    private chatService: ChatService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.currentUserId = this.authService.user()?.id || '';
    
    // Start SignalR connection
    this.chatService.startConnection();

    this.loadConversations();

    // Listen for real-time messages
    this.messageSub = this.chatService.onMessageReceived$.subscribe((msg) => {
      // If the message belongs to the currently open chat, append it
      if (this.selectedConversation && 
         (msg.senderId === this.selectedConversation.userId || msg.receiverId === this.selectedConversation.userId)) {
        this.messages.push(msg);
        this.scrollToBottom();
      }
      
      // Update conversations list with the new message
      this.loadConversations();
    });
  }

  ngOnDestroy(): void {
    if (this.messageSub) {
      this.messageSub.unsubscribe();
    }
  }

  loadConversations(): void {
    this.chatService.getConversations().subscribe({
      next: (data) => {
        this.conversations = data || [];
        // Optional: auto-select first conversation
        if (!this.selectedConversation && this.conversations.length > 0) {
          this.selectChat(this.conversations[0]);
        }
      },
      error: (err) => {
        console.error('Failed to load conversations', err);
        this.toastService.show('Failed to load conversations', 'error');
      }
    });
  }

  selectChat(chat: Conversation) {
    this.selectedConversation = chat;
    this.chatService.getMessages(chat.userId).subscribe({
      next: (msgs) => {
        this.messages = msgs || [];
        chat.unreadCount = 0; // optimistic clear
        this.scrollToBottom();
      },
      error: (err) => {
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
        // Optimistically add message
        const optimisticMsg: ChatMessage = {
          senderId: this.currentUserId,
          receiverId: this.selectedConversation!.userId,
          content: text,
          sentAt: new Date().toISOString()
        };
        this.messages.push(optimisticMsg);
        this.selectedConversation!.lastMessage = text;
        this.selectedConversation!.lastMessageAt = optimisticMsg.sentAt;
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
        this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
      }
    }, 100);
  }
}
