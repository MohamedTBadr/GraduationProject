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
import { VendorTypeService } from '../../../core/services/vendor-type.service';
import { VendorType, EventType } from '../../../core/models/taxonomy.models';
import { EventTypeService } from '../../../core/services/event-type.service';

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
  vendorTypes: VendorType[] = [];

  isDetailModalOpen = false;
  selectedService: ApiProduct | null = null;

  eventTypes: EventType[] = [];

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private authService: AuthService,
    private toastService: ToastService,
    private vendorTypeService: VendorTypeService,
    private eventTypeService: EventTypeService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadProducts();
    this.loadVendorTypes();
    this.loadEventTypes();
  }

  loadEventTypes(): void {
    this.eventTypeService.getAll().subscribe({
      next: (data) => this.eventTypes = data,
      error: (err) => {
        console.error('Failed to load event types', err);
      }
    });
  }

  loadVendorTypes(): void {
    this.vendorTypeService.getAll().subscribe({
      next: (data) => this.vendorTypes = data,
      error: (err) => {
        console.error('Failed to load vendor types', err);
        this.toastService.show('Failed to load vendor types', 'error');
      }
    });
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
      vendorTypeId: ['', Validators.required],
      serviceTypeId: ['', Validators.required],
      classification: ['Corporate'], // Default or optional
      eventTypeIds: [[]], // Array of strings
      price: [0, [Validators.required, Validators.min(0)]],
      description: ['', Validators.required],
      duration: [0],
      leadTime: [0]
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
      error: (err) => {
        console.error('Failed to load products', err);
        this.toastService.show('Failed to load services', 'error');
        this.loading = false;
      }
    });
  }

  openAddServiceModal(serviceToEdit?: ApiProduct) {
    if (serviceToEdit) {
      this.editingId = serviceToEdit.id;
      this.serviceForm.patchValue({
        name: serviceToEdit.name,
        vendorTypeId: serviceToEdit.vendorTypeId || '',
        serviceTypeId: serviceToEdit.serviceTypeId || '',
        classification: serviceToEdit.classification || '',
        eventTypeIds: serviceToEdit.eventTypeIds || serviceToEdit.allowedEventTypes || [],
        price: serviceToEdit.price || 0,
        description: serviceToEdit.description || '',
        duration: serviceToEdit.duration || 0,
        leadTime: serviceToEdit.leadTime || 0
      });
      this.uploadedImages = serviceToEdit.imageUrl ? [{ previewUrl: serviceToEdit.imageUrl, status: 'done' }] : [];
    } else {
      this.editingId = null;
      this.serviceForm.reset({ name: '', vendorTypeId: '', serviceTypeId: '', classification: 'Corporate', eventTypeIds: [], price: 0, description: '' });
      this.uploadedImages = [];
    }
    this.isAddServiceModalOpen = true;
  }
  
  closeAddServiceModal() {
    this.isAddServiceModalOpen = false;
    this.serviceForm.reset();
  }

  toggleEventType(evtId: string) {
    const currentList = this.serviceForm.get('eventTypeIds')?.value || [];
    const index = currentList.indexOf(evtId);
    if (index > -1) {
      currentList.splice(index, 1);
    } else {
      currentList.push(evtId);
    }
    this.serviceForm.get('eventTypeIds')?.setValue(currentList);
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

    if (this.editingId) {
      const formData = new FormData();
      formData.append('Id', this.editingId);
      formData.append('Name', val.name);
      formData.append('Description', val.description);
      formData.append('ServiceTypeId', val.serviceTypeId);
      if (val.eventTypeIds && val.eventTypeIds.length) {
        val.eventTypeIds.forEach((evtId: string) => {
          formData.append('EventTypeIds', evtId);
        });
      }
      formData.append('Price', (val.price || 0).toString());
      if (val.duration != null) formData.append('SetupDuration', val.duration.toString());
      if (val.leadTime != null) formData.append('LeadTimeRequired', val.leadTime.toString());
      
      if (this.uploadedImages && this.uploadedImages.length > 0) {
        this.uploadedImages.forEach(img => {
          if (img.file) {
            formData.append('Images', img.file, img.file.name);
          }
        });
      }
      
      this.productService.update(this.editingId, formData as any).subscribe({
        next: () => {
          this.toastService.show('Service updated successfully', 'success');
          this.loadProducts();
          this.closeAddServiceModal();
        },
        error: (err) => {
          console.error('Failed to update service', err);
          this.toastService.show('Failed to update service', 'error');
        }
      });
    } else {
      // Create expects FormData from the backend
      const formData = new FormData();
      formData.append('Name', val.name);
      formData.append('Description', val.description);
      formData.append('ServiceTypeId', val.serviceTypeId);
      if (val.eventTypeIds && val.eventTypeIds.length) {
        val.eventTypeIds.forEach((evtId: string) => {
          formData.append('EventTypeIds', evtId);
        });
      }
      formData.append('Price', (val.price || 0).toString());
      if (val.duration != null) formData.append('SetupDuration', val.duration.toString());
      if (val.leadTime != null) formData.append('LeadTimeRequired', val.leadTime.toString());
      
      // Append files
      if (this.uploadedImages && this.uploadedImages.length > 0) {
        this.uploadedImages.forEach(img => {
          if (img.file) {
            formData.append('ServiceImages', img.file, img.file.name);
          }
        });
      }

      this.productService.create(formData as any).subscribe({
        next: () => {
          this.toastService.show('Service created successfully', 'success');
          this.loadProducts();
          this.closeAddServiceModal();
        },
        error: (err) => {
           console.error("Error creating service:", err);
           this.toastService.show('Failed to create service', 'error');
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
          },
          error: (err) => {
            console.error('Failed to delete service', err);
            this.toastService.show('Failed to delete service', 'error');
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
      vendorTypeId: service.vendorTypeId,
      serviceTypeId: service.serviceTypeId,
      classification: service.classification,
      eventTypeIds: service.eventTypeIds || service.allowedEventTypes,
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
      },
      error: (err) => {
        console.error('Failed to update service status', err);
        this.toastService.show('Failed to update service status', 'error');
      }
    });
  }
}
