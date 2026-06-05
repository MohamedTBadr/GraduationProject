import { Component, Input, Output, EventEmitter, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EventService } from '../../../core/services/event.service';
import { AiService } from '../../../core/services/ai.service';
import { ProductService } from '../../../core/services/product.service';
import { EventTypeService } from '../../../core/services/event-type.service';
import { ToastService } from '../../../shared/components/toast/toast.service';
import { EventType } from '../../../core/models/taxonomy.models';
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
  imports: [CommonModule, FormsModule],
  templateUrl: './event-studio.component.html',
  styleUrls: ['./event-studio.component.scss']
})
export class EventStudioComponent implements OnInit {
  @Input() eventId!: string;
  @Input() eventBudget: number = 0;
  @Input() eventTypeName: string = 'General';
  @Output() close = new EventEmitter<void>();
  @Output() planAccepted = new EventEmitter<any>();
  @Output() eventUpdated = new EventEmitter<void>();

  eventService = inject(EventService);
  aiService = inject(AiService);
  productService = inject(ProductService);
  eventTypeService = inject(EventTypeService);
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

  // Playground state
  selectedBudget: number = 0;
  selectedEventType: string = '';
  eventTypes: EventType[] = [];
  isSavingBudget = false;
  hoveredSegment: string | null = null;

  // Day-of-Event Timeline state
  loadingTimeline = false;
  timelineData: EventTimelineResponse | null = null;
  timelineError: string | null = null;

  // Timeline local interaction state
  localTimeline: any[] = [];          // mutable copy of timeline items
  completedItems = new Set<number>(); // indices of checked-off items
  importanceFilter: 'All' | 'High' | 'Medium' | 'Low' = 'All';
  timelineSearch = '';
  showAddForm = false;
  editingIndex: number | null = null;
  editDraft: any = {};
  newActivity = { time: '', activity: '', duration: '', importance: 'Medium' };

  ngOnInit() {
    // Pre-fetch recommendations in the background when studio opens
    this.loadRecommendations();
    this.loadEventTypes();
  }

  loadEventTypes() {
    this.eventTypeService.getAll().subscribe({
      next: (types) => {
        this.eventTypes = types;
      },
      error: (err) => console.error("Error loading event types:", err)
    });
  }

