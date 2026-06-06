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
      pageSubtitle="Report platform issues, payout questions, or booking disputes. We typically respond within 1–2 business days."
      emptyHint="No tickets yet. Open one above if you need help from our team.">
    </app-support-tickets-hub>
  `,
})
export class VendorSupportComponent {}
