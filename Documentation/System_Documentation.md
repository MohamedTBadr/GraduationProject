# Eventora (EpicHub) System Documentation

## 1. Overview

Eventora (internally referred to as EpicHub) is an advanced full-stack event marketplace and planning platform that connects clients with vendors who provide event-related services. The platform offers a seamless, end-to-end user experience encompassing discovery, AI-assisted planning, collaborative event management, secure bookings, real-time communication, and payments.

The architecture is split into a modern Single-Page Application (SPA) utilizing Angular 18 with Server-Side Rendering (SSR) and a robust backend built on ASP.NET Core 9 Web API using Clean Architecture principles.

The platform is designed around three main user groups:
- **Clients (Users):** Browse vendors and services, utilize AI to plan events and budgets, collaborate with peers, book services, process payments via Paymob, chat with vendors in real-time, and leave reviews.
- **Vendors:** Access dedicated dashboards to manage their profiles, portfolios, services, and packages. They can track earnings, respond to client inquiries, approve bookings, and view comprehensive reporting data.
- **Admins:** Oversee the entire ecosystem through an admin portal. They manage user suspensions, approve vendor applications, define platform taxonomy (service types, event types), handle support tickets, and analyze platform telemetry and executive reports.

## 2. Project Structure

The project is organized into two main codebases within a monolithic repository to ease development while maintaining clear boundary separation.

```text
Eventora_Workspace/
|-- API/                       (Backend - ASP.NET Core 9)
|   |-- src/Application/       Business services, DTOs, interfaces, AI logic, caching behaviors
|   |-- src/Domain/            Entities, enums, domain contracts, value objects
|   |-- src/Infrastructure/    EF Core persistence, Lucene search, Paymob, AWS S3, Hangfire jobs
|   |-- src/Web.Api/           API Controllers, Middleware, SignalR hubs, application entry point
|   |-- src/Shared/            Shared result types, pagination wrappers, custom exceptions
|   |-- UnitTests/             Unit tests for Application services and Web.Api
|   |-- IntegrationTests/      Repository and persistence tests using SQLite
|
|-- Front-End/                 (Frontend - Angular 18)
|   |-- src/app/core/          Guards (auth, role), interceptors, singleton services (Auth, API)
|   |-- src/app/features/      Domain modules (admin, auth, public, user, vendor dashboards)
|   |-- src/app/layouts/       Structural containers (AdminLayout, VendorLayout, UserLayout)
|   |-- src/app/shared/        Reusable UI components, modals, comparison tools
|   |-- server.ts              Angular Universal (SSR) configuration
|
|-- Documentation/             System documentation, requirements, and architecture diagrams
```

This structure strictly enforces Clean Architecture on the backend (keeping domain models isolated from web concerns) and feature-based modularity on the frontend.

## 3. Technology Stack

| Area | Technology |
| --- | --- |
| **Frontend Framework** | Angular 18 (Standalone Components), Server-Side Rendering (SSR) |
| **Frontend Styling** | TailwindCSS, FontAwesome, SCSS |
| **Maps & Routing** | Leaflet (interactive maps), Angular Router |
| **Backend Framework** | ASP.NET Core 9 Web API, C# |
| **Authentication** | ASP.NET Core Identity, JWT Bearer tokens, Angular Route Guards |
| **Database & ORM** | SQL Server, Entity Framework Core |
| **Caching** | Redis, .NET HybridCache, in-memory caching |
| **Search Engine** | Lucene.NET |
| **Background Jobs** | Hangfire with SQL Server persistence |
| **Real-Time Communication** | SignalR (with Redis scale-out), Server-Sent Events (SSE), RxJS |
| **Payments Integration** | Paymob checkout and Webhook processing |
| **File Storage** | AWS S3 (Infrastructure abstraction) |
| **AI Integrations** | Groq (Llama 3.3), Gemini APIs |
| **Observability** | OpenTelemetry, Aspire Dashboard, Serilog |
| **Resilience** | Polly (retries, timeouts, circuit breakers), Angular HttpInterceptors |
| **DevOps & Containers** | Docker, Docker Compose (API, SQL Server, Redis, Aspire) |
| **Testing** | xUnit, Moq, EF Core SQLite (Backend), Jasmine, Karma (Frontend) |

## 4. Main Business Modules

### 4.1 Authentication and Role Management
**Purpose:** Secure access and identity verification for all user types.
- **Backend:** Handles ASP.NET Identity (Login, Registration, Refresh Tokens, Password Reset) and JWT generation.
- **Frontend:** `auth.service.ts` manages tokens using `localStorage`. Angular route guards (`authGuard`, `roleGuard`) restrict access to `/admin`, `/vendor-dashboard`, and `/user` routes.

