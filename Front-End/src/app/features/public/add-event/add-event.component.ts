import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-add-event',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './add-event.component.html',
  styleUrls: ['./add-event.component.scss']
})
export class AddEventComponent {
  step = 1;

  eventType = '';
  eventName = '';
  eventDate = '';
  location = 'Cairo';
  guestCount = 50;
  budgetRange = '50k - 150k';
  services: string[] = [];

  QUICKSTARTS: any = {
    'Wedding': {
      checklist: ['Book Venue', 'Find Photographer', 'Select Catering', 'Order Flowers', 'Send Invitations'],
      budget: { 'Venue': 40, 'Catering': 30, 'Decor': 15, 'Photo': 10, 'Other': 5 }
    },
    'Engagement': {
      checklist: ['Venue Booking', 'Dress/Suit', 'Ring Selection', 'Photography', 'Small Catering'],
      budget: { 'Venue': 35, 'Catering': 25, 'Decor': 20, 'Dress': 10, 'Photo': 10 }
    },
    'Birthday': {
      checklist: ['Venue/House Prep', 'Cake Order', 'Decoration', 'Entertainment', 'Guest List'],
      budget: { 'Venue': 20, 'Catering': 40, 'Decor': 20, 'Entertainment': 15, 'Cake': 5 }
    }
  };

  constructor(private toastService: ToastService, private router: Router) { }

  selectType(type: string) {
    this.eventType = type;
    if (!this.eventName) {
      this.eventName = `My ${type} Event`;
    }
  }

  toggleService(service: string) {
    if (this.services.includes(service)) {
      this.services = this.services.filter(s => s !== service);
    } else {
      this.services.push(service);
    }
  }

  nextStep() {
    if (this.step === 1 && (!this.eventType || !this.eventName)) {
      this.toastService.show('Please select event type and name', 'error');
      return;
    }

    if (this.step < 4) {
      this.step++;
      if (this.step === 4) {
        this.saveEvent();
      }
    }
  }

  prevStep() {
    if (this.step > 1) {
      this.step--;
    }
  }

  saveEvent() {
    const template = this.QUICKSTARTS[this.eventType] || { checklist: [], budget: {} };
    const eventData = {
      id: Date.now(),
      name: this.eventName,
      type: this.eventType,
      date: this.eventDate,
      location: this.location,
      guests: this.guestCount,
      budgetRange: this.budgetRange,
      services: this.services,
      checklist: template.checklist.map((task: string) => ({ task, completed: false })),
      budgetBreakdown: template.budget
    };

    // Store in localStorage for prototype purpose
    const events = JSON.parse(localStorage.getItem('eventora_user_events') || '[]');
    events.push(eventData);
    localStorage.setItem('eventora_user_events', JSON.stringify(events));

    this.toastService.show('✨ Event successfully created!', 'success');
  }
}
