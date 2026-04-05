import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { ModalService } from '../../../shared/services/modal.service';
import { HttpHeaders } from '@angular/common/http';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterModule],
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent {

    private fb = inject(FormBuilder);
    private authService = inject(AuthService);
    private router = inject(Router);
    private toastService = inject(ToastService);
    private modalService = inject(ModalService);

    loginForm: FormGroup = this.fb.group({
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(6)]],
        role: ['user', Validators.required]
    });

    isLoading = false;

    get emailControl() {
        return this.loginForm.get('email');
    }

    get passwordControl() {
        return this.loginForm.get('password');
    }

    onSubmit() {

        if (this.loginForm.invalid) {
            this.loginForm.markAllAsTouched();
            return;
        }

        this.isLoading = true;

        this.authService.login(this.loginForm.value).subscribe({

            next: (response) => {

                this.isLoading = false;

                this.toastService.show('Logged in successfully!', 'success');
                
                this.modalService.close();
                
                if (response.value.role === 'Admin') {
                    this.router.navigate(['/admin']);
                }
                else if (response.value.role === 'Vendor') {
                    this.router.navigate(['/vendor-dashboard']);
                }
                else if (response.value.role === 'User') {
                    this.router.navigate(['/user/my-events']);
                }
            },

            error: (err: any) => {

                this.isLoading = false;

                this.toastService.show(err.message || 'Failed to login', 'error');
            }
        });
    }

    switchToRegister() {
        this.modalService.open('signup');
    }

    goToForgotPassword() {
        this.modalService.close();
        this.router.navigate(['/forgot-password']);
    }
}