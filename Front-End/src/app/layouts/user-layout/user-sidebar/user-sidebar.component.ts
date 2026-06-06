import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ChatService } from '../../../core/services/chat.service';

@Component({
  selector: 'app-user-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './user-sidebar.component.html',
  styleUrl: './user-sidebar.component.scss'
})
export class UserSidebarComponent implements OnInit {
  unreadMessagesCount = 0;

  constructor(
    public authService: AuthService,
    private chatService: ChatService
  ) {}

  ngOnInit(): void {
    this.chatService.getConversations().subscribe({
      next: (conversations) => {
        this.unreadMessagesCount = (conversations || []).reduce(
          (sum, c) => sum + (c.unreadCount ?? 0),
          0
        );
      },
      error: () => { this.unreadMessagesCount = 0; }
    });
  }
}
