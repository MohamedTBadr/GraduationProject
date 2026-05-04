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

  // Local mock mapping since backend ServiceType doesn't store VendorTypeId yet
  serviceTypeToVendorTypeMap: Record<string, string> = {};

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
    // Load local storage mapping if exists
    const storedMap = localStorage.getItem('serviceTypeToVendorTypeMap');
    if (storedMap) {
      try {
        this.serviceTypeToVendorTypeMap = JSON.parse(storedMap);
      } catch (e) {}
    }

    this.loadVendorTypes();
    this.loadServiceTypes();
    this.loadEventTypes();
  }

  saveMap() {
    localStorage.setItem('serviceTypeToVendorTypeMap', JSON.stringify(this.serviceTypeToVendorTypeMap));
  }

  loadVendorTypes() {
    this.loadingVendorTypes = true;
    this.vendorTypeService.getAll().subscribe({
      next: (data) => {
        this.vendorTypes = data;
        this.loadingVendorTypes = false;
        this.autoMapServiceTypes();
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
        this.autoMapServiceTypes();
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

  // Auto map unmapped service types based on string matching or just assign randomly for UI purposes
  autoMapServiceTypes() {
    if (this.vendorTypes.length === 0 || this.serviceTypes.length === 0) return;
    
    let changed = false;
    this.serviceTypes.forEach(st => {
      if (!this.serviceTypeToVendorTypeMap[st.id]) {
        // Try to guess by name
        const stLower = st.name.toLowerCase();
        let matchedVtId = null;
        
        if (stLower.includes('photo') || stLower.includes('video') || stLower.includes('drone')) {
          matchedVtId = this.vendorTypes.find(v => v.name.toLowerCase().includes('photo') || v.name.toLowerCase().includes('video'))?.id;
        } else if (stLower.includes('cater') || stLower.includes('food') || stLower.includes('buffet') || stLower.includes('coffee')) {
          matchedVtId = this.vendorTypes.find(v => v.name.toLowerCase().includes('cater'))?.id;
        } else if (stLower.includes('venue') || stLower.includes('hall') || stLower.includes('room') || stLower.includes('garden')) {
          matchedVtId = this.vendorTypes.find(v => v.name.toLowerCase().includes('venue'))?.id;
        }
        
        // Default to first if not matched
        if (!matchedVtId && this.vendorTypes.length > 0) {
          matchedVtId = this.vendorTypes[0].id;
        }
        
        if (matchedVtId) {
          this.serviceTypeToVendorTypeMap[st.id] = matchedVtId;
          changed = true;
        }
      }
    });

    if (changed) {
      this.saveMap();
    }
  }

  getMappedServices(vendorTypeId: string): ServiceType[] {
    return this.serviceTypes.filter(st => this.serviceTypeToVendorTypeMap[st.id] === vendorTypeId);
  }

  getVendorTypeNameForService(serviceTypeId: string): string {
    const vtId = this.serviceTypeToVendorTypeMap[serviceTypeId];
    if (!vtId) return 'Unmapped';
    const vt = this.vendorTypes.find(v => v.id === vtId);
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
      vendorTypeId: this.serviceTypeToVendorTypeMap[st.id] || null
    });
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
      if (type === 'serviceType' && this.vendorTypes.length > 0) {
        this.form.patchValue({ vendorTypeId: this.vendorTypes[0].id });
      }
    }
  }

  onSubmit() {
    if (this.form.invalid) return;
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
        this.serviceTypeService.update(this.selectedId, { name: val.name }).subscribe({
          next: () => {
            if (val.vendorTypeId) {
              this.serviceTypeToVendorTypeMap[this.selectedId!] = val.vendorTypeId;
              this.saveMap();
            }
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
        this.serviceTypeService.create({ name: val.name }).subscribe({
          next: (newServiceType: any) => {
            // Update mapping locally
            if (val.vendorTypeId && newServiceType && newServiceType.id) {
               this.serviceTypeToVendorTypeMap[newServiceType.id] = val.vendorTypeId;
               this.saveMap();
            }
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
        // Remove from map
        delete this.serviceTypeToVendorTypeMap[id];
        this.saveMap();
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
    if (n.includes('venue') || n.includes('hall')) return '🏛️';
    if (n.includes('cater') || n.includes('food')) return '🍽️';
    if (n.includes('photo') || n.includes('camera') || n.includes('video')) return '📷';
    if (n.includes('decor')) return '🌸';
    if (n.includes('entertain') || n.includes('music') || n.includes('dj')) return '🎤';
    if (n.includes('light')) return '💡';
    if (n.includes('cake') || n.includes('dessert')) return '🎂';
    return '🗂️';
  }

  getIconForServiceType(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('wed') || n.includes('bride')) return '💍';
    if (n.includes('corp')) return '💼';
    if (n.includes('video') || n.includes('reel') || n.includes('film')) return '🎞️';
    if (n.includes('drone') || n.includes('aerial')) return '🚁';
    if (n.includes('buffet')) return '🥘';
    if (n.includes('plated') || n.includes('dining')) return '🍽️';
    if (n.includes('coffee')) return '☕';
    if (n.includes('ballroom') || n.includes('hall')) return '🏰';
    if (n.includes('garden') || n.includes('outdoor')) return '🌳';
    if (n.includes('conference')) return '🏢';
    return '✨';
  }

  getIconForEventType(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('wed')) return '💍';
    if (n.includes('engag')) return '💐';
    if (n.includes('birth')) return '🎂';
    if (n.includes('grad')) return '🎓';
    if (n.includes('corp') || n.includes('bus')) return '🏢';
    return '🎉';
  }
}
