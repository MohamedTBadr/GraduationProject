import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ModalService } from '../../../shared/services/modal.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastService = inject(ToastService);
  private modalService = inject(ModalService);

  registerForm: FormGroup = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required],
    password: ['', [Validators.required, Validators.minLength(8)]],
    role: ['user', Validators.required]
  });

  isLoading = false;

  get emailControl() { return this.registerForm.get('email'); }
  get passwordControl() { return this.registerForm.get('password'); }
  get phoneControl() { return this.registerForm.get('phone'); }

  onSubmit() {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;

    this.authService.register(this.registerForm.value).subscribe({
      next: (response) => {
        this.isLoading = false;
        this.toastService.show('Account created successfully!', 'success');
        this.modalService.close(); // Close modal upon success

        // redirect based on role
        if (response.value.role === 'Admin') {
          this.router.navigate(['/admin']);
        } else if (response.value.role === 'Vendor') {
          this.router.navigate(['/vendor-dashboard']);
        } else {
          this.router.navigate(['/user/my-events']);
        }
      },
      error: (err: any) => {
        this.isLoading = false;
        this.toastService.show(err.message || 'Failed to create account', 'error');
      }
    });
  }

  switchToLogin() {
    this.modalService.open('login');
  }
}
