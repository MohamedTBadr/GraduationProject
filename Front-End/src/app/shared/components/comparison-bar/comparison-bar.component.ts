import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CompareService } from '../../services/compare.service';

@Component({
  selector: 'app-comparison-bar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="comparison-bar" [class.open]="compareService.compareCount() > 0">
      <div class="container bar-content">
        <div class="bar-left">
          <div class="count-badge">{{compareService.compareCount()}}</div>
          <div>
            <strong>Vendors to Compare</strong>
            <p>Select up to 3 vendors to see a side-by-side break down.</p>
          </div>
        </div>
        
        <div class="bar-vendors">
          <div class="bar-vendor" *ngFor="let vendor of compareService.compareListItems()">
            <!-- <span class="bv-emoji">{{vendor.emoji}}</span> -->
            <span class="bv-name">{{vendor.name}}</span>
            <button class="bv-remove" (click)="compareService.toggleCompare(vendor)"></button>
          </div>
          <div class="bar-empty" *ngIf="compareService.compareCount() < 3">
            <span>+ Add More</span>
          </div>
        </div>

        <div class="bar-actions">
          <button class="btn btn-ghost btn-sm" (click)="compareService.clearCompare()">Clear All</button>
          <button class="btn btn-gold btn-sm" routerLink="/compare" [disabled]="compareService.compareCount() < 2">
            Compare Now →
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .comparison-bar {
      position: fixed;
      bottom: 0;
      left: 0;
      right: 0;
      background: var(--navy);
      color: white;
      padding: 16px 0;
      z-index: 1000;
      transform: translateY(100%);
      transition: transform 0.4s cubic-bezier(0.16, 1, 0.3, 1);
      box-shadow: 0 -10px 40px rgba(0,0,0,0.3);
      border-top: 1px solid rgba(255,255,255,0.1);
    }
    .comparison-bar.open { transform: translateY(0); }
    .bar-content { display: flex; align-items: center; justify-content: space-between; gap: 24px; }
    .bar-left { display: flex; align-items: center; gap: 16px; }
    .count-badge {
      width: 40px; height: 40px; border-radius: 50%; background: var(--gold);
      color: var(--navy); display: flex; align-items: center; justify-content: center;
      font-weight: 800; font-size: 1.2rem;
    }
    .bar-left p { margin: 0; font-size: 0.8rem; color: rgba(255,255,255,0.6); }
    .bar-vendors { flex: 1; display: flex; gap: 12px; }
    .bar-vendor {
      background: rgba(255,255,255,0.08); padding: 8px 12px; border-radius: 50px;
      display: flex; align-items: center; gap: 8px; border: 1px solid rgba(255,255,255,0.15);
    }
    .bv-emoji { font-size: 1.2rem; }
    .bv-name { font-size: 0.85rem; font-weight: 600; }
    .bv-remove { 
      background: none; border: none; color: rgba(255,255,255,0.4); 
      cursor: pointer; padding: 2px 4px; font-size: 0.7rem;
    }
    .bv-remove:hover { color: white; }
    .bar-empty {
      border: 1px dashed rgba(255,255,255,0.2); border-radius: 50px;
      padding: 8px 16px; font-size: 0.8rem; color: rgba(255,255,255,0.4);
      display: flex; align-items: center;
    }
    .bar-actions { display: flex; gap: 12px; align-items: center; }
  `]
})
export class ComparisonBarComponent {
  compareService = inject(CompareService);
}