### 4.2 Admin Portal & Operations
**Purpose:** Centralized operational control.
- **Backend:** `DashboardController`, `UserController`, `VendorController` (admin actions), `SupportTicketsController`.
- **Frontend:** `/admin` features a dashboard for user management, vendor approvals, content moderation, and taxonomy management (Event Types, Service Types).

### 4.3 Vendor Dashboard
**Purpose:** Empowerment of service providers.
- **Backend:** Tracks vendor entities, services, packages, reviews, and bookings. AI endpoints generate "vibe summaries" based on vendor portfolios.
- **Frontend:** Detailed dashboard (`/vendor-dashboard`) allowing vendors to upload portfolio items, manage active services, handle incoming messages, and view earnings and analytics.

### 4.4 Event Planning & AI Assistance
**Purpose:** The core client-facing workflow.
- **Backend:** `EventController` handles event creation, collaborators, and tracking event items (booked services). The `AIController` allocates budgets, suggests timelines, and recommends services using Llama/Gemini.
- **Frontend:** `/add-event` wizard and `/user/my-events` dashboard. Integrates AI recommendations natively into the event planning UX.

### 4.5 Service Discovery & Search
**Purpose:** Connecting clients with the right vendors.
- **Backend:** Lucene.NET powers fast, fuzzy search across vendors and services. Hangfire jobs sync SQL Server data to the Lucene index daily.
- **Frontend:** Public-facing pages with complex filtering (by taxonomy, price, area), integrating `Leaflet` maps for location-based discovery.

### 4.6 Orders and Payments
**Purpose:** Financial transactions and booking finalization.
- **Backend:** `OrderController` creates orders from approved event items. `PaymobController` handles checkout session creation and securely processes Paymob webhooks with idempotency.
- **Frontend:** Seamless checkout flow. The `payment.service.ts` coordinates intent creation and redirects to Paymob's secure UI.

### 4.7 Real-Time Chat & Notifications
**Purpose:** Instant communication between clients and vendors.
- **Backend:** `ChatHub` (SignalR) manages bi-directional messaging. `NotificationController` pushes alerts via SSE. Redis backplane ensures messages sync across API instances.
- **Frontend:** `signalr.service.ts`, `chat.service.ts`, and `notification.service.ts` maintain persistent WebSocket/SSE connections, updating UI state reactively using RxJS subjects.

### 4.8 Reporting & Analytics
**Purpose:** Business intelligence for vendors and admins.
- **Backend:** Hangfire runs `monthly-vendor-reports` and `monthly-admin-report` jobs to generate PDFs and email them automatically.
- **Frontend:** Admin and Vendor Analytics pages render charts and statistics fetched from reporting endpoints.

## 5. API / Frontend / Backend Summary

### Key Backend Controllers
| Controller | Main Responsibility |
| --- | --- |
| `AuthenticationController` | JWT generation, password resets, token refreshing. |
| `VendorController` | Profiles, approval workflows, ratings, bookings. |
| `EventController` | Event management, collaboration, tracking service items. |
| `OrderController` & `PaymobController` | Order state, payment intents, secure webhook handling. |
| `AIController` | AI budget allocation, timeline generation, recommendations. |
| `ChatController` & `Hub/chatHub` | Real-time messaging history and active WebSocket channels. |

### Key Frontend Services
| Service | Main Responsibility |
| --- | --- |
| `auth.service.ts` | Token lifecycle, current user state (BehaviorSubject), logout logic. |
| `signalr.service.ts` | Manages connection lifecycle and reconnection logic for WebSockets. |
| `ai.service.ts` | Interfaces with backend AI endpoints for dynamic UI suggestions. |
| `event.service.ts` | State management for user event creation and editing. |

## 6. Runtime Architecture

