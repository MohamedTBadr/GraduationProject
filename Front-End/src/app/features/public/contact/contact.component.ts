import { Component } from '@angular/core';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-contact',
  standalone: true,
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.scss']
})
export class ContactComponent {

  constructor(private toastService: ToastService) { }

  sendMessage() {
    this.toastService.show('Message sent successfully! We will get back to you shortly.', 'success');
  }
}
