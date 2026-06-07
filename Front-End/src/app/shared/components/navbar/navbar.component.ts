import { Component, inject, HostListener, OnInit } from '@angular/core';
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
export class NavbarComponent implements OnInit {
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

  @HostListener('document:click')
  onDocumentClick() {
    this.showNotifPanel = false;
  }

  ngOnInit() {
    if (this.authService.isLoggedIn() && this.authService.role() !== 'Admin') {
      this.signalRService.refreshNotifications();
    }
  }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  logout() {
    this.authService.logout();
  }

  openLoginModal() {
    this.modalService.open('login');
  }

  openSignupModal() {
    this.modalService.open('signup');
  }

  goToFavorites() {
    this.router.navigate(['/favorites']);
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
    if (this.showNotifPanel) {
      this.signalRService.refreshNotifications();
    }
  }

  goToNotificationsPage(): void {
    const role = this.authService.role();
    const route =
      role === 'Vendor' ? '/vendor-dashboard/notifications'
      : role === 'Admin' ? '/admin/notifications'
      : '/user/notifications';
    this.router.navigate([route]);
    this.showNotifPanel = false;
  }

  constructor(public theme: ThemeService) {}
}
