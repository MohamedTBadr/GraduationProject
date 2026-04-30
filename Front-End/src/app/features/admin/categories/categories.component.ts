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

  showModal = false;
  isEditMode = false;
  activeType: 'vendorType' | 'serviceType' | 'eventType' = 'vendorType';
  selectedId: string | null = null;

  form: FormGroup;

  constructor(
    private vendorTypeService: VendorTypeService,
    private serviceTypeService: ServiceTypeService,
    private eventTypeService: EventTypeService,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required]
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

  openAddModal() {
    this.isEditMode = false;
    this.activeType = 'vendorType';
    this.selectedId = null;
    this.form.reset();
    this.showModal = true;
  }

  openEditVendorType(vt: VendorType) {
    this.isEditMode = true;
    this.activeType = 'vendorType';
    this.selectedId = vt.id;
    this.form.patchValue({ name: vt.name });
    this.showModal = true;
  }

  openEditServiceType(st: ServiceType) {
    this.isEditMode = true;
    this.activeType = 'serviceType';
    this.selectedId = st.id;
    this.form.patchValue({ name: st.name });
    this.showModal = true;
  }

  openEditEventType(et: EventType) {
    this.isEditMode = true;
    this.activeType = 'eventType';
    this.selectedId = et.id;
    this.form.patchValue({ name: et.name });
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
  }

  setType(type: 'vendorType' | 'serviceType' | 'eventType') {
    if (!this.isEditMode) {
      this.activeType = type;
    }
  }

  onSubmit() {
    if (this.form.invalid) return;
    const val = this.form.value;

    if (this.isEditMode && this.selectedId) {
      if (this.activeType === 'vendorType') {
        this.vendorTypeService.update(this.selectedId, val).subscribe({
          next: () => {
            this.loadVendorTypes();
            this.closeModal();
          }
        });
      } else if (this.activeType === 'serviceType') {
        this.serviceTypeService.update(this.selectedId, val).subscribe({
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
      if (this.activeType === 'vendorType') {
        this.vendorTypeService.create(val).subscribe({
          next: () => {
            this.loadVendorTypes();
            this.closeModal();
          }
        });
      } else if (this.activeType === 'serviceType') {
        this.serviceTypeService.create(val).subscribe({
          next: () => {
            this.loadServiceTypes();
            this.closeModal();
          }
        });
      } else {
        this.eventTypeService.create(val).subscribe({
          next: () => {
            this.loadEventTypes();
            this.closeModal();
          }
        });
      }
    }
  }

  deleteVendorType(id: string) {
    console.log('deleteVendorType called with ID:', id);
    this.vendorTypeService.delete(id).subscribe({
      next: () => {
        console.log('deleteVendorType success');
        this.loadVendorTypes();
      },
      error: (err) => {
        console.error('deleteVendorType error:', err);
        alert('Failed to delete: This type may be in use by existing vendors.');
      }
    });
  }

  deleteServiceType(id: string) {
    console.log('deleteServiceType called with ID:', id);
    this.serviceTypeService.delete(id).subscribe({
      next: () => {
        console.log('deleteServiceType success');
        this.loadServiceTypes();
      },
      error: (err) => {
        console.error('deleteServiceType error:', err);
        alert('Failed to delete: This type may be in use by existing services.');
      }
    });
  }

  deleteEventType(id: string) {
    console.log('deleteEventType called with ID:', id);
    this.eventTypeService.delete(id).subscribe({
      next: () => {
        console.log('deleteEventType success');
        this.loadEventTypes();
      },
      error: (err) => {
        console.error('deleteEventType error:', err);
        alert('Failed to delete: This type may be in use by existing events.');
      }
    });
  }





  getIconForVendorType(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('venue')) return '🏛️';
    if (n.includes('cater') || n.includes('food')) return '🍽️';
    if (n.includes('photo') || n.includes('camera')) return '📷';
    if (n.includes('decor')) return '🌸';
    if (n.includes('entertain') || n.includes('music') || n.includes('dj')) return '🎤';
    if (n.includes('light')) return '💡';
    if (n.includes('cake') || n.includes('dessert')) return '🎂';
    return '✨';
  }

  getIconForEventType(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('wed')) return '💍';
    if (n.includes('engag')) return '💐';
    if (n.includes('birth')) return '🎂';
    if (n.includes('grad')) return '🎓';
    if (n.includes('corp') || n.includes('bus')) return '🏢';
    return '✨';
  }
}
