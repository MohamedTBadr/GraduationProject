import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Message {
  text: string;
  time: string;
  isMe: boolean;
}

interface Chat {
  id: number;
  sender: string;
  avatar: string;
  lastMessage: string;
  time: string;
  unread: boolean;
  history: Message[];
}

import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent {
  constructor(private authService: AuthService) {}

  chats: Chat[] = [
    {
      id: 1,
      sender: 'White Rose Decor',
      avatar: 'W',
      lastMessage: 'The quote for your wedding is ready...',
      time: '10:30 AM',
      unread: true,
      history: [
        { text: `Hi ${this.authService.user()?.name?.split(' ')[0] || 'there'}, congratulations on your upcoming event!`, time: '10:00 AM', isMe: false },
        { text: 'Thank you! I was wondering if the floral setup is included.', time: '11:15 AM', isMe: true },
        { text: 'The quote for your wedding is ready. It includes all major floral arrangements.', time: '1:30 PM', isMe: false }
      ]
    },
    {
      id: 2,
      sender: 'Studio Lens',
      avatar: 'S',
      lastMessage: 'Great! See you then.',
      time: 'Yesterday',
      unread: false,
      history: [
        { text: 'Hello, are you available on Dec 5th?', time: 'Yesterday 2:00 PM', isMe: true },
        { text: 'Yes, we have one slot remaining. Great! See you then.', time: 'Yesterday 4:00 PM', isMe: false }
      ]
    }
  ];

  selectedChat = this.chats[0];
  newMessageText = '';

  selectChat(chat: Chat) {
    this.selectedChat = chat;
    chat.unread = false;
  }

  sendMessage(text: string) {
    if (!text.trim()) return;

    const msg: Message = {
      text: text,
      time: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
      isMe: true
    };

    this.selectedChat.history.push(msg);
    this.selectedChat.lastMessage = text;
    this.newMessageText = '';
  }
}
