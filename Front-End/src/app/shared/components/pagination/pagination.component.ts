import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { buildPageNumbers } from '../../utils/pagination.util';

@Component({
  selector: 'app-pagination',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './pagination.component.html',
  styleUrls: ['./pagination.component.scss']
})
export class PaginationComponent {
  @Input() currentPage = 1;
  @Input() totalPages = 1;
  @Input() totalCount = 0;
  @Input() pageSize = 10;
  @Input() variant: 'explore' | 'admin' | 'plain' = 'plain';
  @Input() showSummary = true;
  @Input() scrollToTop = true;

  @Output() pageChange = new EventEmitter<number>();

  get pageNumbers(): number[] {
    return buildPageNumbers(this.currentPage, this.totalPages);
  }

  get visible(): boolean {
    return this.totalPages > 1 || (this.showSummary && this.totalCount > 0);
  }

  goTo(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.pageChange.emit(page);
    if (this.scrollToTop) {
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }
}
