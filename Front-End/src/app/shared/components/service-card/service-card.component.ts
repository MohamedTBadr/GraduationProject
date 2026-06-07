import { Component, Input, Output, EventEmitter, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiProduct } from '../../types/api.interfaces';
import { getProductCoverImage, getProductImageUrls } from '../../utils/image.utils';

@Component({
  selector: 'app-service-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './service-card.component.html',
  styleUrls: ['./service-card.component.scss']
})
export class ServiceCardComponent implements OnChanges {
  @Input() service!: ApiProduct;
  @Input() viewMode: 'list' | 'grid' = 'list';
  @Output() actionClick = new EventEmitter<string>();

  imageLoadFailed = false;

  ngOnChanges(): void {
    this.imageLoadFailed = false;
  }

  get coverImage(): string | null {
    if (this.imageLoadFailed) return null;
    return getProductCoverImage(this.service);
  }

  get imageCount(): number {
    return getProductImageUrls(this.service).length;
  }

  onImageError(): void {
    this.imageLoadFailed = true;
  }
}
