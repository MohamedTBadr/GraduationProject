import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface VendorPackageItem {
  id: number;
  icon: string;
  name: string;
  price: string;
  desc: string;
}

@Component({
  selector: 'app-packages',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './packages.component.html',
  styleUrls: ['./packages.component.scss']
})
export class PackagesComponent {
  packages: VendorPackageItem[] = [
    { id: 1, icon: '', name: 'Premium Wedding Pack', price: '45,000 EGP', desc: 'All-inclusive decoration, lighting, and coordination for up to 300 guests.' },
    { id: 2, icon: '', name: 'Social Event Bundle', price: '12,000 EGP', desc: 'Essential decor and sound system for engagement parties or birthdays.' }
  ];

  deletePackage(id: number) {
    this.packages = this.packages.filter(p => p.id !== id);
  }

  addPackage() {
    const newId = Math.max(...this.packages.map(p => p.id)) + 1;
    this.packages.push({
      id: newId,
      icon: '',
      name: 'New Custom Package',
      price: 'Price on request',
      desc: 'Bundle your services into a value-packed offering here.'
    });
  }
}
