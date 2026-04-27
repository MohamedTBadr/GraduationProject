import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ModalService } from '../../services/modal.service';
import { AuthService } from '../../../core/services/auth.service';
import { EventService } from '../../../core/services/event.service';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../components/toast/toast.service';
import { Router } from '@angular/router';
import { LoginComponent } from '../../../features/auth/login/login.component';
import { RegisterComponent } from '../../../features/auth/register/register.component';
import { CreateEventItemDto, CreateEventDto, EventResponseDto, Category } from '../../types/api.interfaces';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, LoginComponent, RegisterComponent],
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss']
})
export class ModalComponent implements OnInit {
  modalService = inject(ModalService);
  authService = inject(AuthService);
  router = inject(Router);
  eventService = inject(EventService);
  categoryService = inject(CategoryService);
  toastService = inject(ToastService);

  // For enter-event-details
  eventData = {
    title: '',
    eventTypeId: '',
    eventDate: '',
    guests: null,
    budget: null,
    location: '',
    notes: ''
  };
  eventTypes: Category[] = [];

  // For choose-event
  userEvents: EventResponseDto[] = [];
  selectedEventId: string = '';
  showEventDropdown = false;
  submitting = false;

  ngOnInit() {
    this.categoryService.getAll().subscribe({
      next: (categories) => this.eventTypes = categories
    });
  }

  handleServiceRequest(product: any) {
    if (!this.authService.isLoggedIn()) {
      this.modalService.open('login');
      return;
    }

    const userId = this.authService.user()?.id;
    if (!userId) return;

    this.submitting = true;
    this.eventService.getByUser(userId).subscribe({
      next: (events) => {
        this.submitting = false;
        if (events.length === 0) {
          this.eventData.title = `My New Event`;
          this.modalService.open('enter-event-details', { product });
        } else if (events.length === 1) {
          this.addItemToEvent(events[0].id, product);
        } else {
          this.userEvents = events;
          if (events.length > 0) this.selectedEventId = events[0].id;
          this.showEventDropdown = true;
        }
      },
      error: (err) => {
        this.submitting = false;
        this.toastService.show('Failed to fetch events', 'error');
      }
    });
  }

  submitNewEventAndAdd() {
    if (!this.eventData.title || !this.eventData.eventTypeId || !this.eventData.eventDate) {
      this.toastService.show('Please fill required fields', 'error');
      return;
    }
    
    this.submitting = true;
    const createDto: CreateEventDto = {
      userId: this.authService.user()?.id,
      title: this.eventData.title,
      eventTypeId: this.eventData.eventTypeId,
      eventDate: new Date(this.eventData.eventDate).toISOString(),
      totalBudget: Number(this.eventData.budget) || 0,
      guestCount: Number(this.eventData.guests) || 0,
      notes: this.eventData.notes,
      location: this.eventData.location ? { street: 'Unknown', city: this.eventData.location, state: this.eventData.location } : undefined
    };

    this.eventService.create(createDto).subscribe({
      next: (createdEvent) => {
        const product = this.modalService.modalData()?.product;
        this.addItemToEvent(createdEvent.id, product);
      },
      error: (err) => {
        this.submitting = false;
        this.toastService.show('Failed to create event', 'error');
      }
    });
  }

  submitChooseEvent(product: any) {
    if (!this.selectedEventId) return;
    
    if (this.selectedEventId === 'NEW_EVENT') {
      this.showEventDropdown = false;
      this.eventData.title = `My New Event`;
      this.modalService.open('enter-event-details', { product });
      return;
    }

    this.addItemToEvent(this.selectedEventId, product);
  }

  addItemToEvent(eventId: string, product: any) {
    if (!product) return;
    
    this.submitting = true;
    const itemDto: CreateEventItemDto = {
      eventId: eventId,
      serviceId: product.id,
      serviceImage: product.imageUrl || '',
      serviceName: product.name,
      price: product.price,
      vendorId: product.vendorId,
      vendorName: product.vendorName || 'Vendor',
      quantity: 1
    };

    this.eventService.addItem(eventId, itemDto).subscribe({
      next: () => {
        this.submitting = false;
        this.showEventDropdown = false;
        this.toastService.show(`${product.name} added to your event!`, 'success');
        this.modalService.close();
        // Redirect to event detail to show progress
        this.router.navigate(['/user/my-events'], { queryParams: { id: eventId } });
      },
      error: (err) => {
        this.submitting = false;
        this.toastService.show('Failed to add service to event.', 'error');
      }
    });
  }


  onOverlayClick(event: MouseEvent) {
    if ((event.target as HTMLElement).classList.contains('modal-overlay')) {
      this.modalService.close();
    }
  }
}