```mermaid
flowchart TD
    subgraph Frontend [Angular 18 SPA]
        UI[Components & Layouts]
        Services[RxJS Services & Interceptors]
        Guards[Auth/Role Guards]
        UI --> Services
        UI --> Guards
    end

    subgraph Backend [ASP.NET Core Web API]
        Controllers[API Controllers]
        Hubs[SignalR Hubs]
        AppLayer[Application Layer (CQRS/Services)]
        Domain[Domain Models & Interfaces]
        Infra[Infrastructure (EF Core, Lucene)]
        
        Controllers --> AppLayer
        Hubs --> AppLayer
        AppLayer --> Domain
        AppLayer --> Infra
    end

    subgraph External & Infra [Infrastructure Services]
        SQL[(SQL Server)]
        Redis[(Redis Cache & Backplane)]
        Hangfire[Hangfire Background Jobs]
        S3[AWS S3 Storage]
        AI[Groq / Gemini AI APIs]
        Paymob[Paymob Payment Gateway]
        Telemetry[Aspire Dashboard / OTLP]
    end

    Services -- HTTP/REST --> Controllers
    Services -- WebSockets --> Hubs
    Services -- SSE --> Controllers

    Infra --> SQL
    Infra --> Redis
    Infra --> S3
    Infra --> AI
    Infra --> Paymob
    Controllers -.-> Telemetry
    Hangfire --> SQL
```

## 7. Cross-Cutting Concerns

The platform relies on powerful cross-cutting middleware and interceptors:
- **Global Exception Handling:** Custom ASP.NET middleware catches exceptions and maps them to standard HTTP status codes and JSON formats.
- **Idempotency:** `IdempotencyCustomMiddleware` protects critical write operations (like order creation and payment hooks) from duplicate execution.
- **Observability:** OpenTelemetry metrics and Serilog rolling file logs are exported to the Aspire Dashboard.
- **Frontend Interceptors:** Angular HTTP interceptors automatically append JWT tokens to requests and globally catch 401/403 errors to trigger token refresh flows or redirects.
- **Response Compression:** Brotli/Gzip is enabled on the backend to minimize payload sizes for JSON and static assets.
- **Rate Limiting:** Fixed-window rate limiting is configured in `Program.cs` to protect public endpoints.

## 8. Data Model Highlights

Core entities mapping the business domain:
- **Identity & Profiles:** `ApplicationUser`, `Vendor`, `VendorType`.
- **Marketplace:** `Service`, `ServiceType`, `ServiceImage`, `ServiceRating`.
- **Planning:** `Event`, `EventType`, `EventItem` (represents a booked service in an event), `EventCollaborator`.
- **Commerce:** `Order`, `Voucher`.
- **Communication:** `Conversation`, `Message`, `Notification`, `SupportTicket`.
- **Analytics:** `ReportRecord`, `ScheduledReport`.

## 9. Real-Time Features

Real-time capabilities differentiate Eventora from simple CRUD apps:
- **SignalR (WebSockets):** Powers the chat module. Redis is used as a backplane to allow horizontal scaling of the API.
- **Server-Sent Events (SSE):** Used for lightweight, one-way notification streaming (e.g., "Your booking was approved").
- **Frontend RxJS:** Services maintain open subscriptions to these streams, allowing notification badges and chat windows to update instantly without page reloads.

## 10. Background Jobs

**Hangfire** is heavily utilized to prevent blocking the request-response thread:
- **`lucene-daily-sync`**: Synchronizes SQL Server product/vendor data into the Lucene search index daily.
- **`monthly-vendor-reports`**: Aggregates booking data and emails PDF reports to vendors.
- **`monthly-admin-report`**: Sends executive summaries to platform owners.
- Background execution of external side-effects (like specific email triggers).

## 11. Deployment & Infrastructure

The project uses **Docker Compose** for immediate local replication of production infrastructure:
- **Containers:** `Web.Api`, `SQL Server 2022`, `Redis`, `Aspire Dashboard`.
- **Ports:** API (`5000`), SQL (`1433`), Redis (`6379`), Aspire (`18888`), OTLP (`18889`).
- **Frontend:** Can be deployed via Node.js (for Angular SSR) or compiled to static files for CDN hosting.
- **Environment:** Secrets and configurations (API keys for Groq, Paymob, JWT, AWS) are injected via `appsettings.json` or `.env`.

## 12. Testing

The application is built with testability in mind:
- **Backend Unit Tests (`UnitTests/`):** Utilizes `xUnit` and `Moq` to isolate Application services, caching helpers, and Result wrappers.
- **Backend Integration Tests (`IntegrationTests/`):** Validates Entity Framework Core behavior and repository logic using an in-memory SQLite provider.
- **Frontend Tests:** Configured with `Jasmine` and `Karma` for component and service testing.

## 13. Advantages

