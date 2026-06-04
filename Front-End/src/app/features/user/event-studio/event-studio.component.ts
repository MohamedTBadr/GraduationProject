import { Component, Input, Output, EventEmitter, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EventService } from '../../../core/services/event.service';
import { AiService } from '../../../core/services/ai.service';
import { ProductService } from '../../../core/services/product.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import {
  AiEventPlanParsed,
  AiEventPlanResponse,
  BudgetAllocationResponse,
  EventTimelineResponse,
  RecommendationItem
} from '../../../shared/types/api.interfaces';

@Component({
  selector: 'app-event-studio',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './event-studio.component.html',
  styleUrls: ['./event-studio.component.scss']
})
export class EventStudioComponent implements OnInit {
  @Input() eventId!: string;
  @Input() eventBudget: number = 0;
  @Input() eventTypeName: string = 'General';
  @Output() close = new EventEmitter<void>();
  @Output() planAccepted = new EventEmitter<any>();

  eventService = inject(EventService);
  aiService = inject(AiService);
  productService = inject(ProductService);
  toastService = inject(ToastService);

  openingServiceId: string | null = null;

  // Tabs state
  activeTab: 'package' | 'budget' | 'timeline' = 'package';

  // Package Planning state (Groq Llama-3 flow)
  isGenerating = false;
  aiPlan: AiEventPlanParsed | null = null;
  error: string | null = null;

  // Collaborative Recommendations
  loadingRecommendations = false;
  recommendations: RecommendationItem[] = [];

  // Smart Budget Allocation state
  loadingBudget = false;
  budgetAllocation: BudgetAllocationResponse | null = null;
  budgetError: string | null = null;

  // Day-of-Event Timeline state
  loadingTimeline = false;
  timelineData: EventTimelineResponse | null = null;
  timelineError: string | null = null;

  ngOnInit() {
    // Pre-fetch recommendations in the background when studio opens
    this.loadRecommendations();
  }

  switchTab(tab: 'package' | 'budget' | 'timeline') {
    this.activeTab = tab;
    if (tab === 'budget' && !this.budgetAllocation) {
      this.loadBudgetAllocation();
    } else if (tab === 'timeline' && !this.timelineData) {
      this.loadTimeline();
    }
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
          const parsed = JSON.parse(planStr) as AiEventPlanParsed;
          if (parsed && parsed.selected_items) {
            parsed.selected_items.forEach((item: any) => {
              item.selected = true;
            });
          }
          this.aiPlan = parsed;
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

  toggleItemSelection(item: any) {
    item.selected = !item.selected;
  }

  loadRecommendations() {
    this.loadingRecommendations = true;
    this.aiService.getClientsLikeYouRecommendations(this.eventId).subscribe({
      next: (res) => {
        this.loadingRecommendations = false;
        this.recommendations = res?.recommendations || [];
      },
      error: (err) => {
        this.loadingRecommendations = false;
        console.error("Error loading recommendations:", err);
      }
    });
  }

  loadBudgetAllocation() {
    this.loadingBudget = true;
    this.budgetError = null;
    this.aiService.getBudgetAllocation(this.eventBudget, this.eventTypeName).subscribe({
      next: (res) => {
        this.loadingBudget = false;
        this.budgetAllocation = res;
      },
      error: (err) => {
        this.loadingBudget = false;
        this.budgetError = "Failed to retrieve smart budget allocation.";
        console.error("Error loading budget allocation:", err);
      }
    });
  }

  loadTimeline() {
    this.loadingTimeline = true;
    this.timelineError = null;
    this.aiService.getEventTimeline(this.eventId).subscribe({
      next: (res) => {
        this.loadingTimeline = false;
        this.timelineData = res;
      },
      error: (err) => {
        this.loadingTimeline = false;
        this.timelineError = "Failed to generate AI day-of timeline.";
        console.error("Error loading timeline:", err);
      }
    });
  }

  acceptPlan() {
    if (!this.aiPlan || !this.aiPlan.selected_items) return;
    const selectedItems = this.aiPlan.selected_items.filter((item: any) => item.selected !== false);
    
    if (selectedItems.length === 0) {
      this.toastService.show('Please select at least one vendor to add.', 'info');
      return;
    }
    
    const planToEmit = {
      ...this.aiPlan,
      selected_items: selectedItems
    };

    this.planAccepted.emit(planToEmit);
    this.toastService.show('Selected vendors are being added to your event.', 'success');
    this.close.emit();
  }

  openServiceDetails(rec: RecommendationItem) {
    if (this.openingServiceId === rec.ServiceId) return;
    this.openingServiceId = rec.ServiceId;
    this.productService.getById(rec.ServiceId).subscribe({
      next: (svc) => {
        this.openingServiceId = null;
        if (svc?.vendorId) {
          window.open(`/vendor/${svc.vendorId}`, '_blank');
        } else {
          this.toastService.show('Could not find vendor details.', 'error');
        }
      },
      error: () => {
        this.openingServiceId = null;
        this.toastService.show('Failed to load service details.', 'error');
      }
    });
  }
}
