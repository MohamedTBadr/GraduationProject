import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private toastService = inject(ToastService);

  forgotForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  isLoading = false;
  isSubmitted = false;

  get emailControl() {
    return this.forgotForm.get('email');
  }

  onSubmit() {
    if (this.forgotForm.invalid) {
      this.forgotForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    const email = this.forgotForm.value.email;

    this.authService.forgetPassword(email).subscribe({
      next: () => {
        this.isLoading = false;
        this.isSubmitted = true;
        this.toastService.show('If email exists, reset link sent', 'success');
      },
      error: (err) => {
        this.isLoading = false;
        // Even if there's an error, typically the flow says "If email exists, reset link sent" for security reasons.
        // But we handle it accordingly
        this.toastService.show(err.message || 'Failed to request password reset', 'error');
      }
    });
  }
}
