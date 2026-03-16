import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService, ToastMessage } from './toast.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrls: ['./toast.component.scss']
})
export class ToastComponent implements OnInit, OnDestroy {
  toast: ToastMessage | null = null;
  isVisible = false;
  private sub?: Subscription;
  private timeoutId: any;

  constructor(private toastService: ToastService) { }

  ngOnInit() {
    this.sub = this.toastService.toast$.subscribe(toast => {
      this.toast = toast;
      this.isVisible = true;

      // Clear previous timeout
      if (this.timeoutId) {
        clearTimeout(this.timeoutId);
      }

      this.timeoutId = setTimeout(() => {
        this.close();
      }, toast.duration || 3000);
    });
  }

  close() {
    this.isVisible = false;
  }

  ngOnDestroy() {
    if (this.sub) this.sub.unsubscribe();
    if (this.timeoutId) clearTimeout(this.timeoutId);
  }
}
