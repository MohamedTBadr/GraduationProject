import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { catchError, of } from 'rxjs';
import { ServiceCardComponent } from '../../../shared/components/service-card/service-card.component';
import { ApiProduct } from '../../../shared/types/api.interfaces';
import { ImageUploadComponent } from '../../../shared/components/image-upload/image-upload.component';
import { ServiceTypeDropdownComponent } from '../../../shared/components/service-type-dropdown/service-type-dropdown.component';
import { ProductService } from '../../../core/services/product.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { EventType } from '../../../core/models/taxonomy.models';
import { EventTypeService } from '../../../core/services/event-type.service';
import { PackageService, ApiPackage } from '../../../core/services/package.service';
import { appendFormFileList } from '../../../shared/utils/form-data.utils';
import { getProductCoverImage, getProductImageUrls, urlToFile } from '../../../shared/utils/image.utils';
import { UploadedImage } from '../../../shared/components/image-upload/image-upload.component';

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

  // ── Catalog top-level tab ──────────────────────────────────
  catalogTab: 'services' | 'packages' = 'services';

  setCatalogTab(tab: 'services' | 'packages') { this.catalogTab = tab; }

  // ── Services ──────────────────────────────────────────────
  services: ApiProduct[] = [];
  loading = false;
  activeTab: 'active' | 'paused' = 'active';
  includeHidden = true;

  isAddServiceModalOpen = false;
  editingId: string | null = null;

  serviceForm!: FormGroup;
  uploadedImages: UploadedImage[] = [];
  serviceImageUrls: string[] = [];

  isDetailModalOpen = false;
  selectedService: ApiProduct | null = null;

  eventTypes: EventType[] = [];
  isSubmitting = false;

  // ── Packages ──────────────────────────────────────────────
  packages: ApiPackage[] = [];
  packagesLoading = false;
  isPackageModalOpen = false;
  editingPackageId: string | null = null;
  packageForm!: FormGroup;
  selectedServiceIds: string[] = [];
  isPackageSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private authService: AuthService,
    private toastService: ToastService,
    private eventTypeService: EventTypeService,
    private packageService: PackageService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.initPackageForm();
    this.loadProducts();
    this.loadEventTypes();
    this.loadPackages();
  }

  // ── Services ──────────────────────────────────────────────

  loadEventTypes(): void {
    this.eventTypeService.getAll().subscribe({
      next: (data) => this.eventTypes = data,
      error: (err) => console.error('Failed to load event types', err)
    });
  }

  get activeServices(): ApiProduct[] { return this.services.filter(s => !s.isHidden && s.status !== 'paused'); }
  get pausedServices(): ApiProduct[] { return this.services.filter(s => s.isHidden || s.status === 'paused'); }
  get currentServices(): ApiProduct[] { return this.activeTab === 'active' ? this.activeServices : this.pausedServices; }

  setTab(tab: 'active' | 'paused'): void {
    this.activeTab = tab;
    if (tab === 'paused' && !this.includeHidden) {
      this.includeHidden = true;
      this.loadProducts();
    }
  }

  onIncludeHiddenChange(checked: boolean): void {
    this.includeHidden = checked;
    this.loadProducts();
  }

  initForm(): void {
    this.serviceForm = this.fb.group({
      name: ['', Validators.required],
      serviceTypeId: ['', Validators.required],
      eventTypeIds: [[]],
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
    this.productService.getByVendor(user.id, { includeHidden: this.includeHidden }).pipe(
      catchError((err) => {
        if (err?.status === 404) return of([]);
        throw err;
      })
    ).subscribe({
      next: (data) => {
        this.services = data;
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
        serviceTypeId: serviceToEdit.serviceTypeId || '',
        eventTypeIds: serviceToEdit.eventTypeIds || serviceToEdit.allowedEventTypes || [],
        price: serviceToEdit.price || 0,
        description: serviceToEdit.description || '',
        duration: serviceToEdit.duration || 0,
        leadTime: serviceToEdit.leadTime || 0
      });
      this.serviceImageUrls = getProductImageUrls(serviceToEdit);
    } else {
      this.editingId = null;
      this.serviceForm.reset({ name: '', serviceTypeId: '', eventTypeIds: [], price: 0, description: '' });
      this.serviceImageUrls = [];
    }
    this.isAddServiceModalOpen = true;
  }

  closeAddServiceModal() {
    this.isAddServiceModalOpen = false;
    this.isSubmitting = false;
    this.uploadedImages = [];
    this.serviceImageUrls = [];
    this.serviceForm.reset({ name: '', serviceTypeId: '', eventTypeIds: [], price: 0, description: '', duration: 0, leadTime: 0 });
  }

  toggleEventType(evtId: string) {
    const currentList = this.serviceForm.get('eventTypeIds')?.value || [];
    const index = currentList.indexOf(evtId);
    if (index > -1) currentList.splice(index, 1);
    else currentList.push(evtId);
    this.serviceForm.get('eventTypeIds')?.setValue(currentList);
  }

  onImagesChanged(images: UploadedImage[]) { this.uploadedImages = images; }

  getServiceCover(service: ApiProduct): string | null {
    return getProductCoverImage(service);
  }

  getServiceImages(service: ApiProduct): string[] {
    return getProductImageUrls(service);
  }

  private collectNewImageFiles(): File[] {
    return this.uploadedImages
      .filter(img => !!img.file)
      .map(img => img.file!);
  }

  /** On update the API replaces all images — re-include kept server URLs as files. */
  private async collectAllImageFilesForUpdate(): Promise<File[]> {
    const files: File[] = [];
    for (const img of this.uploadedImages) {
      if (img.file) {
        files.push(img.file);
        continue;
      }
      const url = typeof img.previewUrl === 'string' ? img.previewUrl : null;
      if (url && !url.startsWith('data:')) {
        const existing = await urlToFile(url);
        if (existing) files.push(existing);
      }
    }
    return files;
  }

  createService() {
    if (this.serviceForm.invalid) { this.serviceForm.markAllAsTouched(); return; }
    if (this.isSubmitting) return;
    void this.submitService();
  }

  private async submitService(): Promise<void> {
    this.isSubmitting = true;
    const val = this.serviceForm.value;

    try {
      const imageFiles = this.editingId
        ? await this.collectAllImageFilesForUpdate()
        : this.collectNewImageFiles();

      if (this.editingId && imageFiles.length < this.uploadedImages.length) {
        this.toastService.show(
          'Some existing images could not be kept. Remove them or re-select all images you want to save.',
          'error'
        );
        this.isSubmitting = false;
        return;
      }

      if (this.editingId) {
        const formData = new FormData();
        formData.append('Id', this.editingId);
        formData.append('Name', val.name);
        formData.append('Description', val.description);
        formData.append('ServiceTypeId', val.serviceTypeId);
        if (val.eventTypeIds?.length) val.eventTypeIds.forEach((id: string) => formData.append('EventTypeIds', id));
        formData.append('Price', (val.price || 0).toString());
        if (val.duration != null) formData.append('SetupDuration', val.duration.toString());
        if (val.leadTime != null) formData.append('LeadTimeRequired', val.leadTime.toString());
        if (imageFiles.length) appendFormFileList(formData, 'Images', imageFiles);

        this.productService.update(this.editingId, formData as any).subscribe({
          next: () => { this.isSubmitting = false; this.toastService.show('Service updated successfully', 'success'); this.loadProducts(); this.closeAddServiceModal(); },
          error: (err) => { this.isSubmitting = false; console.error('Failed to update service', err); this.toastService.show('Failed to update service', 'error'); }
        });
      } else {
        const formData = new FormData();
        formData.append('Name', val.name);
        formData.append('Description', val.description);
        formData.append('ServiceTypeId', val.serviceTypeId);
        if (val.eventTypeIds?.length) val.eventTypeIds.forEach((id: string) => formData.append('EventTypeIds', id));
        formData.append('Price', (val.price || 0).toString());
        if (val.duration != null) formData.append('SetupDuration', val.duration.toString());
        if (val.leadTime != null) formData.append('LeadTimeRequired', val.leadTime.toString());
        if (imageFiles.length) appendFormFileList(formData, 'ServiceImages', imageFiles);

        this.productService.create(formData as any).subscribe({
          next: () => { this.isSubmitting = false; this.toastService.show('Service created successfully', 'success'); this.loadProducts(); this.closeAddServiceModal(); },
          error: (err) => { this.isSubmitting = false; console.error('Error creating service:', err); this.toastService.show('Failed to create service', 'error'); }
        });
      }
    } catch (err) {
      this.isSubmitting = false;
      console.error('Failed to prepare service images', err);
      this.toastService.show('Failed to prepare images for upload', 'error');
    }
  }

  closeDetailModal() { this.isDetailModalOpen = false; this.selectedService = null; }

  handleAction(action: string, service: ApiProduct) {
    if (action === 'delete') {
      if (confirm('Are you sure you want to delete this service?')) {
        this.productService.delete(service.id).subscribe({
          next: () => { this.toastService.show('Service deleted', 'success'); this.loadProducts(); },
          error: (err) => { console.error('Failed to delete service', err); this.toastService.show('Failed to delete service', 'error'); }
        });
      }
    } else if (action === 'edit') {
      this.openAddServiceModal(service);
    } else if (action === 'detail') {
      this.selectedService = service;
      this.isDetailModalOpen = true;
    } else if (action === 'pause' || action === 'activate') {
      this.toggleServiceStatus(service);
    }
  }

  toggleServiceStatus(service: ApiProduct) {
    const willPause = !service.isHidden && service.status !== 'paused';
    this.productService.toggleStatus(service.id).subscribe({
      next: () => {
        if (willPause) this.includeHidden = true;
        this.toastService.show(willPause ? 'Service paused.' : 'Service activated!', 'info');
        this.loadProducts();
      },
      error: (err) => { console.error('Failed to update service status', err); this.toastService.show('Failed to update service status', 'error'); }
    });
  }

  // ── Packages ──────────────────────────────────────────────

  initPackageForm(): void {
    this.packageForm = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      price: [0, [Validators.required, Validators.min(0)]],
      discount: [0, Validators.min(0)]
    });
  }

  loadPackages(): void {
    const user = this.authService.user();
    if (!user) return;
    this.packagesLoading = true;
    this.packageService.getByVendor(user.id).subscribe({
      next: (data) => { this.packages = data; this.packagesLoading = false; },
      error: (err) => { console.error('Failed to load packages', err); this.toastService.show('Failed to load packages', 'error'); this.packagesLoading = false; }
    });
  }

  openPackageModal(pkg?: ApiPackage): void {
    if (pkg) {
      this.editingPackageId = pkg.id;
      this.packageForm.patchValue({
        name: pkg.name,
        description: pkg.description || '',
        price: pkg.price || 0,
        discount: pkg.discount || 0
      });
      this.selectedServiceIds = (pkg.services || []).map(s => s.id);
    } else {
      this.editingPackageId = null;
      this.packageForm.reset({ name: '', description: '', price: 0, discount: 0 });
      this.selectedServiceIds = [];
    }
    this.isPackageModalOpen = true;
  }

  closePackageModal(): void {
    this.isPackageModalOpen = false;
    this.isPackageSubmitting = false;
    this.packageForm.reset();
  }

  toggleServiceForPackage(serviceId: string): void {
    const index = this.selectedServiceIds.indexOf(serviceId);
    if (index > -1) this.selectedServiceIds.splice(index, 1);
    else this.selectedServiceIds.push(serviceId);
  }

  isServiceSelectedForPackage(serviceId: string): boolean {
    return this.selectedServiceIds.includes(serviceId);
  }

  savePackage(): void {
    if (this.packageForm.invalid) { this.packageForm.markAllAsTouched(); return; }
    if (this.isPackageSubmitting) return;
    this.isPackageSubmitting = true;

    const val = this.packageForm.value;
    const user = this.authService.user();

    if (this.editingPackageId) {
      this.packageService.update(this.editingPackageId, {
        name: val.name,
        description: val.description,
        price: val.price,
        discount: val.discount ?? 0,
        serviceIds: this.selectedServiceIds,
        vendorId: user?.id ?? ''
      }).subscribe({
        next: () => { this.isPackageSubmitting = false; this.toastService.show('Package updated successfully', 'success'); this.loadPackages(); this.closePackageModal(); },
        error: (err) => { this.isPackageSubmitting = false; console.error('Failed to update package', err); this.toastService.show('Failed to update package', 'error'); }
      });
    } else {
      this.packageService.create({
        name: val.name,
        description: val.description,
        price: val.price,
        discount: val.discount ?? 0,
        serviceIds: this.selectedServiceIds
      }).subscribe({
        next: () => { this.isPackageSubmitting = false; this.toastService.show('Package created successfully', 'success'); this.loadPackages(); this.closePackageModal(); },
        error: (err) => { this.isPackageSubmitting = false; console.error('Failed to create package', err); this.toastService.show('Failed to create package', 'error'); }
      });
    }
  }

  deletePackage(id: string): void {
    if (!confirm('Are you sure you want to delete this package?')) return;
    this.packageService.delete(id).subscribe({
      next: () => { this.toastService.show('Package deleted', 'success'); this.loadPackages(); },
      error: (err) => { console.error('Failed to delete package', err); this.toastService.show('Failed to delete package', 'error'); }
    });
  }
}
