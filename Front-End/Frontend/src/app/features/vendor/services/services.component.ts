import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface VendorServiceItem {
  id: number;
  icon: string;
  name: string;
  price: string;
  desc: string;
}

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './services.component.html',
  styleUrls: ['./services.component.scss']
})
export class ServicesComponent {
  services: VendorServiceItem[] = [
    { id: 1, icon: '', name: 'Premium Decoration', price: '15,000 EGP', desc: 'Full event decor including entrance, tables, and stage.' },
    { id: 2, icon: '✨', name: 'Standard Setup', price: '9,000 EGP', desc: 'Core floral and lighting setup for medium events.' },
    { id: 3, icon: '', name: 'Lighting Package', price: '4,500 EGP', desc: 'Specialized uplighting and fairy lights installation.' }
  ];

  deleteService(id: number) {
    this.services = this.services.filter(s => s.id !== id);
  }

  addService() {
    // Logic for new service (could open modal)
    const newId = Math.max(...this.services.map(s => s.id)) + 1;
    this.services.push({
      id: newId,
      icon: '🆕',
      name: 'New Custom Service',
      price: 'Price on request',
      desc: 'Set your description and pricing details here.'
    });
  }
}
