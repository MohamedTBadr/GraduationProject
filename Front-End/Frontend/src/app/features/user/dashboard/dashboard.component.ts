import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  stats = [
    { label: 'Active Events', value: '3', icon: '📅' },
    { label: 'Booked Vendors', value: '7', icon: '' },
    { label: 'Tasks Remaining', value: '12', icon: '' },
    { label: 'Budget Used', value: '68%', icon: '' }
  ];

  events = [
    {
      id: 1,
      name: 'Engagement Party',
      date: '2026-06-12',
      type: 'Engagement',
      guests: 60,
      daysLeft: 92,
      vendorsConfirmed: 2,
      totalVendors: 4,
      vendorProgress: '2/4 Confirmed',
      tasksDone: 4,
      totalTasks: 12,
      tasksProgress: '8 Remaining',
      budgetUsed: 6200,
      totalBudget: 10000,
      budgetProgress: '62%'
    },
    {
      id: 2,
      name: 'Brother\'s Wedding',
      date: '2026-09-20',
      type: 'Wedding',
      guests: 250,
      daysLeft: 192,
      vendorsConfirmed: 4,
      totalVendors: 6,
      vendorProgress: '4/6 Confirmed',
      tasksDone: 2,
      totalTasks: 15,
      tasksProgress: '13 Remaining',
      budgetUsed: 35000,
      totalBudget: 80000,
      budgetProgress: '43%'
    },
    {
      id: 3,
      name: 'Company Conference',
      date: '2026-04-05',
      type: 'Corporate',
      guests: 120,
      daysLeft: 24,
      vendorsConfirmed: 1,
      totalVendors: 1,
      vendorProgress: '1/1 Confirmed',
      tasksDone: 0,
      totalTasks: 2,
      tasksProgress: '2 Remaining',
      budgetUsed: 19950,
      totalBudget: 35000,
      budgetProgress: '57%'
    }
  ];

  constructor(private router: Router) {}

  goToEvent(id: number) {
    this.router.navigate(['/user/my-events'], { queryParams: { id }});
  }
}
