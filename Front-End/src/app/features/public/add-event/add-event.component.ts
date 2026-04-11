import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { EventService } from '../../../core/services/event.service';
import { CategoryService } from '../../../core/services/category.service';
import { AuthService } from '../../../core/services/auth.service';
import { Category } from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-add-event',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './add-event.component.html',
  styleUrls: ['./add-event.component.scss']
})
export class AddEventComponent implements OnInit {
  step = 1;
  loadingCategories = true;
  submitting = false;
  
  eventTypes: any[] = [];
  selectedEventType: any = null;
  eventForm: FormGroup;
  
  servicesRequired = [
    { id: 'venue', label: 'Venue', selected: true },
    { id: 'catering', label: 'Catering', selected: true },
    { id: 'decoration', label: 'Decoration', selected: true },
    { id: 'photography', label: 'Photography', selected: true },
    { id: 'entertainment', label: 'Entertainment', selected: true },
    { id: 'lighting', label: 'Lighting', selected: false }
  ];

  constructor(
    private router: Router, 
    private fb: FormBuilder, 
    private toastService: ToastService,
    private eventService: EventService,
    private categoryService: CategoryService,
    private authService: AuthService
  ) {
    this.eventForm = this.fb.group({
      name: ['', Validators.required],
      date: ['', Validators.required],
      guests: [''],
      location: [''],
      budget: ['', Validators.required],
      notes: ['']
    });
  }

  ngOnInit() {
    this.categoryService.getAll().subscribe({
      next: (categories: Category[]) => {
        // Map backend categories (Event Types) to UI cards
        this.eventTypes = categories.map(c => ({
          id: c.id,
          title: c.name,
          icon: this.getIconForType(c.name),
          desc: `Plan a beautiful ${c.name} with our guided tools`,
          services: 'Suggested customized services'
        }));
        this.loadingCategories = false;
      },
      error: () => {
        this.toastService.show('Failed to load event types', 'error');
        this.loadingCategories = false;
      }
    });
  }

  getIconForType(name: string): string {
    const n = name.toLowerCase();
    if (n.includes('wed')) return '💍';
    if (n.includes('engag')) return '💐';
    if (n.includes('birth')) return '🎂';
    if (n.includes('grad')) return '🎓';
    if (n.includes('corp') || n.includes('bus')) return '🏢';
    if (n.includes('party')) return '🎉';
    return '✨';
  }

  get totalBudget(): number {
    const val = this.eventForm.get('budget')?.value;
    return val ? Number(val) : 0;
  }

  get budgetBreakdownCalculated() {
    const total = this.totalBudget;
    if (!total) return null;
    return {
      venue: { val: total * 0.35, pct: 35 },
      catering: { val: total * 0.25, pct: 25 },
      decoration: { val: total * 0.20, pct: 20 },
      photography: { val: total * 0.12, pct: 12 },
      other: { val: total * 0.08, pct: 8 }
    };
  }

  selectType(type: any) {
    this.selectedEventType = type;
  }

  goToStep2() {
    if (this.selectedEventType) {
      this.step = 2;
      this.eventForm.patchValue({
         name: `My ${this.selectedEventType.title}`
      });
    }
  }

  goToStep1() {
    this.step = 1;
  }

  toggleService(service: any) {
    service.selected = !service.selected;
  }

  createEvent() {
    if (this.eventForm.valid && this.selectedEventType) {
      this.submitting = true;
      const formVal = this.eventForm.value;
      const activeUser = this.authService.user();

      // Extract City/State simply from input (e.g. "New Cairo, Cairo")
      let locCity = 'Cairo';
      let locState = 'Cairo';
      if (formVal.location) {
        const parts = formVal.location.split(',');
        locCity = parts[0]?.trim();
        locState = parts.length > 1 ? parts[1]?.trim() : parts[0]?.trim();
      }

      const createDto: any = {
        userId: activeUser?.id, // Fix 500 error in .NET because IsClient() fails on "User" role
        title: formVal.name,
        categoryId: this.selectedEventType.id,
        eventDate: new Date(formVal.date).toISOString(),
        totalBudget: Number(formVal.budget),
        guestCount: Number(formVal.guests) || 0,
        notes: formVal.notes || '',
        location: formVal.location ? { street: 'Unknown', city: locCity, state: locState } : null
      };

      this.eventService.create(createDto).subscribe({
        next: (createdEvent: any) => {
          this.toastService.show('✨ Event successfully created!', 'success');
          // Navigate to My Events and select the newly created event
          this.router.navigate(['/user/my-events'], { queryParams: { id: createdEvent.id } });
        },
        error: (err: any) => {
          this.toastService.show(err.message || 'Error creating event', 'error');
          this.submitting = false;
        }
      });
    } else {
      this.toastService.show('Please fill in required fields', 'error');
    }
  }

  goBackToDash() {
    this.router.navigate(['/user']);
  }
}
