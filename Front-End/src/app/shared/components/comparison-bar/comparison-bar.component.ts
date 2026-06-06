import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CompareService } from '../../services/compare.service';

@Component({
  selector: 'app-comparison-bar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="compare-bar" [class.show]="compareService.compareCount() > 0">
      <div class="compare-bar-title">
        <i class="bi bi-arrow-left-right"></i>
        Compare ({{ compareService.compareCount() }}/3)
      </div>

      <div class="compare-slots">
        <div class="compare-slot filled" *ngFor="let vendor of compareService.compareListItems()">
          <span class="compare-slot-name">{{ vendor.name }}</span>
          <button type="button" class="compare-slot-remove" (click)="compareService.toggleCompare(vendor)" title="Remove">
            <i class="bi bi-x"></i>
          </button>
        </div>
        <div class="compare-slot" *ngIf="compareService.compareCount() < 3">+ Add vendor</div>
      </div>

      <div class="compare-bar-actions">
        <button type="button" class="btn compare-bar-clear btn-sm" (click)="compareService.clearCompare()">Clear</button>
        <button type="button" class="btn compare-bar-cta btn-sm" routerLink="/compare" [queryParams]="{ tab: 'vendors' }" [disabled]="compareService.compareCount() < 2">
          Compare Now →
        </button>
      </div>
    </div>
  `
})
export class ComparisonBarComponent {
  compareService = inject(CompareService);
}
