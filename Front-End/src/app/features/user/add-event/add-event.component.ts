import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-add-event',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-event.component.html',
  styleUrls: ['./add-event.component.scss']
})
export class AddEventComponent {
  step = 1;
  
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

  constructor(private router: Router, private fb: FormBuilder) {
    this.eventForm = this.fb.group({
      name: ['', Validators.required],
      date: ['', Validators.required],
      guests: [''],
      location: [''],
      budget: ['50000', Validators.required],
      notes: ['']
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
    if (this.eventForm.valid) {
      // In a real app we would call an API here
      this.router.navigate(['/user/dashboard']);
    }
  }

  goBackToDash() {
    this.router.navigate(['/user/dashboard']);
  }
}
