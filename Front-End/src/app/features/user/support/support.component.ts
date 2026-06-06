import { Component } from '@angular/core';
import { SupportTicketsHubComponent } from '../../../shared/components/support-tickets-hub/support-tickets-hub.component';

@Component({
  selector: 'app-user-support',
  standalone: true,
  imports: [SupportTicketsHubComponent],
  template: `
    <app-support-tickets-hub
      submitterType="Client"
      pageTitle="Help & Support"
      pageSubtitle="Get help with bookings, payments, or technical issues."
      emptyHint="No support tickets yet. You can also report an issue from My Bookings.">
    </app-support-tickets-hub>
  `,
})
export class UserSupportComponent {}