1. **True Full-Stack Cohesion:** The Angular frontend strictly mirrors the Clean Architecture domain models of the backend.
2. **AI Differentiation:** Going beyond standard booking, the integration of Groq/Llama for budget allocation and timeline generation provides unique user value.
3. **Enterprise Reliability:** Idempotency, Hangfire background processing, and Polly resilience handlers ensure the app won't break under network failures or duplicate clicks.
4. **Scale-Ready:** Distributed caching (HybridCache/Redis), SignalR backplanes, and separated search indexes (Lucene) prove the system is designed for high traffic.
5. **Observability Out-of-the-Box:** OpenTelemetry and Aspire provide immediate insights into bottlenecks.

## 14. Pros (Technical Strengths)

- Uses modern .NET 9 features and Nullable Reference Types.
- Uses Angular 18 Standalone components, dropping `NgModule` overhead.
- Strong external API isolation (Paymob, AWS, Groq) via Infrastructure abstractions.
- JWT stateless authentication ensures easy API scaling.
- Rich domain model properly handles edge cases (like multi-user collaboration on a single event).

## 15. Cons and Limitations

- **Secret Management:** Currently, many secrets exist in configuration files. These must be migrated to Azure Key Vault, AWS Secrets Manager, or injected safely via CI/CD before production.
- **Missing Swagger/OpenAPI:** While the backend is robust, generating Swagger would drastically improve frontend-backend integration speed.
- **Search Index Drift:** Lucene is fast, but real-time updates (like a vendor instantly updating a price) might take time to reflect unless Hangfire syncs are triggered manually on updates.
- **SSR Complexity:** Angular SSR requires Node.js hosting rather than a simple S3/CDN static host, increasing deployment complexity.

## 16. Recommended Improvements

### High Priority
- Move all environment variables (API keys, connection strings) to a secure `.env` / secret manager.
- Re-enable and configure Swagger/OpenAPI for the backend.
- Enforce the registered `UseRateLimiter()` in the middleware pipeline explicitly.

### Medium Priority
- Trigger partial Lucene updates immediately when a vendor updates a profile, rather than waiting for daily Hangfire syncs.
- Implement comprehensive frontend E2E tests using Cypress or Playwright.
- Centralize logging outputs to a cloud provider (e.g., DataDog, ELK) instead of local text files inside Docker.

### Long-Term Improvements
- Introduce CQRS (Command Query Responsibility Segregation) using MediatR for the most complex flows (like Order Processing).
- Introduce Micro-frontends if the Admin and Vendor dashboards become too large for the main Angular bundle.

## 17. Suggested Demo Flow

1. **Client Registration:** Demonstrate JWT creation and Angular auth guards.
2. **AI Planning:** Use the `/add-event` flow to generate an AI timeline and budget.
3. **Discovery:** Browse vendors utilizing the Lucene-powered search and Leaflet maps.
4. **Collaboration:** Invite another user to view the event via the Event Collaborators feature.
5. **Booking & Payment:** Send a booking request, log in as a Vendor to approve it, then complete the Paymob checkout flow as the Client.
6. **Real-Time:** Open two browser windows to demonstrate SignalR chat and SSE notifications.
7. **Admin Oversight:** Log in to the Admin Portal to view telemetry, generated PDF reports, and approve a support ticket.

## 18. Thesis Support

For graduation thesis integration, we recommend the following mappings:

- **System Architecture Chapter:** Utilize the Mermaid diagram in section 6. Explain Clean Architecture principles based on the Backend structure, and SPA concepts based on the Angular structure.
- **Implementation Highlights Chapter:** Dedicate subsections to:
  - *Real-Time Communication:* Explain the SignalR/Redis and RxJS implementation.
  - *AI Integration:* Explain how external LLMs influence the local domain (Budgeting algorithm).
  - *Reliability Patterns:* Highlight the Idempotency Middleware and Hangfire.
- **Suggested Figures:**
  - Sequence diagram of the Paymob webhook handling (showing idempotency checks).
  - Flowchart of the AI event generation process.
  - Screenshots of the Angular Vendor Analytics dashboard.
- **Testing Chapter:** Summarize the `xUnit`/`Moq` setup and how SQLite integration tests ensure database integrity.

## 19. Summary

Eventora (EpicHub) is an exceptionally comprehensive full-stack platform that exceeds typical graduation project standards. By combining an Angular 18 SSR frontend with a .NET 9 Clean Architecture backend, it models complex real-world workflows including marketplace discovery, real-time collaboration, payments, and AI assistance. 

Its strongest engineering qualities are the rigorous separation of concerns, the inclusion of enterprise resilience patterns (idempotency, Hangfire, caching, OpenTelemetry), and the robust real-time communication stack. Once secrets management and API documentation are polished, the system is fundamentally production-ready and highly scalable.
