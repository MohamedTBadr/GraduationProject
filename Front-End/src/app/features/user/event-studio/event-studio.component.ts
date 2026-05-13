import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventService } from '../../../core/services/event.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { AiEventPlanParsed, AiEventPlanResponse } from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-event-studio',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './event-studio.component.html',
  styleUrls: ['./event-studio.component.scss']
})
export class EventStudioComponent {
  @Input() eventId!: string;
  @Input() eventBudget: number = 0;
  @Output() close = new EventEmitter<void>();
  @Output() planAccepted = new EventEmitter<any>();

  eventService = inject(EventService);
  toastService = inject(ToastService);

  isGenerating = false;
  aiPlan: AiEventPlanParsed | null = null;
  error: string | null = null;

  ngOnInit() {
    // Optionally auto-generate when opened, or wait for user click
  }

  generatePlan() {
    this.isGenerating = true;
    this.error = null;
    this.aiPlan = null;

    this.eventService.generateEventByAI(this.eventId).subscribe({
      next: (response: AiEventPlanResponse) => {
        this.isGenerating = false;
        try {
          const planStr = response.aiPlan.replace(/```json/g, '').replace(/```/g, '').trim();
          this.aiPlan = JSON.parse(planStr) as AiEventPlanParsed;
        } catch (e) {
          this.error = "Failed to parse the AI response. Please try again.";
          console.error("Parse error:", e, response.aiPlan);
        }
      },
      error: (err: any) => {
        this.isGenerating = false;
        this.error = "The AI failed to generate a plan. Make sure vendors exist in your budget!";
        console.error("AI Error:", err);
      }
    });
  }

  acceptPlan() {
    // Trigger logic to add the recommended items to the event
    // The user has to click it. We might just close and tell the parent.
    this.planAccepted.emit(this.aiPlan);
    this.toastService.show('AI Plan accepted! Selected vendors are being added to your event.', 'success');
    this.close.emit();
  }
}
