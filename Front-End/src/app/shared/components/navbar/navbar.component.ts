import { Component, inject, HostListener } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ModalService } from '../../services/modal.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {

  isMenuOpen = false;
  isScrolled = false; 

  private modalService = inject(ModalService);
  private router = inject(Router);
  authService = inject(AuthService);

  //  scroll listener
  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.isScrolled = window.scrollY > 50; 
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
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/user/favorites']);
    } else {
      this.modalService.open('login');
    }
  }
}