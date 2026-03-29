import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss'
})
export class ChangePasswordComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);
  private router = inject(Router);

  changeForm: FormGroup = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(6), Validators.pattern(/.*[a-z].*/)]]
  });

  isLoading = false;

  get currentPasswordControl() {
    return this.changeForm.get('currentPassword');
  }

  get newPasswordControl() {
    return this.changeForm.get('newPassword');
  }

  onSubmit() {
    if (this.changeForm.invalid) {
      this.changeForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;

    this.authService.changePassword({
      currentPassword: this.changeForm.value.currentPassword,
      newPassword: this.changeForm.value.newPassword
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.toastService.show('Password updated successfully', 'success');
        
        // redirect back to suitable dashboard
        const role = this.authService.role();
        if (role === 'Admin') this.router.navigate(['/admin']);
        else if (role === 'Vendor') this.router.navigate(['/vendor-dashboard']);
        else this.router.navigate(['/user/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        this.toastService.show(err.message || 'Failed to change password', 'error');
      }
    });
  }

  cancel() {
    const role = this.authService.role();
    if (role === 'Admin') this.router.navigate(['/admin']);
    else if (role === 'Vendor') this.router.navigate(['/vendor-dashboard']);
    else this.router.navigate(['/user/dashboard']);
  }
}