  switchTab(tab: 'package' | 'budget' | 'timeline') {
    this.activeTab = tab;
    if (tab === 'budget') {
      if (!this.selectedBudget) {
        this.selectedBudget = this.eventBudget;
      }
      if (!this.selectedEventType) {
        this.selectedEventType = this.eventTypeName;
      }
      this.loadBudgetAllocation();
    }
    // Timeline is loaded only on explicit "Generate Timeline" button click
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
      next: (res: any) => {
        this.loadingRecommendations = false;
        const raw = res?.value ?? res?.Value ?? res;
        const recList = raw?.recommendations ?? raw?.Recommendations ?? raw ?? [];
        this.recommendations = recList.map((rec: any) => ({
          ServiceId: rec.ServiceId ?? rec.serviceId ?? '',
          ServiceName: rec.ServiceName ?? rec.serviceName ?? '',
          VendorName: rec.VendorName ?? rec.vendorName ?? '',
          Reasoning: rec.Reasoning ?? rec.reasoning ?? ''
        }));
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

    if (!this.selectedBudget) {
      this.selectedBudget = this.eventBudget;
    }
    if (!this.selectedEventType) {
      this.selectedEventType = this.eventTypeName;
    }

    this.aiService.getBudgetAllocation(this.selectedBudget, this.selectedEventType).subscribe({
      next: (res: any) => {
        this.loadingBudget = false;
        const raw = res?.value ?? res;
        if (raw) {
          this.budgetAllocation = {
            totalBudget: raw.totalBudget ?? 0,
            eventType: raw.eventType ?? '',
            advice: raw.advice ?? '',
            categories: (raw.categories ?? []).map((cat: any) => ({
              name: cat.name ?? '',
              amount: cat.amount ?? 0,
              percentage: cat.percentage ?? 0,
              description: cat.description ?? ''
            }))
          };
        }
      },
      error: (err) => {
        this.loadingBudget = false;
        this.budgetError = "Failed to retrieve smart budget allocation.";
        console.error("Error loading budget allocation:", err);
      }
    });
  }

  recalculateBudget() {
    if (this.selectedBudget <= 0) {
      this.toastService.show('Please enter a budget greater than 0.', 'error');
      return;
    }
    this.loadBudgetAllocation();
  }

  saveBudgetToEvent() {
    if (this.selectedBudget <= 0) {
      this.toastService.show('Please enter a budget greater than 0.', 'error');
      return;
    }
    this.isSavingBudget = true;

    this.eventService.getById(this.eventId).subscribe({
      next: (res: any) => {
        const raw = res?.value ?? res;
        if (!raw) {
          this.isSavingBudget = false;
          this.toastService.show('Failed to retrieve current event details.', 'error');
          return;
        }

        const oldType = this.eventTypes.find(t => t.name === raw.eventTypeName);
        const oldTypeId = oldType ? oldType.id : '';

        const matchingType = this.eventTypes.find(t => t.name === this.selectedEventType);
        const eventTypeId = matchingType ? matchingType.id : oldTypeId;

        const payload: any = {
          title: raw.title,
          eventTypeId: eventTypeId,
          eventDate: raw.eventDate,
          location: raw.location,
          totalBudget: this.selectedBudget,
          guestCount: raw.guestCount ?? 0,
          notes: raw.notes,
          eventStatus: raw.eventStatus ?? 'Planned'
        };

        this.eventService.update(this.eventId, payload).subscribe({
          next: () => {
            this.isSavingBudget = false;
            this.eventBudget = this.selectedBudget;
            this.eventTypeName = this.selectedEventType;
            this.toastService.show('Budget allocation successfully applied and saved!', 'success');
            this.eventUpdated.emit();
          },
          error: (err) => {
            this.isSavingBudget = false;
            console.error('Failed to update event budget:', err);
            this.toastService.show('Failed to save budget to event.', 'error');
          }
        });
      },
      error: (err) => {
        this.isSavingBudget = false;
        console.error('Failed to get event details for update:', err);
        this.toastService.show('Failed to fetch event details.', 'error');
      }
    });
  }

  getDonutSegments() {
    if (!this.budgetAllocation || !this.budgetAllocation.categories.length) return [];
    
    let accumulatedPercentage = 0;
    const circumference = 251.3; // 2 * PI * r (r=40)
    
    return this.budgetAllocation.categories.map((cat: any, index: number) => {
      const percentage = cat.percentage;
      const strokeLength = (percentage / 100) * circumference;
      const strokeOffset = circumference - (accumulatedPercentage / 100) * circumference;
      accumulatedPercentage += percentage;
      
      const colors = [
        '#c9a84c', // Gold
        '#1a2540', // Navy
        '#64748b', // Slate
        '#0ea5e9', // Blue
        '#10b981', // Emerald
        '#f59e0b', // Amber
        '#ec4899'  // Pink
      ];
      const color = colors[index % colors.length];
      
      return {
        category: cat.name,
        percentage: percentage,
        color: color,
        strokeDashArray: `${strokeLength} ${circumference - strokeLength}`,
        strokeDashOffset: strokeOffset
      };
    });
  }

  getCategoryColor(index: number): string {
    const colors = [
      '#c9a84c', // Gold
      '#1a2540', // Navy
      '#64748b', // Slate
      '#0ea5e9', // Blue
      '#10b981', // Emerald
      '#f59e0b', // Amber
      '#ec4899'  // Pink
    ];
    return colors[index % colors.length];
  }

  loadTimeline() {
    this.loadingTimeline = true;
    this.timelineError = null;
    // Reset all local interaction state on each generation
    this.localTimeline = [];
    this.completedItems = new Set();
    this.importanceFilter = 'All';
    this.timelineSearch = '';
    this.showAddForm = false;
    this.editingIndex = null;

    this.aiService.getEventTimeline(this.eventId).subscribe({
      next: (res: any) => {
        this.loadingTimeline = false;
        const raw = res?.value ?? res;
        if (raw) {
          this.timelineData = {
            eventId: raw.eventId ?? '',
            eventTitle: raw.eventTitle ?? '',
            planningNotes: raw.planningNotes ?? '',
            timeline: (raw.timeline ?? []).map((item: any) => ({
              time: item.time ?? '',
              activity: item.activity ?? '',
              duration: item.duration ?? '',
              importance: item.importance ?? 'Low'
            }))
          };
          // Seed local mutable copy
          this.localTimeline = this.timelineData.timeline.map(i => ({ ...i }));
        }
      },
      error: (err) => {
        this.loadingTimeline = false;
        this.timelineError = "Failed to generate AI day-of timeline.";
        console.error("Error loading timeline:", err);
      }
    });
  }

  getFilteredTimeline(): any[] {
    let items = this.localTimeline;
    if (this.importanceFilter !== 'All') {
      items = items.filter(i => i.importance === this.importanceFilter);
    }
    const q = this.timelineSearch.trim().toLowerCase();
    if (q) {
      items = items.filter(i =>
        i.activity.toLowerCase().includes(q) ||
        i.time.toLowerCase().includes(q)
      );
    }
    return items;
  }

  getOriginalIndex(item: any): number {
    return this.localTimeline.indexOf(item);
  }

  toggleItemCompleted(originalIndex: number) {
    if (this.completedItems.has(originalIndex)) {
      this.completedItems.delete(originalIndex);
    } else {
      this.completedItems.add(originalIndex);
    }
  }

  isCompleted(originalIndex: number): boolean {
    return this.completedItems.has(originalIndex);
  }

  addCustomActivity() {
    if (!this.newActivity.time.trim() || !this.newActivity.activity.trim()) {
      this.toastService.show('Time and Activity name are required.', 'error');
      return;
    }
    this.localTimeline.push({ ...this.newActivity });
    this.newActivity = { time: '', activity: '', duration: '', importance: 'Medium' };
    this.showAddForm = false;
  }

  deleteTimelineItem(originalIndex: number) {
    this.localTimeline.splice(originalIndex, 1);
    // Re-build completed set since indices shift
    const newCompleted = new Set<number>();
    this.completedItems.forEach(idx => {
      if (idx < originalIndex) newCompleted.add(idx);
      else if (idx > originalIndex) newCompleted.add(idx - 1);
    });
    this.completedItems = newCompleted;
    if (this.editingIndex === originalIndex) this.editingIndex = null;
  }

  startEditingItem(originalIndex: number) {
    this.editingIndex = originalIndex;
    this.editDraft = { ...this.localTimeline[originalIndex] };
  }

  saveEditedItem() {
    if (this.editingIndex === null) return;
    if (!this.editDraft.time?.trim() || !this.editDraft.activity?.trim()) {
      this.toastService.show('Time and Activity name are required.', 'error');
      return;
    }
    this.localTimeline[this.editingIndex] = { ...this.editDraft };
    this.editingIndex = null;
  }

  cancelEditing() {
    this.editingIndex = null;
  }

  printTimeline() {
    window.print();
  }

  completedCount(): number {
    return this.completedItems.size;
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

  openServiceInExplore(serviceId?: string) {
    if (!serviceId) {
      this.toastService.show('Service details not available.', 'info');
      return;
    }
    window.open(`/explore-services?openServiceId=${serviceId}`, '_blank');
  }

  openCategoryInExplore(categoryName: string) {
    if (!categoryName) return;
    let cat = categoryName;
    if (cat.toLowerCase() === 'decor') {
      cat = 'Decoration';
    }
    window.open(`/explore-services?serviceCategory=${cat}`, '_blank');
  }
}
