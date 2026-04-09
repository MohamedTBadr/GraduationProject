import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryService } from '../../../core/services/category.service';
import { ServiceTypeService } from '../../../core/services/service-type.service';
import { Category, ServiceType } from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './categories.component.html',
  styleUrls: ['./categories.component.scss']
})
export class CategoriesComponent implements OnInit {
  categories: Category[] = [];
  eventTypes: ServiceType[] = [];

  loadingCategories = false;
  loadingEventTypes = false;

  showModal = false;
  isEditMode = false;
  activeType: 'category' | 'eventType' = 'category';
  selectedId: string | null = null;

  form: FormGroup;

  constructor(
    private categoryService: CategoryService,
    private serviceTypeService: ServiceTypeService,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      // description: ['']
    });
  }

  ngOnInit(): void {
    this.loadCategories();
    this.loadEventTypes();
  }

  loadCategories() {
    this.loadingCategories = true;
    this.categoryService.getAll().subscribe({
      next: (data) => {
        this.categories = data;
        this.loadingCategories = false;
      },
      error: () => this.loadingCategories = false
    });
  }

  loadEventTypes() {
    this.loadingEventTypes = true;
    this.serviceTypeService.getAll().subscribe({
      next: (data) => {
        this.eventTypes = data;
        this.loadingEventTypes = false;
      },
      error: () => this.loadingEventTypes = false
    });
  }

  openAddModal() {
    this.isEditMode = false;
    this.activeType = 'category';
    this.selectedId = null;
    this.form.reset();
    this.showModal = true;
  }

  openEditCategory(cat: Category) {
    this.isEditMode = true;
    this.activeType = 'category';
    this.selectedId = cat.id;
    this.form.patchValue({ name: cat.name });
    this.showModal = true;
  }

  openEditEventType(ev: ServiceType) {
    this.isEditMode = true;
    this.activeType = 'eventType';
    this.selectedId = ev.id;
    this.form.patchValue({ name: ev.name });
    this.showModal = true;
  }

  closeModal() {
    this.showModal = false;
  }

  setType(type: 'category' | 'eventType') {
    if (!this.isEditMode) {
      this.activeType = type;
    }
  }

  onSubmit() {
    if (this.form.invalid) return;
    const val = this.form.value;

    if (this.isEditMode && this.selectedId) {
      if (this.activeType === 'category') {
        this.categoryService.update(this.selectedId, val).subscribe({
          next: () => {
            this.loadCategories();
            this.closeModal();
          }
        });
      } else {
        this.serviceTypeService.update(this.selectedId, val).subscribe({
          next: () => {
            this.loadEventTypes();
            this.closeModal();
          }
        });
      }
    } else {
      if (this.activeType === 'category') {
        this.categoryService.create(val).subscribe({
          next: () => {
            this.loadCategories();
            this.closeModal();
          }
        });
      } else {
        this.serviceTypeService.create(val).subscribe({
          next: () => {
            this.loadEventTypes();
            this.closeModal();
          }
        });
      }
    }
  }

  deleteCategory(id: string) {
    if (confirm('Are you sure you want to delete this category?')) {
      this.categoryService.delete(id).subscribe({
        next: () => this.loadCategories()
      });
    }
  }

  deleteEventType(id: string) {
    if (confirm('Are you sure you want to delete this event type?')) {
      this.serviceTypeService.delete(id).subscribe({
        next: () => this.loadEventTypes()
      });
    }
  }

  getIconForCategory(name: string): string {
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
