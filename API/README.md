<div align="center">

<br/>

<pre align="center">
███████╗██╗   ██╗███████╗███╗   ██╗████████╗ ██████╗ ██████╗  █████╗
██╔════╝██║   ██║██╔════╝████╗  ██║╚══██╔══╝██╔═══██╗██╔══██╗██╔══██╗
█████╗  ██║   ██║█████╗  ██╔██╗ ██║   ██║   ██║   ██║██████╔╝███████║
██╔══╝  ╚██╗ ██╔╝██╔══╝  ██║╚██╗██║   ██║   ██║   ██║██╔══██╗██╔══██║
███████╗ ╚████╔╝ ███████╗██║ ╚████║   ██║   ╚██████╔╝██║  ██║██║  ██║
╚══════╝  ╚═══╝  ╚══════╝╚═╝  ╚═══╝   ╚═╝    ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝
</pre>
### **Backend API — Graduation Project**

*A production-grade event planning ecosystem built on Clean Architecture*

<br/>

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-EF_Core-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-Distributed_Cache-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![AWS S3](https://img.shields.io/badge/AWS-S3_Storage-FF9900?style=for-the-badge&logo=amazons3&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-Real--Time-00BCEB?style=for-the-badge&logo=signalr&logoColor=white)

<br/>

<a href="https://app.apidog.com/invite/project?token=zG2ZhohbOdBh5J8CGYRgp" target=_blank>
  <img src="https://img.shields.io/badge/📖_Explore_Live_API_Docs-00C2B8?style=for-the-badge&logoColor=white" alt="Apidog Documentation" width="280"/>
</a>

<br/><br/>

</div>

---

## 📖 Overview

**Eventora** is the backend engine of a full-stack event planning platform, designed to connect clients with vendors for real-world events — Weddings, Graduations, Birthdays, and beyond.

The API is built with **ASP.NET Core 9.0** and follows **Clean Architecture** principles, serving as a high-performance gateway and core logic provider. Every design decision is **production-oriented**, with an unwavering focus on:

> **Resilience · Scalability · Security · Financial Integrity**

---

## 📑 Table of Contents

- [Architecture](#-architecture)
- [Key Features](#-key-features)
  - [Clean Architecture & Core Logic](#-clean-architecture--core-logic)
  - [Resilience & Fault Tolerance](#️-resilience--fault-tolerance)
  - [Product & Business Logic](#-product--business-logic)
  - [Payment Integration](#-paymob-payment-integration)
  - [Real-Time & Notifications](#-real-time--notifications)
  - [Infrastructure Services](#-infrastructure-services)
- [Tech Stack](#️-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Project Principles](#-project-principles)
- [API Documentation](#-api-documentation)

---

## 🏛 Architecture

Eventora is structured around **Clean Architecture** with a strict dependency rule — outer layers depend on inner layers, never the reverse.

```
┌─────────────────────────────────────────────────────────────────────┐
│                           API  LAYER                                │
│         Controllers · SignalR Hubs · SSE Middleware · YARP          │
├─────────────────────────────────────────────────────────────────────┤
│                       APPLICATION LAYER                             │
│          Use Cases · DTOs · Interfaces · Result Pattern             │
├─────────────────────────────────────────────────────────────────────┤
│                         DOMAIN LAYER                                │
│         Entities · Value Objects · Domain Exceptions · Enums        │
├─────────────────────────────────────────────────────────────────────┤
│                     INFRASTRUCTURE LAYER                            │
│     EF Core · AWS S3 · Paymob · Llama AI · Email · Lucene.NET       │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🚀 Key Features

### ⚡ Clean Architecture & Core Logic

| Feature | Description |
|---|---|
| **Domain-Driven Design (DDD)** | Pure entities and business rules fully isolated from infrastructure concerns |
| **Result Pattern** | Functional response handling via `Result<T>` — consistent outputs, zero flow-control exceptions |
| **JWT-based Security** | Secure RBAC with granular permissions scoped to **Clients**, **Vendors**, and **Admins** |
| **Idempotent API Design** | Custom Redis-backed middleware that prevents duplicate network retries and double-payment callbacks |

---

### 🛡️ Resilience & Fault Tolerance

The system implements a centralized **Resilience Layer** using **Polly** and `Microsoft.Extensions.Resilience` to guard against cascading failures across every external integration.

```
External Dependency Call
        │
        ▼
┌───────────────────┐
│   Request Timeout │  ← Prevents thread exhaustion
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│  Circuit Breaker  │  ← Shields API from failing dependencies (S3, Paymob)
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│  Retry + Backoff  │  ← Exponential backoff with jitter for transient DB failures
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│ Bulkhead Isolation│  ← Failure in AI/Email ≠ failure in Payments/Auth
└───────────────────┘
```

- **Database Resilience** — Custom pipelines for SQL Server with exponential backoff and jitter
- **Circuit Breakers** — Protect the API from failing external dependencies (AWS S3, Paymob)
- **Request Timeouts** — Strict timeouts on all external I/O to prevent thread exhaustion
- **Bulkhead Isolation** — A failure in the AI or Email service cannot crash critical Payment or Auth flows

---

### 🏆 Product & Business Logic

#### 🗓️ Direct-to-Event Booking Engine
A unique **"Cart-less" workflow** where users plan specific events (Weddings, Graduations) and aggregate vendor services directly into them — no abandoned carts, no ambiguity.

#### 🔍 3D Taxonomy System
A hierarchical matching engine based on:
```
Vendor Type  ──▶  Service Type  ──▶  Event Types Served
```
This three-dimensional taxonomy drives the **vendor recommendation engine**, ensuring clients see only contextually relevant results.

#### 🔄 Vendor Lifecycle Management
Automated onboarding with mandatory Admin moderation and multi-stage status tracking:

```
Registered ──▶ Pending Review ──▶ Approved ──▶ Active
                                      │
                                  isApproved
                                  isActive
```

> 
#### 🎫 Support Ticket System
A full-featured dispute resolution platform featuring:
- Priority levels: `Low` → `Medium` → `High` → `Critical`
- Internal escalation workflows
- Full audit trail per ticket

---

### 💳 Paymob Payment Integration

```
Client                  Eventora API               Paymob
  │                          │                        │
  │──── Initiate Payment ───▶│                        │
  │                          │── Server-to-Server ───▶│
  │                          │◀─── Payment Intent ────│
  │◀────── Redirect ──────── │                        │
  │                          │                        │
  │───────────────────────────────── 3D Secure ──────▶│
  │                          │                        │
  │                          │◀────── Webhook ─────── │  (Bank Confirmed)
  │                          │                        │
  │                    Mark Order PAID                 │
```

| Feature | Detail |
|---|---|
| **Secure Initiation** | Server-to-server payment intent generation — no client-side secrets |
| **Financial Integrity** | Webhook-based state sync — orders marked `Paid` only on verified bank confirmation |
| **No Refund Policy** | Strictly enforced at the domain level to protect vendor revenue |
| **SCA Ready** | Full support for 3-D Secure card payments |

---

### 💬 Real-Time & Notifications

| Channel | Technology | Purpose |
|---|---|---|
| **One-to-One Chat** | SignalR | Persistent, real-time vendor ↔ client messaging |
| **In-App Notifications** | SSE (Server-Sent Events) | Lightweight unidirectional stream (e.g., "Booking Approved") |
| **Transactional Email** | SMTP | Critical alerts: account verification, suspension notices |

---

### 📦 Infrastructure Services

| Service | Technology | Role |
|---|---|---|
| **File Storage** | AWS S3 | Binary streaming to S3; metadata indexed in SQL Server |
| **AI Engine** | Meta Llama | Event summarization, checklist generation, vendor recommendations |
| **Full-Text Search** | Lucene.NET | High-speed ranked search with tokenization for services & vendors |
| **Distributed Cache** | Redis | Idempotency keys, distributed session state |
| **In-Memory Cache** | .NET IMemoryCache | Static taxonomy data (Vendor Types, Service Types) |
| **Reverse Proxy** | YARP | Centralized HTTPS entry point with load balancing |

---

## 🛠️ Tech Stack

```
Language & Runtime
  └── C# · ASP.NET Core 9.0

Resilience
  └── Polly · Microsoft.Extensions.Resilience

Database
  └── SQL Server · Entity Framework Core (Retry-enabled)

Search & Cache
  └── Lucene.NET · Redis

Real-Time
  └── SignalR · Server-Sent Events (SSE)

Cloud & AI
  └── AWS S3 · Meta Llama

Payment
  └── Paymob (3DS · Webhook · Server-to-Server)

DevOps
  └── Docker · Docker Compose · YARP Reverse Proxy

Documentation
  └── Apidog
```

---

## 📂 Project Structure

```plaintext
Eventora/
│
├── Api/                    # Entry point for the application
│   ├── Controllers/        # HTTP endpoints (REST)
│   ├── Hubs/               # SignalR real-time hubs
│   ├── Middleware/          # Idempotency, SSE, exception handling
│   └── Extensions/         # Service registration & startup config
│
├── Application/            # Business orchestration layer
│   ├── UseCases/           # Feature-specific command & query handlers
│   ├── DTOs/               # Data Transfer Objects (Request / Response)
│   ├── Interfaces/         # Abstractions for infrastructure
│   ├── ResultPattern/      # Result<T> implementation
│   └── Resilience/         # Polly pipeline definitions
│
├── Domain/                 # The core — framework-independent
│   ├── Entities/           # Aggregate roots & domain entities
│   ├── ValueObjects/       # Immutable domain concepts
│   ├── Exceptions/         # Domain-specific exceptions
│   └── Enums/              # Taxonomy enums & status types
│
├── Infrastructure/         # External integrations
│   ├── Persistence/        # EF Core DbContext, migrations, repositories
│   ├── Storage/            # AWS S3 file handling
│   ├── Payment/            # Paymob integration & webhook handling
│   ├── AI/                 # Llama AI client & prompt engineering
│   ├── Search/             # Lucene.NET indexing & querying
│   └── Email/              # SMTP transactional email service
│
├── ReverseProxy/           # YARP configuration & HTTPS termination
│
└── Docker/                 # Containerization & orchestration
    ├── Dockerfile
    └── docker-compose.yml
```

---

## 🔧 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- A running Redis instance (or use the included Docker Compose setup)

---

### Step 1 — Trust the HTTPS Development Certificate

```bash
dotnet dev-certs https --trust
```

### Step 2 — Run the Full Stack

Spin up all services (API instances, Redis, SQL Server, YARP Proxy) with a single command:

```bash
docker-compose up --build
```

The system will start **multiple API instances** behind the YARP Reverse Proxy with load balancing across:

| Instance | Port |
|---|---|
| API Instance 1 | `5001` |
| API Instance 2 | `5002` |
| API Instance 3 | `5003` |
| YARP Entry Point | `443` (HTTPS) |

---

## 📌 Project Principles

These are the non-negotiable design mandates that govern every decision in this codebase:

> **🎯 One-to-One Focus**  
> Chat and notification architecture is strictly peer-to-peer. No group chats, no broadcast noise.

> **💰 Financial Rigidity**  
> Once a transaction is authorized, it is final. **No refunds.** This is enforced at the domain level — not just a UI label.

> **🛡️ Resilient by Design**  
> Every external call is wrapped in a safety pipeline (Retry + Circuit Breaker + Timeout + Bulkhead). Failures are contained, never cascading.

> **🤖 AI-Augmented**  
> Llama AI transforms raw user input (event name, budget, guest count) into structured, actionable event plans with vendor shortlists and smart checklists.

---

## 📡 API Documentation

All endpoints are fully documented, schema-referenced, and interactively testable.

<div align="center">

<br/>

<a href="https://app.apidog.com/invite/project?token=zG2ZhohbOdBh5J8CGYRgp">
  <img src="https://img.shields.io/badge/📖_Open_API_Documentation-00C2B8?style=for-the-badge&logoColor=white" alt="Apidog Documentation" width="320"/>
</a>

<br/><br/>

*Powered by **Apidog** · Interactive testing · Full schema reference · Live environment*

<br/>

</div>

---

<div align="center">

*Built with ❤️ as a Graduation Project*

</div>
