import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, ActivatedRoute, Router } from '@angular/router';

interface EventVendor {
  emoji: string;
  name: string;
  type: string;
  price: number;
  status: 'confirmed' | 'pending';
}

interface ChecklistItem {
  text: string;
  done: boolean;
}

interface EventData {
  id: number;
  name: string;
  date: string;
  type: string;
  guests: number;
  budget: number;
  vendors: EventVendor[];
  checklist: ChecklistItem[];
}

@Component({
  selector: 'app-my-events',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './my-events.component.html',
  styleUrls: ['./my-events.component.scss']
})
export class MyEventsComponent implements OnInit {
  events: EventData[] = [
    {
      id: 1,
      name: 'Engagement Party',
      date: '2026-06-12',
      type: 'Engagement',
      guests: 60,
      budget: 20000,
      vendors: [
        { emoji: '️', name: 'Nile City Venue', type: 'Venue', price: 5000, status: 'confirmed' },
        { emoji: '', name: 'White Rose Decor', type: 'Decoration', price: 4200, status: 'confirmed' },
        { emoji: '', name: 'Studio Lens', type: 'Photography', price: 0, status: 'pending' },
        { emoji: '️', name: 'Royal Catering', type: 'Catering', price: 3200, status: 'pending' }
      ],
      checklist: [
        { text: 'Book venue', done: true },
        { text: 'Choose decor vendor', done: true },
        { text: 'Set guest list', done: true },
        { text: 'Confirm catering menu', done: false },
        { text: 'Book photographer', done: false },
        { text: 'Send invitations', done: false },
        { text: 'Order flowers', done: false }
      ]
    },
    {
      id: 2,
      name: 'Brother\'s Wedding',
      date: '2026-09-20',
      type: 'Wedding',
      guests: 250,
      budget: 55000,
      vendors: [
        { emoji: '', name: 'Nile City Venue', type: 'Venue', price: 30000, status: 'confirmed' },
        { emoji: '️', name: 'Elite Catering', type: 'Catering', price: 12000, status: 'confirmed' }
      ],
      checklist: [
        { text: 'Book wedding hall', done: true },
        { text: 'Choose catering', done: true },
        { text: 'Photography quote', done: false }
      ]
    },
    {
      id: 3,
      name: 'Company Conference',
      date: '2026-04-05',
      type: 'Corporate',
      guests: 120,
      budget: 35000,
      vendors: [
        { emoji: '', name: 'Sound Systems Pro', type: 'AV', price: 15000, status: 'confirmed' }
      ],
      checklist: [
        { text: 'Book speakers', done: true },
        { text: 'Arrange seating', done: false }
      ]
    }
  ];

  activeEventId = 1;

  constructor(private route: ActivatedRoute, private router: Router) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
        if(params['id']) {
            this.activeEventId = Number(params['id']);
        }
    });
  }

  get activeEvent() {
    return this.events.find(e => e.id === this.activeEventId);
  }

  get spent() {
    return this.activeEvent?.vendors.reduce((sum, v) => sum + v.price, 0) || 0;
  }

  get budgetPct() {
    if (!this.activeEvent) return 0;
    return Math.min(100, Math.round((this.spent / this.activeEvent.budget) * 100));
  }

  get daysLeft() {
    if (!this.activeEvent) return 0;
    const diff = new Date(this.activeEvent.date).getTime() - new Date().getTime();
    return Math.ceil(diff / (1000 * 60 * 60 * 24));
  }

  switchEvent(id: number) {
    this.activeEventId = id;
  }

  toggleCheck(index: number) {
    if (this.activeEvent) {
      this.activeEvent.checklist[index].done = !this.activeEvent.checklist[index].done;
    }
  }

  openAddEvent() {
    this.router.navigate(['/add-event']);
  }
}
