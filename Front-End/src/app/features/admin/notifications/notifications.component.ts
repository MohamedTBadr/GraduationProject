import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-notifications',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.scss'
})
export class NotificationsComponent {
  audience  = 'All Vendors';
  subject   = '';
  message   = '';
  channels  = { push: true, email: true };

  constructor(private toastService: ToastService) {}

  sendBroadcast() {
    // Backend broadcast endpoint not yet implemented.
    // Required: POST /api/admin/notifications/broadcast
    this.toastService.show('Broadcast endpoint pending backend implementation', 'error');
  }
}
