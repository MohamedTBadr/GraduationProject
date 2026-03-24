import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ServiceCardComponent } from '../../../shared/components/service-card/service-card.component';
import { VendorService } from '../../../shared/types/vendor.interface';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule, ServiceCardComponent],
  templateUrl: './services.component.html',
  styleUrls: ['./services.component.scss']
})
export class ServicesComponent {
  services: VendorService[] = [
    { 
      icon: '💐', 
      name: 'Wedding Stage Floral Design', 
      price: 'From 15,000', 
      desc: 'Full stage arrangement — arch, backdrop, aisle décor, and bridal entrance. Premium fresh flowers sourced daily.',
      duration: '4–6 hrs',
      images: ['https://images.unsplash.com/photo-1519225421980-715cb0215aed?ixlib=rb-4.0.3&auto=format&fit=crop&w=400&q=80']
    },
    { 
      icon: '🌹', 
      name: 'Table Centerpieces', 
      price: 'From 1,200', 
      desc: 'Fresh flower centerpieces per table. Wide variety of arrangements to match your wedding theme.',
      duration: '2–3 hrs'
    },
    { 
      icon: '🎈', 
      name: 'Balloon Art & Setup', 
      price: 'From 3,500', 
      desc: 'Custom balloon arches, organic installations, and full room setups. Latex, foil, and biodegradable options.',
      duration: '2–4 hrs',
      images: ['https://images.unsplash.com/photo-1530103862676-de3c9da59c6b?ixlib=rb-4.0.3&auto=format&fit=crop&w=400&q=80', 'https://images.pexels.com/photos/796606/pexels-photo-796606.jpeg?auto=compress&cs=tinysrgb&w=400']
    }
  ];

  handleAction(action: string, service: VendorService) {
    if (action === 'delete') {
      this.services = this.services.filter(s => s !== service);
    } else if (action === 'edit') {
      console.log('Editing service:', service.name);
    }
  }
}
