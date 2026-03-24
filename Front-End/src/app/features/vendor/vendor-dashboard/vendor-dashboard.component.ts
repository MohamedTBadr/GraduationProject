import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ImageUploadComponent } from '../../../shared/components/image-upload/image-upload.component';

@Component({
  selector: 'app-vendor-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ImageUploadComponent],
  templateUrl: './vendor-dashboard.component.html',
  styleUrls: ['./vendor-dashboard.component.scss']
})
export class VendorDashboardComponent {
  isAddServiceModalOpen = false;
  
  newService = {
    name: '',
    category: 'Decoration',
    price: '',
    description: '',
    duration: '',
    leadTime: '',
    images: [] as any[]
  };

  openAddServiceModal() {
    this.isAddServiceModalOpen = true;
  }
  
  closeAddServiceModal() {
    this.isAddServiceModalOpen = false;
  }
  
  onImagesChanged(images: any[]) {
    // Collect the files for future FormData or backend integration
    this.newService.images = images.map(img => img.file || img.previewUrl);
  }

  createService() {
    // Format for future .NET backend:
    // const formData = new FormData();
    // formData.append('name', this.newService.name);
    // this.newService.images.forEach((file) => {
    //    if(file instanceof File) formData.append('images', file, file.name);
    // });
    // this.api.post(formData).subscribe(...)
    
    console.log('Service created:', this.newService);
    this.isAddServiceModalOpen = false;
    this.newService = {
      name: '', category: 'Decoration', price: '', description: '', duration: '', leadTime: '', images: []
    };
  }
}
