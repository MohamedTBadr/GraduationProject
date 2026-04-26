# Comprehensive Frontend Integration Plan

This document outlines the strategy for completing the frontend integration with the .NET backend for the Graduation Project. It has been updated to reflect the current state of progress.

## User Review Required

> [!WARNING]
> Please review the "Open Questions" section carefully. We need clarification on the Event Creation flow (how users book services) because the `CreateEvent` interface and endpoint seem missing from the frontend API definitions.

## Progress Summary So Far

Based on a thorough system audit:

### ✅ Completed & Integrated
- **Shared Infrastructure**: Global Error Handler and HTTP Interceptors (Auth/Error) are active. API Interfaces are largely defined.
- **Admin Portal**: Categories & Event Types management is fully functional and responsive, integrated with real endpoints.
- **Vendor Portal**:
  - Profile updates and loading.
  - Services (Products) CRUD operations and Active/Paused logic.
  - Bookings List logic (approving/rejecting requests via `EventItem` status).
  - Dashboard Statistics (aggregated via `getByUser`).
- **Public Portal**: Home page dynamically loads and filters top-rated active vendors.

### ⏳ Pending Implementation
- **Admin Portal**: 
  - `vendors.component`: Needs to list vendors and implement the Approve/Reject flow.
  - `users.component`: Needs paginated list and management of users.
- **User Portal**:
  - `dashboard`, `my-bookings`, `my-events`, `favorites` components exist but are mostly static/mock data and need connection to `EventService`.
- **Public Portal**:
  - `Add Event` flow: The complete checkout/booking flow where a user selects vendor services and creates an event. (User must login first)
  - Search/Explore pages: Connecting the search queries to backend vendor/product filtering.

---

## Proposed Execution Plan

I will tackle the remaining work in the following prioritized phases:

### Phase 1: Complete Admin Portal (Users & Vendors)
#### [MODIFY] `features/admin/vendors/vendors.component.ts`
- Implement `VendorService.getAll()` to list vendors in a datatable.
- Add "Approve" functionality triggering `PATCH /vendor/{vendorId}/approve`.
#### [MODIFY] `features/admin/users/users.component.ts`
- Implement `UserService.getAll(pagination)` with dynamic tables.

### Phase 2: Complete User Portal (My Bookings & Events)
#### [MODIFY] `features/user/my-bookings/my-bookings.component.ts`
- Replace mock arrays with real data via `EventService.getByUser()`.
- Group events dynamically and display them by status tabs.
#### [MODIFY] `features/user/dashboard/dashboard.component.ts`
- Create aggregate statistics (Total Spent, Total Events, Pending Requests).

### Phase 3: Public Portal & Event Creation (The Booking Engine)
#### [MODIFY] `features/user/add-event/add-event.component.ts` (or Public Booking Flow)
- *Pending clarification from Open Questions.* Will assemble the cart of selected Vendor Services and submit the master Event payload to the backend.
#### [MODIFY] `features/public/explore/explore.component.ts`
- Hook up search inputs and sidebar filters (categories/event types) to the backend API.

### Phase 4: Advanced Integrations
- **AI Integration**: Implement `GeminiService` for AI-generated event suggestions.
- **Payments**: Integrate Paymob for checkout processing.
- **Real-Time**: Connect WebSockets for chat and Server-Sent Events (SSE) for Notifications.

---

## Open Questions

> [!IMPORTANT]
> 1. **File Uploads**: The Vendor Services form has an `ImageUploadComponent`. Does the backend have a dedicated `POST /upload` endpoint for images, or should images be sent as Base64 strings?
> 2. **Order of Execution**: Are you okay with me starting with **Phase 1: Complete Admin Portal** next, or would you prefer I jump into the **User Portal** first?

## Verification Plan

### Automated/Manual Testing
- Log in as Admin to verify vendor approval flow correctly changes vendor status without page refreshes.
- Log in as Vendor to verify new active status appearance.
- Log in as User to verify that `my-bookings` accurately reflects backend event items.
- Run UI cross-browser consistency checks on all newly built tabular layouts.
