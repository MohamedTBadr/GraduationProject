import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { EventService } from '../../../core/services/event.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { CreateEventDto } from '../../../shared/types/api.interfaces';
import { EventTypeService } from '../../../core/services/event-type.service';


@Component({
  selector: 'app-add-event',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-event.component.html',
  styleUrls: ['./add-event.component.scss']
})
export class AddEventComponent implements OnInit {
  step = 1;
  isSubmitting = false;
  
  eventTypes = [
    { id: 'wedding', title: 'Wedding', icon: '💍', desc: 'Romantic ceremony & reception for your big day', services: 'Venue · Catering · Decor · Photography · DJ' },
    { id: 'engagement', title: 'Engagement', icon: '💐', desc: 'Intimate celebration for your forever commitment', services: 'Venue · Decor · Desserts · Photography' },
    { id: 'birthday', title: 'Birthday', icon: '🎂', desc: 'A memorable party for any age milestone', services: 'Venue · Decor · Cake · Entertainment' },
    { id: 'graduation', title: 'Graduation', icon: '🎓', desc: 'Celebrate the achievement with family and friends', services: 'Venue · Decor · Photography · Catering' },
    { id: 'corporate', title: 'Corporate', icon: '🏢', desc: 'Professional events, conferences & product launches', services: 'Venue · AV · Catering · Branding' },
    { id: 'custom', title: 'Custom Event', icon: '✨', desc: 'Build your own event from scratch, your way', services: 'Choose any services' }
  ];

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
    private eventService: EventService,
    private authService: AuthService,
    private toastService: ToastService,
    private eventTypeService: EventTypeService
  ) {
    this.eventForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      date: ['', Validators.required],
      guests: ['', [Validators.min(1)]],
      city: ['', Validators.required],
      state: ['', Validators.required],
      street: [''],
      budget: ['50000', [Validators.required, Validators.min(1000)]],
      notes: ['']
    });
  }

  getBudgetPart(percent: number): number {
    const b = this.eventForm.get('budget')?.value;
    return (Number(b) || 0) * percent;
  }

  ngOnInit() {
    this.eventTypeService.getAll().subscribe({
      next: (backendTypes) => {
        if (backendTypes && backendTypes.length > 0) {
          this.eventTypes = backendTypes.map(bt => {
            const hardcoded = this.eventTypes.find(h => h.title.toLowerCase() === bt.name.toLowerCase()) 
              || { id: bt.id, title: bt.name, icon: '✨', desc: 'Custom Event', services: 'Choose any services' };
            return {
              ...hardcoded,
              id: bt.id
            };
          });
        }
      },
      error: (err) => {
        console.error('Failed to fetch event types', err);
      }
    });
  }

  selectType(type: any) {
    this.selectedEventType = type;
  }

  goToStep2() {
    if (this.selectedEventType) {
      this.step = 2;
      this.eventForm.patchValue({
         name: `Sara & Omar's ${this.selectedEventType.title}`
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
    if (this.eventForm.invalid) {
      this.toastService.show('Please fill in all required fields.', 'error');
      return;
    }

    const user = this.authService.user();
    if (!user) {
      this.toastService.show('Please login to create an event.', 'error');
      this.router.navigate(['/auth/login']);
      return;
    }

    this.isSubmitting = true;
    const formValues = this.eventForm.value;

    const createDto: CreateEventDto = {
      userId: user.id,
      title: formValues.name,
      eventTypeId: this.selectedEventType?.id || '',
      eventDate: new Date(formValues.date).toISOString(),
      totalBudget: Number(formValues.budget) || 0,
      guestCount: Number(formValues.guests) || 0,
      notes: formValues.notes,
      location: {
        street: formValues.street || 'Unknown',
        city: formValues.city,
        state: formValues.state
      }
    };

    this.eventService.create(createDto).subscribe({
      next: (res: any) => {
        this.isSubmitting = false;
        this.toastService.show('Event created successfully!', 'success');

        // Backend returns a Result wrapper: { isSuccess, value: { id, ... } }
        const eventId = res?.value?.id ?? res?.id ?? null;
        if (eventId) {
          this.router.navigate(['/user/my-events'], { queryParams: { id: eventId } });
        } else {
          this.router.navigate(['/user/my-events']);
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        this.toastService.show('Failed to create event. Please try again.', 'error');
      }
    });
  }

  goBackToDash() {
    this.router.navigate(['/user/dashboard']);
  }
}
