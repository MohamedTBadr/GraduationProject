import { Component, inject, HostListener } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ModalService } from '../../services/modal.service';
import { AuthService } from '../../../core/services/auth.service';
import { ThemeService } from '../../../services/theme.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { FavoriteService } from '../../services/favorite.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss'],
})
export class NavbarComponent {
  isMenuOpen = false;
  isScrolled = false;
  showNotifPanel = false;

  private modalService = inject(ModalService);
  private router = inject(Router);
  authService = inject(AuthService);
  signalRService = inject(SignalRService);
  favoriteService = inject(FavoriteService);

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled = window.scrollY > 50;
  }

  @HostListener('document:keydown.escape')
  onEscape() {
    this.showNotifPanel = false;
  }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  logout() {
    this.signalRService.stopConnections();
    this.authService.logout();
  }

  openLoginModal() {
    this.modalService.open('login');
  }

  openSignupModal() {
    this.modalService.open('signup');
  }

  goToFavorites() {
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/user/favorites']);
    } else {
      this.modalService.open('login');
    }
  }

  goToMessages() {
    if (this.authService.isLoggedIn()) {
      const route = this.authService.role() === 'Vendor' ? '/vendor-dashboard/messages' : '/user/messages';
      this.router.navigate([route]);
    }
  }

  toggleNotificationPanel(event: Event) {
    event.stopPropagation();
    this.showNotifPanel = !this.showNotifPanel;
    // Reset unread count when panel is opened
    if (this.showNotifPanel) {
      this.signalRService.unreadCount.set(0);
    }
  }

  closeNotificationPanel(event?: Event) {
    this.showNotifPanel = false;
  }

  clearNotifications() {
    this.signalRService.notifications.set([]);
    this.signalRService.unreadCount.set(0);
    this.showNotifPanel = false;
  }

  constructor(public theme: ThemeService) {}
}
