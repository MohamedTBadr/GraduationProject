import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ServiceCardComponent } from '../../../shared/components/service-card/service-card.component';
import { ApiProduct, CreateProductRequest, UpdateProductRequest } from '../../../shared/types/api.interfaces';
import { ImageUploadComponent } from '../../../shared/components/image-upload/image-upload.component';
import { ServiceTypeDropdownComponent } from '../../../shared/components/service-type-dropdown/service-type-dropdown.component';
import { ProductService } from '../../../core/services/product.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-services',
  standalone: true,
  imports: [
    CommonModule, 
    ServiceCardComponent, 
    FormsModule, 
    ReactiveFormsModule, 
    ImageUploadComponent,
    ServiceTypeDropdownComponent
  ],
  templateUrl: './services.component.html',
  styleUrls: ['./services.component.scss']
})
export class ServicesComponent implements OnInit {
  services: ApiProduct[] = [];
  loading = false;
  activeTab: 'active' | 'paused' = 'active';

  isAddServiceModalOpen = false;
  editingId: string | null = null;
  
  serviceForm!: FormGroup;
  uploadedImages: any[] = [];

  isDetailModalOpen = false;
  selectedService: ApiProduct | null = null;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private authService: AuthService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadProducts();
  }

  get activeServices(): ApiProduct[] {
    return this.services.filter(s => s.status !== 'paused');
  }

  get pausedServices(): ApiProduct[] {
    return this.services.filter(s => s.status === 'paused');
  }

  get currentServices(): ApiProduct[] {
    return this.activeTab === 'active' ? this.activeServices : this.pausedServices;
  }

  setTab(tab: 'active' | 'paused') {
    this.activeTab = tab;
  }

  initForm(): void {
    this.serviceForm = this.fb.group({
      name: ['', Validators.required],
      serviceTypeId: ['', Validators.required],
      price: [0, [Validators.required, Validators.min(0)]],
      description: ['', Validators.required],
      duration: [''],
      leadTime: ['']
    });
  }

  loadProducts(): void {
    const user = this.authService.user();
    if (!user) return;
    
    this.loading = true;
    this.productService.getByVendor(user.id).subscribe({
      next: (data) => {
        // Enforce default status as active locally for backwards-compatibility or missing data.
        this.services = data.map(d => ({ ...d, status: d.status || 'active' }));
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  openAddServiceModal(serviceToEdit?: ApiProduct) {
    if (serviceToEdit) {
      this.editingId = serviceToEdit.id;
      this.serviceForm.patchValue({
        name: serviceToEdit.name,
        serviceTypeId: serviceToEdit.serviceTypeId || '',
        price: serviceToEdit.price || 0,
        description: serviceToEdit.description || '',
        duration: serviceToEdit.duration || '',
        leadTime: serviceToEdit.leadTime || ''
      });
      this.uploadedImages = serviceToEdit.imageUrl ? [{ previewUrl: serviceToEdit.imageUrl }] : [];
    } else {
      this.editingId = null;
      this.serviceForm.reset({ name: '', serviceTypeId: '', price: 0, description: '' });
      this.uploadedImages = [];
    }
    this.isAddServiceModalOpen = true;
  }
  
  closeAddServiceModal() {
    this.isAddServiceModalOpen = false;
    this.serviceForm.reset();
  }
  
  onImagesChanged(images: any[]) {
    this.uploadedImages = images;
  }

  createService() {
    if (this.serviceForm.invalid) {
      this.serviceForm.markAllAsTouched();
      return;
    }

    const val = this.serviceForm.value;
    const imageUrl = this.uploadedImages.length > 0 ? this.uploadedImages[0].previewUrl : null;

    if (this.editingId) {
      const existing = this.services.find(s => s.id === this.editingId);
      const updateData: UpdateProductRequest = {
        name: val.name,
        serviceTypeId: val.serviceTypeId,
        price: Number(val.price),
        description: val.description,
        imageUrl: imageUrl,
        status: existing?.status || 'active',
        duration: val.duration,
        leadTime: val.leadTime
      };
      
      this.productService.update(this.editingId, updateData).subscribe({
        next: () => {
          this.toastService.show('Service updated successfully', 'success');
          this.loadProducts();
          this.closeAddServiceModal();
        }
      });
    } else {
      const createData: CreateProductRequest = {
        name: val.name,
        serviceTypeId: val.serviceTypeId,
        price: Number(val.price),
        description: val.description,
        imageUrl: imageUrl,
        status: 'active',
        duration: val.duration,
        leadTime: val.leadTime
      };

      this.productService.create(createData).subscribe({
        next: () => {
          this.toastService.show('Service created successfully', 'success');
          this.loadProducts();
          this.closeAddServiceModal();
        }
      });
    }
  }

  closeDetailModal() {
    this.isDetailModalOpen = false;
    this.selectedService = null;
  }

  handleAction(action: string, service: ApiProduct) {
    if (action === 'delete') {
      if(confirm('Are you sure you want to delete this service?')) {
        this.productService.delete(service.id).subscribe({
          next: () => {
            this.toastService.show('Service deleted', 'success');
            this.loadProducts();
          }
        });
      }
    } else if (action === 'edit') {
      this.openAddServiceModal(service);
    } else if (action === 'detail') {
      this.selectedService = service;
      this.isDetailModalOpen = true;
    } else if (action === 'pause') {
      this.updateServiceStatus(service, 'paused');
    } else if (action === 'activate') {
      this.updateServiceStatus(service, 'active');
    }
  }

  updateServiceStatus(service: ApiProduct, newStatus: 'active' | 'paused') {
    const updateData: UpdateProductRequest = {
      name: service.name,
      serviceTypeId: service.serviceTypeId,
      price: service.price,
      description: service.description,
      imageUrl: service.imageUrl,
      status: newStatus
    };
    
    this.productService.update(service.id, updateData).subscribe({
      next: () => {
        service.status = newStatus;
        if (newStatus === 'paused') {
          this.toastService.show('Service paused. It will no longer be visible to clients.', 'info');
        } else {
          this.toastService.show('Service activated successfully!', 'success');
        }
      }
    });
  }
}
