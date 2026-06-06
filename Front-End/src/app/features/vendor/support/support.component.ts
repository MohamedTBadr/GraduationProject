import { Component } from '@angular/core';
import { SupportTicketsHubComponent } from '../../../shared/components/support-tickets-hub/support-tickets-hub.component';

@Component({
  selector: 'app-vendor-support',
  standalone: true,
  imports: [SupportTicketsHubComponent],
  template: `
    <app-support-tickets-hub
      submitterType="Vendor"
      pageTitle="Vendor Support"
      pageSubtitle="Report platform issues, payout questions, or booking disputes."
      emptyHint="No support tickets yet. Open one if you need help from the EpicHub team.">
    </app-support-tickets-hub>
  `,
})
export class VendorSupportComponent {}
