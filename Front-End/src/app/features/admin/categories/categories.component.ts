import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { VendorTypeService } from '../../../core/services/vendor-type.service';
import { ServiceTypeService } from '../../../core/services/service-type.service';
import { EventTypeService } from '../../../core/services/event-type.service';
import { VendorType, ServiceType, EventType } from '../../../core/models/taxonomy.models';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './categories.component.html',
  styleUrls: ['./categories.component.scss']
})
export class CategoriesComponent implements OnInit {
  vendorTypes: VendorType[] = [];
  serviceTypes: ServiceType[] = [];
  eventTypes: EventType[] = [];

  loadingVendorTypes = false;
  loadingServiceTypes = false;
  loadingEventTypes = false;

  activeTab: 'vendorTypes' | 'serviceTypes' | 'eventTypes' | 'dependencyMap' = 'dependencyMap';

  showModal = false;
  isEditMode = false;
  activeModalType: 'vendorType' | 'serviceType' | 'eventType' = 'vendorType';
  showTypeSelector = true;
  selectedId: string | null = null;

  form: FormGroup;

  constructor(
    private vendorTypeService: VendorTypeService,
    private serviceTypeService: ServiceTypeService,
    private eventTypeService: EventTypeService,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      vendorTypeId: [null]
    });
  }

  ngOnInit(): void {
    this.loadVendorTypes();
    this.loadServiceTypes();
    this.loadEventTypes();
  }



  loadVendorTypes() {
    this.loadingVendorTypes = true;
    this.vendorTypeService.getAll().subscribe({
      next: (data) => {
        this.vendorTypes = data;
        this.loadingVendorTypes = false;
      },
      error: () => this.loadingVendorTypes = false
    });
  }

  loadServiceTypes() {
    this.loadingServiceTypes = true;
    this.serviceTypeService.getAll().subscribe({
      next: (data) => {
        this.serviceTypes = data;
        this.loadingServiceTypes = false;
      },
      error: () => this.loadingServiceTypes = false
    });
  }

  loadEventTypes() {
    this.loadingEventTypes = true;
    this.eventTypeService.getAll().subscribe({
      next: (data) => {
        this.eventTypes = data;
        this.loadingEventTypes = false;
      },
      error: () => this.loadingEventTypes = false
    });
  }

  getMappedServices(vendorTypeId: string): ServiceType[] {
    return this.serviceTypes.filter(st => st.vendorTypeId === vendorTypeId);
  }

  getVendorTypeNameForService(serviceTypeId: string): string {
    const st = this.serviceTypes.find(s => s.id === serviceTypeId);
    if (!st || !st.vendorTypeId) return 'Unmapped';
    const vt = this.vendorTypes.find(v => v.id === st.vendorTypeId);
    return vt ? vt.name : 'Unknown';
  }

  openAddModal(type?: 'vendorType' | 'serviceType' | 'eventType', prefillVendorTypeId?: string) {
    this.isEditMode = false;
    
    if (type) {
      this.activeModalType = type;
      this.showTypeSelector = false;
    } else {
      this.activeModalType = 'vendorType';
      this.showTypeSelector = true;
    }

    this.selectedId = null;
    this.form.reset();
    
    if (prefillVendorTypeId) {
      this.form.patchValue({ vendorTypeId: prefillVendorTypeId });
    } else if (this.vendorTypes.length > 0) {
      this.form.patchValue({ vendorTypeId: this.vendorTypes[0].id });
    }

    if (type === 'serviceType') {
      this.form.get('vendorTypeId')?.setValidators([Validators.required]);
    } else {
      this.form.get('vendorTypeId')?.clearValidators();
    }
    this.form.get('vendorTypeId')?.updateValueAndValidity();

    this.showModal = true;
  }

  openEditVendorType(vt: VendorType) {
    this.isEditMode = true;
    this.activeModalType = 'vendorType';
    this.showTypeSelector = false;
    this.selectedId = vt.id;
    this.form.patchValue({ name: vt.name });
    this.showModal = true;
  }

  openEditServiceType(st: ServiceType) {
    this.isEditMode = true;
    this.activeModalType = 'serviceType';
    this.showTypeSelector = false;
    this.selectedId = st.id;
    this.form.patchValue({ 
      name: st.name,
      vendorTypeId: st.vendorTypeId || ''
    });
    this.form.get('vendorTypeId')?.setValidators([Validators.required]);
    this.form.get('vendorTypeId')?.updateValueAndValidity();
    this.showModal = true;
  }

  openEditEventType(et: EventType) {
    this.isEditMode = true;
    this.activeModalType = 'eventType';
    this.showTypeSelector = false;
    this.selectedId = et.id;
    this.form.patchValue({ name: et.name });
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
  }

  setModalType(type: 'vendorType' | 'serviceType' | 'eventType') {
    if (!this.isEditMode) {
      this.activeModalType = type;
      if (type === 'serviceType') {
        this.form.get('vendorTypeId')?.setValidators([Validators.required]);
        if (this.vendorTypes.length > 0) {
          this.form.patchValue({ vendorTypeId: this.vendorTypes[0].id });
        }
      } else {
        this.form.get('vendorTypeId')?.clearValidators();
      }
      this.form.get('vendorTypeId')?.updateValueAndValidity();
    }
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const val = this.form.value;

    if (this.isEditMode && this.selectedId) {
      if (this.activeModalType === 'vendorType') {
        this.vendorTypeService.update(this.selectedId, { name: val.name }).subscribe({
          next: () => {
            this.loadVendorTypes();
            this.closeModal();
          }
        });
      } else if (this.activeModalType === 'serviceType') {
        if (!val.vendorTypeId) return; // Guard
        this.serviceTypeService.update(this.selectedId, { name: val.name, vendorTypeId: val.vendorTypeId }).subscribe({
          next: () => {
            this.loadServiceTypes();
            this.closeModal();
          }
        });
      } else {
        this.eventTypeService.update(this.selectedId, { id: this.selectedId, name: val.name }).subscribe({
          next: () => {
            this.loadEventTypes();
            this.closeModal();
          }
        });
      }
    } else {
      if (this.activeModalType === 'vendorType') {
        this.vendorTypeService.create({ name: val.name }).subscribe({
          next: () => {
            this.loadVendorTypes();
            this.closeModal();
          }
        });
      } else if (this.activeModalType === 'serviceType') {
        if (!val.vendorTypeId) return; // Guard
        this.serviceTypeService.create({ name: val.name, vendorTypeId: val.vendorTypeId }).subscribe({
          next: () => {
            this.loadServiceTypes();
            this.closeModal();
          }
        });
      } else {
        this.eventTypeService.create({ name: val.name }).subscribe({
          next: () => {
            this.loadEventTypes();
            this.closeModal();
          }
        });
      }
    }
  }

  deleteVendorType(id: string) {
    this.vendorTypeService.delete(id).subscribe({
      next: () => {
        this.loadVendorTypes();
      },
      error: (err) => {
        alert('Failed to delete: This type may be in use by existing vendors.');
      }
    });
  }

  deleteServiceType(id: string) {
    this.serviceTypeService.delete(id).subscribe({
      next: () => {
        this.loadServiceTypes();
      },
      error: (err) => {
        alert('Failed to delete: This type may be in use by existing services.');
      }
    });
  }

  deleteEventType(id: string) {
    this.eventTypeService.delete(id).subscribe({
      next: () => {
        this.loadEventTypes();
      },
      error: (err) => {
        alert('Failed to delete: This type may be in use by existing events.');
      }
    });
  }

  getIconForVendorType(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('venue') || n.includes('hall')) return 'bi bi-building';
    if (n.includes('cater') || n.includes('food')) return 'bi bi-cup-hot';
    if (n.includes('photo') || n.includes('camera') || n.includes('video')) return 'bi bi-camera';
    if (n.includes('decor')) return 'bi bi-flower1';
    if (n.includes('entertain') || n.includes('music') || n.includes('dj')) return 'bi bi-music-note-beamed';
    if (n.includes('light')) return 'bi bi-lightbulb';
    if (n.includes('cake') || n.includes('dessert')) return 'bi bi-gift';
    return 'bi bi-grid-3x3-gap';
  }

  getIconForServiceType(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('wed') || n.includes('bride')) return 'bi bi-heart';
    if (n.includes('corp')) return 'bi bi-briefcase';
    if (n.includes('video') || n.includes('reel') || n.includes('film')) return 'bi bi-film';
    if (n.includes('drone') || n.includes('aerial')) return 'bi bi-camera-video';
    if (n.includes('buffet')) return 'bi bi-basket';
    if (n.includes('plated') || n.includes('dining')) return 'bi bi-cup-hot';
    if (n.includes('coffee')) return 'bi bi-cup';
    if (n.includes('ballroom') || n.includes('hall')) return 'bi bi-building-fill';
    if (n.includes('garden') || n.includes('outdoor')) return 'bi bi-tree';
    if (n.includes('conference')) return 'bi bi-buildings';
    return 'bi bi-stars';
  }

  getIconForEventType(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('wed')) return 'bi bi-heart';
    if (n.includes('engag')) return 'bi bi-heart-fill';
    if (n.includes('birth')) return 'bi bi-gift';
    if (n.includes('grad')) return 'bi bi-mortarboard';
    if (n.includes('corp') || n.includes('bus')) return 'bi bi-briefcase';
    return 'bi bi-calendar-event';
  }
}
