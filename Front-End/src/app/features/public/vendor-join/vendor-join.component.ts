import { Component } from '@angular/core';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-vendor-join',
  standalone: true,
  templateUrl: './vendor-join.component.html',
  styleUrls: ['./vendor-join.component.scss']
})
export class VendorJoinComponent {

  constructor(private toastService: ToastService) { }

  submitVendorApplication() {
    this.toastService.show('Application submitted! Our team will review it and contact you within 48 hours.', 'success');
  }
}
