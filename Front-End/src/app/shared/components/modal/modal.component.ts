import { Component, inject, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ModalService } from '../../services/modal.service';
import { AuthService } from '../../../core/services/auth.service';
import { EventService } from '../../../core/services/event.service';
import { EventTypeService } from '../../../core/services/event-type.service';
import { ToastService } from '../../components/toast/toast.service';
import { Router, RouterLink } from '@angular/router';
import { LoginComponent } from '../../../features/auth/login/login.component';
import { RegisterComponent } from '../../../features/auth/register/register.component';
import { CreateEventItemDto, CreateEventDto, EventResponseDto } from '../../types/api.interfaces';
import { EventType } from '../../../core/models/taxonomy.models';
import {
  EGYPT_CITY_OPTIONS,
  EGYPT_GOVERNORATE_OPTIONS,
  getLocationByCity
} from '../../constants/egypt-locations';
import {
  normalizeAddressFields,
  serviceAreasToLabel
} from '../../utils/location.utils';
import { getProductImageUrls } from '../../utils/image.utils';
import { FavoriteService } from '../../services/favorite.service';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, LoginComponent, RegisterComponent, RouterLink],
  templateUrl: './modal.component.html',
  styleUrls: ['./modal.component.scss']
})
export class ModalComponent implements OnInit {
  modalService = inject(ModalService);
  authService = inject(AuthService);
  router = inject(Router);
  eventService = inject(EventService);
  eventTypeService = inject(EventTypeService);
  toastService = inject(ToastService);
  favoriteService = inject(FavoriteService);

  constructor() {
    effect(() => {
      // Trigger whenever activeModal or modalData changes
      this.modalService.activeModal();
      this.modalService.modalData();
      this.currentSlideIndex = 0;
    });
  }

  // For enter-event-details
  eventData = {
    title: '',
    eventTypeId: '',
    eventDate: '',
    guests: null,
    budget: null,
    street: '',
    city: '',
    state: '',
    notes: ''
  };
  eventTypes: EventType[] = [];

  // For choose-event
  userEvents: EventResponseDto[] = [];
  selectedEventId: string = '';
  showEventDropdown = false;
  submitting = false;

  // Image slider state
  currentSlideIndex = 0;

  ngOnInit() {
    this.eventTypeService.getAll().subscribe({
      next: (types) => this.eventTypes = types
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
    this.eventService.getByUser().subscribe({
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
    if (!this.eventData.title || !this.eventData.eventTypeId || !this.eventData.eventDate || !this.eventData.city) {
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
      location: this.eventData.city
        ? (() => {
            const { city, state } = normalizeAddressFields(this.eventData.city, this.eventData.state);
            return {
              street: this.eventData.street || '',
              city,
              state
            };
          })()
        : undefined
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

  // ── Image Slider Helpers ──────────────────────────────
  getModalImages(product: any): string[] {
    return getProductImageUrls(product);
  }

  prevSlide(images: string[]) {
    this.currentSlideIndex = this.currentSlideIndex > 0
      ? this.currentSlideIndex - 1
      : images.length - 1;
  }

  nextSlide(images: string[]) {
    this.currentSlideIndex = this.currentSlideIndex < images.length - 1
      ? this.currentSlideIndex + 1
      : 0;
  }

  goToSlide(index: number) {
    this.currentSlideIndex = index;
  }

  // Reset slider when modal data changes
  resetSlider() {
    this.currentSlideIndex = 0;
  }

  // ── Favorites Helpers ────────────────────────────────
  toggleFavorite(product: { vendorId?: string }, event?: Event) {
    if (event) event.stopPropagation();
    const vendorId = product.vendorId;
    if (!vendorId) {
      this.toastService.show('Cannot save — vendor unavailable', 'error');
      return;
    }
    this.favoriteService.toggleFavorite(vendorId);
    const isFav = this.favoriteService.isFavorite(vendorId);
    this.toastService.show(isFav ? 'Saved to favorites!' : 'Removed from favorites', isFav ? 'success' : 'info');
  }

  isFavorite(product: { vendorId?: string }): boolean {
    return !!product.vendorId && this.favoriteService.isFavorite(product.vendorId);
  }

  // ── Vendor Navigation ────────────────────────────────
  navigateToVendor(vendorId: string) {
    if (!vendorId) return;
    this.modalService.close();
    this.router.navigate(['/vendor', vendorId]);
  }

  readonly cityOptions = EGYPT_CITY_OPTIONS;
  readonly governorateOptions = EGYPT_GOVERNORATE_OPTIONS;

  onEventCityChange(city: string): void {
    const loc = getLocationByCity(city);
    if (loc) {
      this.eventData.state = loc.governorate;
    }
  }

  // Get service area location label
  getLocation(product: any): string {
    if (!product?.serviceAreas?.length) return '';
    return serviceAreasToLabel(product.serviceAreas);
  }

  // Get star rating as array for rendering
  getStars(rating: number): number[] {
    return Array.from({ length: 5 }, (_, i) => i);
  }
}
