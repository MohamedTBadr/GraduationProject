import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss'
})
export class ChangePasswordComponent {
  private authService = inject(AuthService);
  private router = inject(Router);

  /** Backend POST /Authentication/ChangePassword is not available yet. */
  readonly comingSoon = true;

  goBack() {
    const role = this.authService.role();
    if (role === 'Admin') this.router.navigate(['/admin']);
    else if (role === 'Vendor') this.router.navigate(['/vendor-dashboard']);
    else this.router.navigate(['/user/dashboard']);
  }
}
