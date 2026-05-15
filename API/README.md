
# 📌 Graduation Project – Eventora API

This repository contains the **backend implementation** of the Eventora ecosystem. 
The API is built with **ASP.NET Core 9.0** and follows **Clean Architecture** principles to serve as a high-performance gateway and core logic provider.

The architecture is **production-oriented**, focusing on resilience, scalability, security, and financial integrity.

-----

## 🚀 Key Features

### ⚡ Clean Architecture & Core Logic
*   **Domain-Driven Design (DDD):** Pure entities and business rules isolated from infrastructure.
*   **Result Pattern:** Functional response handling (`Result<T>`) to provide consistent API outputs and eliminate flow-control exceptions.
*   **JWT-based Security:** Secure RBAC (Role-Based Access Control) with granular permissions for Clients, Vendors, and Admins.
*   **Idempotent API Design:** Custom middleware protection using Redis to prevent duplicate network retries and double-payment callbacks.

### 🛡️ Resilience & Fault Tolerance
The system implements a centralized **Resilience Layer** using **Polly** and `Microsoft.Extensions.Resilience` to protect against cascading failures.
*   **Database Resilience:** Custom pipelines for SQL Server handling transient failures with **Exponential Backoff** and **Jitter**.
*   **Infrastructure Shielding:** **Circuit Breakers** protect the API from failing external dependencies (AWS S3, Paymob).
*   **Request Timeouts:** Strict 2026-standard timeouts on all external I/O to prevent thread exhaustion.
*   **Bulkhead Isolation:** Ensures a failure in the AI or Email service doesn't crash critical Payment or Auth flows.

### 🏆 Product & Business Excellence
*   **Direct-to-Event Booking Engine:** A unique "Cart-less" workflow where users plan specific events (Weddings, Graduations) and aggregate vendor services directly into them.
*   **3D Taxonomy System:** A hierarchical matching logic (Vendor Type > Service Type > Event Types Served) that drives the recommendation engine.
*   **Vendor Lifecycle Management:** Automated onboarding with mandatory Admin moderation and multi-stage status tracking (`isApproved`, `isActive`).
*   **Loyalty & Rewards Program:** A spend-to-earn logic (1 Point per 10 EGP) integrated into the order finalization pipeline.
*   **Support Ticket System:** A full-featured dispute resolution platform with prioritization (Low to Critical) and internal escalation workflows.

### 💳 Paymob Payment Integration
*   **Secure Initiation:** Server-to-server payment intent generation via Paymob API.
*   **Financial Integrity:** Webhook-based state synchronization ensuring orders are only marked `Paid` upon verified bank confirmation.
*   **Business Rules:** Strictly **No Refund** policy enforced at the domain level to protect vendor revenue.
*   **SCA Ready:** Full support for 3-D Secure card payments.

### 💬 Real-Time & Notifications
*   **One-to-One Chat:** SignalR-powered chat optimized for direct vendor-to-client communication with message persistence.
*   **SSE (Server-Sent Events):** Lightweight, real-time stream for unidirectional in-app notifications (e.g., "Booking Approved").
*   **Mail Support:** Automated SMTP-based transactional emails for critical alerts (Verification, Suspension reasons).

### 📦 Infrastructure Services
*   **AWS S3 Storage:** Secure file handling; metadata is indexed in SQL, while binaries are streamed to S3 buckets.
*   **Llama AI:** Integrated for intelligent event summarization, smart checklist generation, and personalized vendor recommendations.
*   **Search Engine (Lucene.NET):** High-speed full-text search providing ranked results and tokenization for services and vendors.
*   **Caching Strategy:** Multi-level caching using **Redis** for distributed state and **In-Memory** for static taxonomy data.
*   **YARP Reverse Proxy:** Centralized HTTPS entry point with load balancing across backend instances.

-----

## 🛠️ Tech Stack

*   **Framework:** ASP.NET Core 9.0 (Web API)
*   **Resilience:** Polly & Microsoft Resilience Extensions
*   **Database:** SQL Server & Entity Framework Core (Retry-enabled)
*   **Search & Cache:** Lucene.NET & Redis
*   **Real-time:** SignalR & Server-Sent Events (SSE)
*   **Cloud & AI:** AWS S3 (Storage) & **Meta Llama (AI)**
*   **DevOps:** Docker, Docker Compose, YARP Proxy
*   **Documentation:** Apidog

-----

## 📂 Project Structure

```plaintext
├── Api             // Controllers, SignalR Hubs, SSE Middlewares, Idempotency Logic
├── Application     // Use Cases, DTOs, Interfaces, Result Pattern, Resilience Definitions
├── Domain          // Entities, Value Objects, Domain Exceptions, Taxonomy Enums
├── Infrastructure  // Persistence (EF Core), AWS S3, Paymob, Llama AI, Email, Lucene
├── ReverseProxy    // YARP Configuration & HTTPS Termination
└── Docker          // Containerization & Orchestration Logic
```

-----

## 🔧 Getting Started

### 1️⃣ Trust HTTPS Development Certificate
```bash
dotnet dev-certs https --trust
```

### 2️⃣ Run the System
You can run the full stack via Docker Compose:
```bash
docker-compose up --build
```
The system will start multiple API instances behind the **YARP Proxy** (Load balancing on ports 5001, 5002, 5003).

-----

## 📌 Project Principles

*   **One-to-One focus:** Chat and notification architecture is strictly peer-to-peer.
*   **Financial Rigidity:** Once a transaction is authorized, it is final (**No Refunds**).
*   **Resilient by Design:** Every external call is wrapped in a safety pipeline (Retry + Circuit Breaker).
*   **AI-Augmented:** Leveraging **Llama AI** to transform raw user input into actionable event plans.


-----

## 📡 API Documentation

All endpoints are fully documented and interactively testable via **Apidog**.  
Click below to join the project and explore the API:

<div align="center">

<a href="https://app.apidog.com/invite/project?token=zG2ZhohbOdBh5J8CGYRgp">
  <img src="https://img.shields.io/badge/📖_Explore_API_Docs-00C2B8?style=for-the-badge&logoColor=white" alt="Apidog Documentation" width="300">
</a>

<br/>
<sub>Powered by <strong>Apidog</strong> · Interactive testing · Full schema reference</sub>

</div>
```

