# Comprehensive Frontend Integration Plan

This document outlines the strategy for completing the frontend integration with the .NET backend for the Graduation Project. It has been updated to reflect the current state of progress.

## User Review Required

> [!WARNING]
> Please review the "Open Questions" section carefully. We need clarification on the Event Creation flow (how users book services) because the `CreateEvent` interface and endpoint seem missing from the frontend API definitions.

## Progress Summary So Far

Based on a thorough system audit:

### ✅ Completed & Integrated
- **Shared Infrastructure**: Global Error Handler and HTTP Interceptors (Auth/Error) are active. API Interfaces are largely defined.
- **Admin Portal**: 
  - Categories & Event Types management is fully functional and responsive, integrated with real endpoints.
  - User Management (`users.component`) is fully functional with paginated list and Suspend/Unsuspend flow.
  - Vendor Management (`vendors.component`) is fully functional with Approve/Reject/Suspend flows.
- **Vendor Portal**:
  - Profile updates and loading.
  - Services (Products) CRUD operations and Active/Paused logic.
  - Bookings List logic (approving/rejecting requests via `EventItem` status).
  - Dashboard Statistics (aggregated via `getByUser`).
- **User Portal**:
  - `my-bookings` is integrated with `EventService.getByUser`, displaying dynamic data and Post-Service Completion actions (Done, Completed, Review).
- **Public Portal**: 
  - Home page dynamically loads and filters top-rated active vendors.
  - Search/Explore pages are implemented with taxonomy filtering.
- **Post-Service Completion Flow**:
  - Complete backend & frontend lifecycle: `Done` -> `Completed` -> `Review` rating submissions.

### ⏳ Pending Implementation
- **User Portal**:
  - `dashboard`, `my-events`, `favorites` need finalized connection to `EventService`.
- **Public Portal**:
  - `Add Event` flow: The complete checkout/booking flow where a user selects vendor services and creates an event. (User must login first)

---

## Proposed Execution Plan

I will tackle the remaining work in the following prioritized phases:

### Phase 1: Complete User Portal (Dashboard & Events)
#### [MODIFY] `features/user/dashboard/dashboard.component.ts`
- Create aggregate statistics (Total Spent, Total Events, Pending Requests).
#### [MODIFY] `features/user/my-events/my-events.component.ts`
- Connect event checklist and details to `EventService`.

### Phase 2: Public Portal & Event Creation (The Booking Engine)
#### [MODIFY] `features/user/add-event/add-event.component.ts` (or Public Booking Flow)
- *Pending clarification from Open Questions.* Will assemble the cart of selected Vendor Services and submit the master Event payload to the backend.

### Phase 3: Advanced Integrations
- **AI Integration**: Implement `GeminiService` for AI-generated event suggestions.
- **Payments**: Integrate Paymob for checkout processing.
- **Real-Time**: Connect WebSockets for chat and Server-Sent Events (SSE) for Notifications.

---

## Open Questions / Backend Status

> [!IMPORTANT]
> 1. **File Uploads [RESOLVED]**: The backend has a dedicated `POST /api/files/upload` endpoint (in `FileController.cs`) that uploads directly to AWS S3 and returns `{ key, url }`. 
> 2. **Post-Service Backend APIs [RESOLVED]**: The backend now supports transitioning EventItems to `Done` and `Completed`, along with Review submission and User Suspension endpoints.
> 3. **Event Creation Flow [ACTION REQUIRED]**: The complete checkout/booking flow where a user selects vendor services and creates an event still needs clarification. How should the frontend submit multiple services to a single Event in the current API schema?

## Verification Plan

### Automated/Manual Testing
- Log in as Admin to verify vendor approval flow correctly changes vendor status without page refreshes.
- Log in as Vendor to verify new active status appearance.
- Log in as User to verify that `my-bookings` accurately reflects backend event items.
- Run UI cross-browser consistency checks on all newly built tabular layouts.
