import { Component, OnInit, Optional, Self, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ControlValueAccessor, NgControl } from '@angular/forms';
import { ServiceTypeService } from '../../../core/services/service-type.service';
import { ServiceType } from '../../types/api.interfaces';

@Component({
  selector: 'app-service-type-dropdown',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './service-type-dropdown.component.html',
  styleUrls: ['./service-type-dropdown.component.scss']
})
export class ServiceTypeDropdownComponent implements ControlValueAccessor, OnInit {
  @Input() placeholder: string = 'Select a service type';
  @Input() vendorTypeId?: string | null = null;
  
  allServiceTypes: ServiceType[] = [];
  serviceTypes: ServiceType[] = [];
  loading: boolean = false;
  error: string | null = null;
  
  value: string = '';
  isDisabled: boolean = false;

  onChange = (value: any) => {};
  onTouched = () => {};

  constructor(
    private serviceTypesService: ServiceTypeService,
    @Optional() @Self() public ngControl: NgControl
  ) {
    if (this.ngControl != null) {
      this.ngControl.valueAccessor = this;
    }
  }

  ngOnInit(): void {
    this.fetchServiceTypes();
  }

  fetchServiceTypes(): void {
    this.loading = true;
    this.error = null;
    
    this.serviceTypesService.getAll().subscribe({
      next: (data) => {
        this.allServiceTypes = data;
        this.applyFilter();
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load service types.';
        this.loading = false;
        console.error('Error fetching service types:', err);
      }
    });
  }

  ngOnChanges(changes: any): void {
    if (changes['vendorTypeId']) {
      this.applyFilter();
    }
  }

  applyFilter() {
    if (!this.vendorTypeId) {
      this.serviceTypes = this.allServiceTypes;
      return;
    }

    this.serviceTypes = this.allServiceTypes.filter((st: any) => st.vendorTypeId === this.vendorTypeId);
    
    // Fallback to all if mapping is too strict and returns empty (unless explicitly empty)
    if (this.serviceTypes.length === 0) {
       this.serviceTypes = this.allServiceTypes;
    }
  }

  writeValue(obj: any): void {
    this.value = obj || '';
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState?(isDisabled: boolean): void {
    this.isDisabled = isDisabled;
  }

  onSelectChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.value = target.value;
    this.onChange(this.value);
    this.onTouched();
  }
}
