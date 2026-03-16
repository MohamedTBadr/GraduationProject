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

@Component({
  selector: 'app-messages',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss']
})
export class MessagesComponent {
  chats: Chat[] = [
    {
      id: 1,
      sender: 'Sara Ahmed',
      avatar: 'S',
      lastMessage: 'Thank you! I was wondering if the floral setup...',
      time: '11:15 AM',
      unread: true,
      history: [
        { text: 'Hi Sara, congratulations on your upcoming event!', time: '10:00 AM', isMe: true },
        { text: 'Thank you! I was wondering if the floral setup is included.', time: '11:15 AM', isMe: false },
        { text: 'The quote for your wedding is ready. It includes all major floral arrangements.', time: '1:30 PM', isMe: true }
      ]
    },
    {
      id: 2,
      sender: 'Youssef & Nada',
      avatar: 'Y',
      lastMessage: 'Great, we will proceed with the booking.',
      time: 'Yesterday',
      unread: false,
      history: [
        { text: 'Hello, are you available on Dec 5th?', time: 'Yesterday 2:00 PM', isMe: false },
        { text: 'Yes, we have one slot remaining. Great, we will proceed with the booking.', time: 'Yesterday 4:00 PM', isMe: true }
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
