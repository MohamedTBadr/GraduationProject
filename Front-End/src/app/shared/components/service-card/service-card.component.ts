import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiProduct } from '../../types/api.interfaces';
import { getProductCoverImage } from '../../utils/image.utils';

@Component({
  selector: 'app-service-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './service-card.component.html',
  styleUrls: ['./service-card.component.scss']
})
export class ServiceCardComponent {
  @Input() service!: ApiProduct;
  @Input() viewMode: 'list' | 'grid' = 'list';
  @Output() actionClick = new EventEmitter<string>();

  get coverImage(): string | null {
    return getProductCoverImage(this.service);
  }
}
