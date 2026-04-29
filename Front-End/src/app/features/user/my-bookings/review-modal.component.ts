import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReviewService } from '../../../core/services/review.service';
import { ToastService } from '../../../shared/components/toast/toast.service';

@Component({
  selector: 'app-review-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="vd-modal-overlay" [class.open]="isOpen">
      <div class="vd-modal">
        <button class="vd-modal-close" (click)="closeModal()">✕</button>
        <div class="vd-modal-title">Leave a Review</div>
        <div class="vd-modal-sub mb-[20px]">
          Rate your experience and let others know what you think.
        </div>

        <div class="vd-form-group">
          <label class="mb-[8px]">Rating <span class="text-[#e11d48]">*</span></label>
          <div class="flex gap-[8px] text-[1.5rem] mb-[10px]">
            <span *ngFor="let star of [1,2,3,4,5]" 
                  (click)="rating = star"
                  [ngStyle]="{'color': star <= rating ? '#eba834' : '#ccc', 'cursor': 'pointer'}">
              ★
            </span>
          </div>
        </div>

        <div class="vd-form-group mt-[20px]">
          <label class="mb-[8px]">Review <span class="text-[#e11d48]">*</span></label>
          <textarea class="w-[100%] rounded-[8px] p-[12px] text-[0.9rem] border border-[var(--lgray)]" 
                    rows="4" 
                    placeholder="Tell us about the service..." 
                    [(ngModel)]="reviewText"></textarea>
        </div>

        <div class="vd-form-group mt-[20px]">
          <label class="mb-[8px]">Upload Event Photo (Optional)</label>
          <input type="file" class="w-[100%] text-[0.9rem]" (change)="onFileSelected($event)">
        </div>

        <div class="vd-submit-bar pt-[20px] mt-[24px] justify-center gap-[14px] [border-top:1px_solid_var(--lgray)]">
          <button class="vd-btn vd-btn-ghost py-[10px] px-[24px] rounded-[50px]" (click)="closeModal()">Cancel</button>
          <button class="vd-btn py-[10px] px-[24px] rounded-[50px]" 
                  style="background-color: #eba834; color: white;" 
                  (click)="submitReview()" 
                  [disabled]="rating === 0 || !reviewText.trim()">
            Submit Review
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .vd-modal-overlay {
      position: fixed; top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(0,0,0,0.5);
      display: flex; align-items: center; justify-content: center;
      z-index: 1000;
      opacity: 0; pointer-events: none; transition: 0.2s;
    }
    .vd-modal-overlay.open {
      opacity: 1; pointer-events: auto;
    }
    .vd-modal {
      background: white; border-radius: 12px;
      padding: 24px; width: 90%; max-width: 480px;
      position: relative;
    }
    .vd-modal-close {
      position: absolute; top: 16px; right: 16px;
      background: none; border: none; font-size: 1.2rem; cursor: pointer;
    }
    .vd-modal-title { font-size: 1.25rem; font-weight: bold; color: var(--navy); }
    .vd-modal-sub { font-size: 0.9rem; color: var(--gray); }
    .vd-form-group label { display: block; font-weight: 600; color: var(--navy); }
    .vd-btn { cursor: pointer; font-weight: 600; border: none; }
    .vd-btn-ghost { background: transparent; color: var(--navy); }
    .vd-submit-bar { display: flex; }
  `]
})
export class ReviewModalComponent {
  @Input() isOpen = false;
  @Input() serviceId = '';
  @Input() userId = '';
  @Output() close = new EventEmitter<void>();

  rating = 0;
  reviewText = '';
  photoUrl = ''; // Mock photo handling

  constructor(private reviewService: ReviewService, private toastService: ToastService) {}

  onFileSelected(event: any) {
    // In a real scenario, this would upload the file to S3 and set the photoUrl.
    // We will just mock it for now.
    this.photoUrl = 'https://mock-s3-url.com/event-photo.jpg';
  }

  submitReview() {
    if (this.rating === 0 || !this.reviewText.trim()) return;

    this.reviewService.submitReview({
      userId: this.userId,
      serviceId: this.serviceId,
      rating: this.rating,
      review: this.reviewText,
      photoUrl: this.photoUrl
    }).subscribe({
      next: () => {
        this.toastService.show('Review submitted successfully!', 'success');
        this.closeModal();
      },
      error: (err: any) => {
        console.error('Error submitting review', err);
        // Mock success since endpoint doesn't exist yet
        this.toastService.show('Review submitted successfully! (Mock)', 'success');
        this.closeModal();
      }
    });
  }

  closeModal() {
    this.rating = 0;
    this.reviewText = '';
    this.photoUrl = '';
    this.close.emit();
  }
}
