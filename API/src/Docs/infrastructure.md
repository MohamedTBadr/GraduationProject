### 🏗️ Backend Infrastructure Analysis

The project is built using **ASP.NET Core** following **Clean Architecture (Onion)** principles, with a strong emphasis on separation of concerns.

#### **Core Tech Stack**

- **Framework:** .NET 8 / ASP.NET Core
- **Architecture:** Clean Architecture (Domain, Application, Infrastructure, Web.Api)
- **Database:** Entity Framework Core (SQL Server likely, based on decimal types and Migrations)
- **Identity:** ASP.NET Core Identity with GUID primary keys
- **Real-time:** SignalR / Server-Sent Events (SSE)
- **Payment:** Paymob Integration
- **Caching:** Distributed Redis + Local HybridCache (L1/L2)
- **DevOps:** Docker & Docker Compose support

#### **Infrastructure Pros**

1.  **Strict Separation of Concerns:** Logic is well-partitioned. The `Domain` layer is pure, and `Application` services encapsulate business rules, keeping `Web.Api` controllers thin.
2.  **Robust Pattern Usage:** Excellent use of the **Result Pattern** (`Result<T>`) for consistent API responses and the **Repository Pattern** for data abstraction.
3.  **Advanced Caching Strategy:** Uses **Microsoft HybridCache** with a **Redis** backplane, providing ultra-fast L1 local memory lookups and a synchronized L2 distributed cache for high-traffic data (Vendors, Services).
4.  **Idempotency Engine:** A robust idempotency layer ensures that expensive or critical operations (like Payments and AI generation) are never executed twice for the same request.
5.  **Real-time Ready:** Native support for SignalR and SSE indicates the system is designed for interactive, live updates (chats, notifications).
6.  **Advanced EF Configuration:** Use of **Owned Types** (e.g., `Address`) and **Database Views** (e.g., `OrderInsight`) shows a sophisticated understanding of data modeling.
7.  **Synchronous Dependencies:** Some integrations (like Paymob) could benefit from more robust background processing (e.g., Hangfire/RabbitMQ) to handle failures or timeouts gracefully.
8.  **Logging Maturity:** While the structure is clean, it lacks a dedicated observability layer (like Serilog + Seq/ELK) for structured logging and performance monitoring.

---
#### **Infrastructure Cons / Areas for Improvement**

1.  **Implicit Monolith:** While a `Modules` folder exists, it appears underutilized. The system is currently a monolithic deployment.


---

### 🚀 Business & Backend Logic Features

#### **Current Capabilities**

- **Vendor Marketplace:** Verified vendor profiles, portfolios, and service/package management.
- **Search Indexing (Lucene.NET):** High-performance full-text search with **fuzzy matching**, category filtering, and price range optimization.
- **AI-Driven Planning Assistant:**
    - **Smart Budgeting:** Automatically allocates budget portions (Venue, Catering, etc.) based on total budget and event type using Llama-3.
    - **Event Timeline Generator:** Generates a minute-by-minute schedule based on booked services and event logistics.
- **Distributed Caching:** Seamless caching across all high-traffic endpoints (Vendors, Services, ServiceTypes) using Redis.
- **Event Orchestration:** Multi-item event planning with budget tracking and status synchronization logic.
- **Payment Flow:** Integrated checkout via Paymob with order/event status linking and idempotency protection.
- **Real-time Communication:** Full chat system with bi-directional notifications.
- **Support Ecosystem:** Ticket-based help desk with agent assignment and replies.
- **Collaborative Event Spaces:** Shared event planning where users can invite "Collaborators" with granular view/edit permissions.
- **Dynamic Review Sentiment:** AI-generated "Vendor Vibe" summaries based on customer review analysis.
- **Background Workflow Engine (Hangfire):** Handles long-running tasks like email invitations, PDF generation, and index maintenance.

#### **Recommended New Features (Business Value)**

1.  **Vendor Availability & Booking Calendar:** A unified calendar system for vendors to manage bookings, sync with Google/Outlook, and prevent double-booking.
2.  **Milestone-Based Payments:** Instead of full payment, allow users to pay deposits or installments linked to event progress/milestones.

#### **Recommended Backend Logic Improvements**

1.  **Soft Deletes & Auditing:** Implement an interceptor in `ApplicationDbContext` to handle `IsDeleted` flags and `CreatedAt/ModifiedAt` auditing automatically across all entities.
2.  **Rate Limiting & Security:** Implement per-user rate limiting on expensive endpoints (like Chat and AI) and add a dedicated "Audit Log" for sensitive administrative actions.
