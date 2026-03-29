import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.scss'
})
export class ResetPasswordComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  resetForm: FormGroup = this.fb.group({
    newPassword: ['', [Validators.required, Validators.minLength(6), Validators.pattern(/.*[a-z].*/)]]
  });

  isLoading = false;
  email = '';
  token = '';

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
      this.token = params['token'] || '';

      if (!this.email || !this.token) {
        this.toastService.show('Invalid password reset link.', 'error');
        this.router.navigate(['/']);
      }
    });
  }

  get passwordControl() {
    return this.resetForm.get('newPassword');
  }

  onSubmit() {
    if (this.resetForm.invalid) {
      this.resetForm.markAllAsTouched();
      return;
    }

    if (!this.email || !this.token) return;

    this.isLoading = true;
    const newPassword = this.resetForm.value.newPassword;

    this.authService.resetPassword({
      email: this.email,
      token: this.token,
      newPassword
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.toastService.show('Password updated successfully', 'success');
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.isLoading = false;
        this.toastService.show(err.message || 'Failed to reset password', 'error');
      }
    });
  }
}
