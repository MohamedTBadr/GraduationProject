import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { EventService } from '../../../core/services/event.service';
import { AuthService } from '../../../core/services/auth.service';
import { EventResponseDto } from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  providers: [DatePipe],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  stats = [
    { label: 'Active Events', value: '0', icon: '📅' },
    { label: 'Booked Vendors', value: '0', icon: '🏪' },
    { label: 'Pending Requests', value: '0', icon: '⏳' },
    { label: 'Avg Budget Used', value: '0%', icon: '💰' }
  ];

  events: any[] = [];
  loading = true;

  constructor(
    private router: Router,
    private eventService: EventService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    const user = this.authService.user();
    if (!user || user.role !== 'User') {
      this.loading = false;
      return;
    }

    this.eventService.getByUser(user.id).subscribe({
      next: (data: EventResponseDto[]) => {
        this.processEventsData(data);
        this.loading = false;
      },
      error: () => {
        console.error('Failed to load events');
        this.loading = false;
      }
    });
  }

  processEventsData(data: EventResponseDto[]) {
    let totalVendors = 0;
    let pendingVendors = 0;
    let totalBudgetSum = 0;
    let spentBudgetSum = 0;

    this.events = data.map(ev => {
      const today = new Date();
      const evDate = new Date(ev.eventDate);
      const diffTime = evDate.getTime() - today.getTime();
      const daysLeft = Math.max(0, Math.ceil(diffTime / (1000 * 60 * 60 * 24)));

      const totalEvVendors = ev.eventItems ? ev.eventItems.length : 0;
      const confirmedVendors = ev.eventItems ? ev.eventItems.filter(i => i.itemStatus === 'Approved').length : 0;
      const pendingEvVendors = ev.eventItems ? ev.eventItems.filter(i => i.itemStatus === 'Pending').length : 0;

      totalVendors += confirmedVendors;
      pendingVendors += pendingEvVendors;

      const budgetUsed = ev.eventItems ? ev.eventItems.reduce((acc, item) => acc + ((item.itemStatus === 'Approved' || item.itemStatus === 'Pending') ? item.price : 0), 0) : 0;
      totalBudgetSum += ev.totalBudget || 0;
      spentBudgetSum += budgetUsed;

      return {
        id: ev.id,
        name: ev.title || 'Untitled Event',
        date: ev.eventDate,
        type: ev.categoryName || 'General',
        guests: ev.guestCount || 0,
        daysLeft,
        vendorsConfirmed: confirmedVendors,
        totalVendors: totalEvVendors,
        vendorProgress: totalEvVendors > 0 ? `${confirmedVendors}/${totalEvVendors} Confirmed` : 'No vendors',
        tasksDone: confirmedVendors, 
        totalTasks: totalEvVendors > 0 ? totalEvVendors + 2 : 2, 
        tasksProgress: 'Mock Tasks',
        budgetUsed,
        totalBudget: ev.totalBudget || 1, 
        budgetProgress: ev.totalBudget ? `${Math.round((budgetUsed / ev.totalBudget) * 100)}%` : '0%'
      };
    });

    this.stats[0].value = data.length.toString();
    this.stats[1].value = totalVendors.toString();
    this.stats[2].value = pendingVendors.toString();
    this.stats[3].value = totalBudgetSum > 0 ? `${Math.round((spentBudgetSum / totalBudgetSum) * 100)}%` : '0%';
  }

  goToEvent(id: string) {
    this.router.navigate(['/user/my-events'], { queryParams: { id }});
  }
}
