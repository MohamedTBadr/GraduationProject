# Project Description: Event Marketplace Platform

This document provides a comprehensive overview of the Graduation Project, detailing all features, architectural components, business logic, and infrastructure.

## 1. Project Overview

The project is a comprehensive event marketplace platform connecting Clients (Users) with Vendors offering specific event services. The platform facilitates discovering services, booking them for events, managing payments, and reviewing completed services. 

The system operates across three distinct user roles governed by JWT-based authentication:
- **Clients (Users):** Explore services, create events, book vendors, manage payments, and leave reviews.
- **Vendors:** Offer services (products), manage incoming booking requests, and fulfill event items.
- **Admins:** Manage platform taxonomy, moderate users and vendors, and handle support escalations.

## 2. Infrastructure & Tech Stack

### Core Technologies
- **Backend API:** ASP.NET Core Web API 
- **Frontend App:** Component-based UI framework (Angular, based on existing `.component.ts` structures), featuring robust error handling and HTTP interceptors.
- **Database:** Relational database managed via Entity Framework Core (implied by ASP.NET Core ecosystem).

### Third-Party Integrations
- **AWS S3:** Used for secure, scalable image hosting. Vendor service images are uploaded via the platform's `FileController` and directly streamed to S3.
- **Paymob:** Payment gateway integration to handle secure checkouts via an embedded iFrame. Webhooks sync transaction statuses (`Paid` / `Failed`) back to the platform.
- **Gemini AI (Planned):** For an "AI-Powered Event Studio" to provide intelligent vendor and service recommendations based on user-described events.
- **Real-Time Communication (Planned):** WebSockets for live chat between clients and vendors, and Server-Sent Events (SSE) for system notifications.

## 3. Core Features & Business Logic

### A. Taxonomy & Recommendation Engine
The platform relies on a three-dimensional taxonomy to match supply and demand efficiently:
1. **Vendor Type:** Primary classification (e.g., Photographer, Venue).
2. **Service Type:** Specific offerings dependent on Vendor Type (e.g., Wedding Photography).
3. **Event Types:** The occasions vendors serve (e.g., Weddings, Corporate Events).

The matching logic prioritizes vendors based on explicit need (Service Type) and relevance (Event Type).

### B. Vendor Lifecycle
1. **Registration & Approval:** Vendors register but remain invisible to the public (`isApproved: false`). An **Admin** must review and approve them to appear on public explore pages.
2. **Service Creation:** Vendors create specific offerings. Image uploads stream to S3, returning a URL stored with the service details. Services can be paused or set active.
3. **Booking Moderation:** Vendors receive `Pending` booking requests (Event Items) on their dashboard. They have the autonomy to `Approve` or `Reject` these requests.

### C. Client Booking Flow (Direct-to-Event)
Unlike traditional e-commerce, the platform uses a direct-to-event booking model:
1. **Explore:** Users browse approved vendors and active services.
2. **Add to Event:** When booking a service, if the user has no active events, a new "Untitled Event" is created. If they have one, it's added automatically. If multiple, an inline dropdown lets them choose the destination event.
3. **Event Finalization:** Users specify event details (Guests, Date, Location) to finalize the master Event.

### D. Payment Processing
1. **Checkout:** An order is dynamically calculated based on the `Approved` Event Items.
2. **Paymob iFrame:** The frontend requests a payment session and presents the Paymob iFrame.
3. **Webhooks:** Paymob notifies the backend upon success, updating the Order status to `Paid`. Points are awarded via the Loyalty Program upon payment.

### E. Post-Service Lifecycle
To ensure quality, services follow a strict lifecycle after being booked and approved:
1. **Done:** The Vendor marks the service as `Done` once delivered.
2. **Completed:** The Client marks the service as `Completed` to acknowledge delivery.
3. **Review:** Once `Completed`, the Client can submit a rating and review, affecting the vendor's profile score.

### F. Loyalty & Rewards Program
- Clients earn **1 point for every 10 EGP spent** on successful bookings.
- Points are credited when an order is marked `Paid` or `Completed`.
- **Future:** Redeeming points for discounts on subsequent bookings.

### G. Moderation & Support
- **Account Suspension:** Admins can suspend Clients or Vendors for policy violations, instantly revoking platform access and triggering notification emails.
- **Support Tickets:** A robust ticketing system categorized by `Technical`, `Booking`, `Payment`, or `General`. Issues have priorities (`Low` to `Critical`) and can be escalated to `Management` or `Legal`.

## 4. Platform Structure

The project is structured into modular portals to ensure clean separation of concerns:

- **Public Portal:** Landing page featuring top-rated vendors, Search/Explore pages with taxonomy filtering.
- **Client (User) Portal:** Dashboard (stats, pending requests), My Events (event checklist), My Bookings (post-service actions), Favorites.
- **Vendor Portal:** Dashboard statistics, Service/Product management, Profile updates, Booking Requests management.
- **Admin Portal:** Taxonomy management (Categories, Event Types), User/Vendor lists, Suspension/Approval workflows, Support ticket triage.

## 5. Ongoing & Future Development
- **AI-Powered Event Studio:** Implementing `GeminiService` to create automated event checklists and recommendations.
- **Discount Redemptions:** Completing the flow for utilizing loyalty points.
- **Real-Time Features:** Rolling out Chat and Notifications via WebSockets and SSE.
