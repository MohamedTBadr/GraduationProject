import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalService } from '../../services/modal.service';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../types/auth.interface';
import { Router } from '@angular/router';
import { LoginComponent } from '../../../features/auth/login/login.component';
import { RegisterComponent } from '../../../features/auth/register/register.component';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule, LoginComponent, RegisterComponent],
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss']
})
export class ModalComponent {
  modalService = inject(ModalService);
  authService = inject(AuthService);
  router = inject(Router);

  mockLogin(role: UserRole) {
    this.authService.login({ email: `${role}@example.com`, role }).subscribe();
    this.modalService.close();

    // Navigate based on role
    if (role === 'admin') this.router.navigate(['/admin']);
    else if (role === 'vendor') this.router.navigate(['/vendor-dashboard']);
    else if (role === 'user') this.router.navigate(['/user/my-events']);
  }

  onOverlayClick(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('modal-overlay')) {
      this.modalService.close();
    }
  }
}
