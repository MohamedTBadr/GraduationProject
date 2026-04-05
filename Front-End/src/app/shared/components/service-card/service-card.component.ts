import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { VendorService } from '../../types/vendor.interface';

@Component({
  selector: 'app-service-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './service-card.component.html',
  styleUrls: ['./service-card.component.scss']
})
export class ServiceCardComponent {
  @Input() service!: VendorService;
  @Input() viewMode: 'list' | 'grid' = 'list';
  @Output() actionClick = new EventEmitter<string>();

  get coverImage(): string | null {
    if (this.service?.images && this.service.images.length > 0) {
      return this.service.images[0];
    }
    return null;
  }
}
