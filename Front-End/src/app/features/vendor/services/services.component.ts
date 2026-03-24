import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ServiceCardComponent } from '../../../shared/components/service-card/service-card.component';
import { VendorService } from '../../../shared/types/vendor.interface';
import { ImageUploadComponent } from '../../../shared/components/image-upload/image-upload.component';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [CommonModule, ServiceCardComponent, FormsModule, ImageUploadComponent],
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
      images: ['https://images.unsplash.com/photo-1519225421980-715cb0215aed?ixlib=rb-4.0.3&auto=format&fit=crop&w=400&q=80'],
      status: 'active'
    },
    { 
      icon: '🌹', 
      name: 'Table Centerpieces', 
      price: 'From 1,200', 
      desc: 'Fresh flower centerpieces per table. Wide variety of arrangements to match your wedding theme.',
      duration: '2–3 hrs',
      status: 'active'
    },
    { 
      icon: '🎈', 
      name: 'Balloon Art & Setup', 
      price: 'From 3,500', 
      desc: 'Custom balloon arches, organic installations, and full room setups. Latex, foil, and biodegradable options.',
      duration: '2–4 hrs',
      images: ['https://images.unsplash.com/photo-1530103862676-de3c9da59c6b?ixlib=rb-4.0.3&auto=format&fit=crop&w=400&q=80', 'https://images.pexels.com/photos/796606/pexels-photo-796606.jpeg?auto=compress&cs=tinysrgb&w=400'],
      status: 'paused'
    }
  ];

  activeTab: 'active' | 'paused' = 'active';

  get filteredServices() {
    return this.services.filter(s => s.status === this.activeTab || (!s.status && this.activeTab === 'active'));
  }

  getActiveCount(): number {
    return this.services.filter(s => s.status === 'active' || !s.status).length;
  }

  getPausedCount(): number {
    return this.services.filter(s => s.status === 'paused').length;
  }

  isAddServiceModalOpen = false;
  editingIndex: number = -1;
  
  newService = {
    name: '',
    category: 'Decoration',
    price: '',
    description: '',
    duration: '',
    leadTime: '',
    images: [] as any[]
  };

  isDetailModalOpen = false;
  selectedService: VendorService | null = null;

  openAddServiceModal(serviceToEdit?: VendorService) {
    if (serviceToEdit) {
      this.editingIndex = this.services.indexOf(serviceToEdit);
      this.newService = {
        name: serviceToEdit.name,
        category: 'Decoration',
        price: serviceToEdit.price.replace('From ', ''),
        description: serviceToEdit.desc,
        duration: serviceToEdit.duration || '',
        leadTime: serviceToEdit.delivery || '',
        images: serviceToEdit.images || []
      };
    } else {
      this.editingIndex = -1;
      this.newService = {
        name: '', category: 'Decoration', price: '', description: '', duration: '', leadTime: '', images: []
      };
    }
    this.isAddServiceModalOpen = true;
  }
  
  closeAddServiceModal() {
    this.isAddServiceModalOpen = false;
  }
  
  onImagesChanged(images: any[]) {
    // For UI demonstration, store the previewUrl (base64 data) to ensure it renders correctly on screen.
    // When connecting to .NET backend, you will extract img.file and send it via FormData instead.
    this.newService.images = images.map(img => img.previewUrl);
  }

  createService() {
    if (this.editingIndex > -1) {
      // Update existing
      this.services[this.editingIndex] = {
        ...this.services[this.editingIndex],
        name: this.newService.name,
        price: this.newService.price ? `From ${this.newService.price}` : 'Price on request',
        desc: this.newService.description,
        duration: this.newService.duration || '',
        delivery: this.newService.leadTime || '',
        images: this.newService.images.length > 0 ? this.newService.images : this.services[this.editingIndex].images
      };
    } else {
      // Create new
      this.services.unshift({
        icon: '✨',
        name: this.newService.name,
        price: this.newService.price ? `From ${this.newService.price}` : 'Price on request',
        desc: this.newService.description,
        duration: this.newService.duration || '',
        delivery: this.newService.leadTime || '',
        status: 'active',
        images: this.newService.images
      });
    }

    this.isAddServiceModalOpen = false;
  }

  closeDetailModal() {
    this.isDetailModalOpen = false;
    this.selectedService = null;
  }

  handleAction(action: string, service: VendorService) {
    if (action === 'delete') {
      if(confirm('Are you sure you want to delete this service?')) {
        this.services = this.services.filter(s => s !== service);
      }
    } else if (action === 'edit') {
      this.openAddServiceModal(service);
    } else if (action === 'pause') {
      service.status = 'paused';
      // Automatically switch to the paused tab so the user sees where it went
      this.activeTab = 'paused';
    } else if (action === 'activate') {
      service.status = 'active';
      // Automatically switch back to the active tab
      this.activeTab = 'active';
    } else if (action === 'detail') {
      this.selectedService = service;
      this.isDetailModalOpen = true;
    }
  }
}
