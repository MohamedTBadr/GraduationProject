# EpicHub — Backend API

<div align="center">

<br/>

<pre align="center">
███████╗██████╗ ██╗ ██████╗██╗  ██╗██╗   ██╗██████╗ 
██╔════╝██╔══██╗██║██╔════╝██║  ██║██║   ██║██╔══██╗
█████╗  ██████╔╝██║██║     ███████║██║   ██║██████╔╝
██╔══╝  ██╔═══╝ ██║██║     ██╔══██║██║   ██║██╔══██╗
███████╗██║     ██║╚██████╗██║  ██║╚██████╔╝██████╔╝
╚══════╝╚═╝     ╚═╝ ╚═════╝╚═╝  ╚═════╝ ╚═════╝ 
</pre>

### **Enterprise Event Planning Platform — Graduation Project**

*A production-grade event planning ecosystem built on Clean Architecture, Resilience Engineering, and Modern Distributed Infrastructure.*

<br/>

![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-9.0-512BD4?style=for-the-badge\&logo=dotnet\&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-EF_Core-CC2927?style=for-the-badge\&logo=microsoftsqlserver\&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-Distributed_Cache-DC382D?style=for-the-badge\&logo=redis\&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge\&logo=docker\&logoColor=white)
![AWS S3](https://img.shields.io/badge/AWS-S3_Storage-FF9900?style=for-the-badge\&logo=amazons3\&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-Real--Time-00BCEB?style=for-the-badge\&logo=signalr\&logoColor=white)
![YARP](https://img.shields.io/badge/YARP-Reverse_Proxy-6E4AFF?style=for-the-badge)
![Polly](https://img.shields.io/badge/Polly-Resilience-FF6B35?style=for-the-badge)

<br/>

<a href="https://epichub.apidog.io/" target="_blank">
  <img src="https://img.shields.io/badge/📖_Explore_Live_API_Docs-00C2B8?style=for-the-badge&logoColor=white" width="320"/>
</a>

<br/><br/>

</div>

---

# 📖 Overview

**EpicHub** is the backend engine powering a full-stack enterprise event planning ecosystem designed to connect clients with vendors for real-world events such as:

* Weddings
* Graduations
* Birthdays
* Corporate Events
* Private Celebrations

The platform is built using **ASP.NET Core 9.0** and follows a strict **Clean Architecture** approach focused on scalability, maintainability, resilience, and financial integrity.

The system combines:

* Enterprise-grade backend engineering
* Distributed infrastructure patterns
* Real-time communication
* AI-assisted planning
* Secure payment workflows
* High-availability gateway architecture

---

# 🎯 Core Engineering Goals

> **Scalability · Reliability · Security · Financial Integrity · Clean Separation of Concerns**

The architecture was designed with production-level principles from day one.

Key goals include:

* Preventing cascading failures
* Isolating infrastructure concerns
* Protecting payment consistency
* Supporting horizontal scaling
* Maintaining strict domain boundaries
* Enabling future microservice migration

---

# 📑 Table of Contents

* [Architecture](#-architecture)
* [API Gateway & Edge Architecture](#-api-gateway--edge-architecture-yarp)
* [Key Features](#-key-features)
* [Resilience Engineering](#️-resilience-engineering)
* [Payment Architecture](#-payment-architecture)
* [Real-Time Communication](#-real-time-communication)
* [Infrastructure Services](#-infrastructure-services)
* [Tech Stack](#️-tech-stack)
* [Project Structure](#-project-structure)
* [Getting Started](#-getting-started)
* [Deployment Strategy](#-deployment-strategy)
* [Project Principles](#-project-principles)
* [API Documentation](#-api-documentation)

---

# 🏛 Architecture

EpicHub follows a layered **Clean Architecture** model with strict dependency inversion rules.

```plaintext
┌─────────────────────────────────────────────────────────────────────┐
│                      API GATEWAY LAYER                              │
│      YARP · JWT Validation · Rate Limiting · CORS · Serilog        │
├─────────────────────────────────────────────────────────────────────┤
│                           API LAYER                                 │
│         Controllers · SignalR Hubs · SSE Middleware                │
├─────────────────────────────────────────────────────────────────────┤
│                       APPLICATION LAYER                             │
│       Use Cases · DTOs · Interfaces · Result Pattern               │
├─────────────────────────────────────────────────────────────────────┤
│                         DOMAIN LAYER                                │
│      Entities · Value Objects · Domain Exceptions · Enums          │
├─────────────────────────────────────────────────────────────────────┤
│                     INFRASTRUCTURE LAYER                            │
│  EF Core · AWS S3 · Paymob · Llama AI · Email · Lucene.NET         │
└─────────────────────────────────────────────────────────────────────┘
```

---

# 🌐 API Gateway & Edge Architecture (YARP)

To improve scalability and separation of concerns, the platform introduces a dedicated **YARP API Gateway** in front of the backend monolith.

```plaintext
Client Applications
(Angular · Mobile · Admin Dashboard)
                │
                ▼
┌──────────────────────────────────────────────┐
│                YARP GATEWAY                  │
│──────────────────────────────────────────────│
│ • HTTPS Termination                          │
│ • JWT Validation                             │
│ • Rate Limiting                              │
│ • CORS Policies                              │
│ • Serilog Request Logging                    │
│ • Reverse Proxy Routing                      │
└──────────────────────────────────────────────┘
                │
                ▼
┌──────────────────────────────────────────────┐
│              WEB.API MONOLITH                │
│──────────────────────────────────────────────│
│ • Business Logic                             │
│ • Authorization Policies                     │
│ • SignalR                                    │
│ • Application Services                       │
│ • Database Transactions                      │
└──────────────────────────────────────────────┘
```

---

## 🚦 Gateway Responsibilities

| Responsibility     | Technology                | Purpose                                        |
| ------------------ | ------------------------- | ---------------------------------------------- |
| Reverse Proxy      | YARP                      | Route external traffic to backend APIs         |
| JWT Authentication | ASP.NET Authentication    | Validate access tokens before reaching backend |
| Rate Limiting      | ASP.NET Core Rate Limiter | Prevent brute force & API abuse                |
| Logging            | Serilog                   | Centralized structured request logging         |
| CORS Management    | ASP.NET Core CORS         | Frontend access control                        |
| Load Balancing     | YARP Clusters             | Distribute requests across instances           |

---

## 🔐 Gateway-First Authentication Strategy

EpicHub uses a **Gateway-First JWT Validation** strategy.

```plaintext
Client Request
      │
      ▼
YARP Gateway
      ├── Validate JWT
      ├── Validate Signature
      ├── Apply Rate Limits
      └── Forward Request
               │
               ▼
           Web.API
```

### Why Validate JWT at the Gateway?

| Benefit                  | Description                                                   |
| ------------------------ | ------------------------------------------------------------- |
| Defense in Depth         | Unauthorized traffic is blocked before reaching the backend   |
| Performance Optimization | Reduces unnecessary processing in Web.API                     |
| User-Aware Rate Limiting | Enables per-user throttling instead of only IP throttling     |
| Security Isolation       | Internal services receive trusted authenticated requests only |

The backend API still performs authorization and policy checks using forwarded claims and roles.

---

## ⚙️ Infrastructure Separation

### Moved to Gateway

* JWT Validation
* Rate Limiting
* CORS Policies
* Request Logging
* Reverse Proxy Routing

### Remaining in Web.API

* Domain Logic
* Authorization Policies
* SignalR Hubs
* Business Workflows
* Database Transactions
* Application Services

---

# 🚀 Key Features

## ⚡ Clean Architecture & Core Logic

| Feature                    | Description                                               |
| -------------------------- | --------------------------------------------------------- |
| Domain-Driven Design (DDD) | Business rules isolated from frameworks & infrastructure  |
| Result Pattern             | Consistent `Result<T>` response handling                  |
| Dependency Inversion       | Infrastructure depends on Application abstractions        |
| CQRS-Inspired Structure    | Separation between commands and queries                   |
| JWT-Based Security         | Role-based authorization for Clients, Vendors, and Admins |
| Idempotent APIs            | Prevent duplicate requests and payment retries            |

---

## 🗓️ Direct-to-Event Booking Engine

EpicHub introduces a **Cart-less Event Workflow**.

Instead of generic shopping carts, users create real events and attach vendor services directly to those events.

```plaintext
Event
   ├── Venue
   ├── Photographer
   ├── Decoration
   ├── Catering
   └── Music Band
```

This creates:

* Better business context
* Reduced abandoned carts
* Structured budgeting
* Cleaner planning workflows

---

## 🔍 3D Vendor Taxonomy System

The platform uses a three-dimensional recommendation structure:

```plaintext
Vendor Type
      │
      ▼
Service Type
      │
      ▼
Supported Event Types
```

This enables intelligent filtering and contextual vendor recommendations.

---

## 🔄 Vendor Lifecycle Management

```plaintext
Registered
      │
      ▼
Pending Review
      │
      ▼
Approved
      │
      ▼
Active
```

Features include:

* Admin moderation
* Multi-stage activation
* Suspension workflows
* Verification pipelines

---

## 🎫 Support Ticket System

Enterprise-grade support workflow with:

* Priority escalation
* Audit trails
* Internal moderation
* Ticket lifecycle tracking

```plaintext
Low → Medium → High → Critical
```

---

# 🛡️ Resilience Engineering

EpicHub implements centralized resilience pipelines using:

* Polly
* Microsoft.Extensions.Resilience

---

## 🔄 Resilience Flow

```plaintext
External Dependency Call
        │
        ▼
┌───────────────────┐
│   Request Timeout │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│  Circuit Breaker  │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│  Retry + Backoff  │
└────────┬──────────┘
         │
         ▼
┌───────────────────┐
│ Bulkhead Isolation│
└───────────────────┘
```

---

## 🧩 Resilience Features

| Feature            | Purpose                               |
| ------------------ | ------------------------------------- |
| Request Timeouts   | Prevent thread exhaustion             |
| Retry Policies     | Handle transient failures             |
| Circuit Breakers   | Prevent cascading dependency failures |
| Bulkhead Isolation | Isolate failures between services     |
| Jittered Backoff   | Prevent synchronized retry storms     |

---

# 💳 Payment Architecture

EpicHub integrates with **Paymob** using a secure server-to-server workflow.

```plaintext
Client
   │
   ▼
EpicHub API
   │
   ├── Create Payment Intent
   │
   ▼
Paymob
   │
   ├── 3D Secure
   ├── Bank Verification
   │
   ▼
Webhook Callback
   │
   ▼
Mark Order Paid
```

---

## 💰 Financial Integrity Rules

| Rule                        | Description                                     |
| --------------------------- | ----------------------------------------------- |
| Server-to-Server Payments   | No payment secrets exposed to frontend          |
| Webhook Validation          | Orders marked paid only after verified callback |
| Idempotent Payment Handling | Prevent duplicate callbacks                     |
| Domain-Enforced No Refunds  | Financial rules enforced at business layer      |
| 3D Secure Support           | SCA-compliant payment processing                |

---

# 💬 Real-Time Communication

| Feature               | Technology               |
| --------------------- | ------------------------ |
| One-to-One Messaging  | SignalR                  |
| Notifications Stream  | Server-Sent Events (SSE) |
| Email Notifications   | SMTP                     |
| Real-Time Vendor Chat | SignalR Hubs             |

---

# 📦 Infrastructure Services

| Service           | Technology            | Role                          |
| ----------------- | --------------------- | ----------------------------- |
| Database          | SQL Server            | Persistent relational storage |
| ORM               | Entity Framework Core | Data access layer             |
| Distributed Cache | Redis                 | Idempotency & caching         |
| In-Memory Cache   | IMemoryCache          | Static lookup optimization    |
| Search Engine     | Lucene.NET            | Full-text search              |
| File Storage      | AWS S3                | Media storage                 |
| AI Engine         | Meta Llama            | Smart recommendations         |
| Reverse Proxy     | YARP                  | Gateway & routing             |
| Containerization  | Docker                | Deployment orchestration      |

---

# 🛠️ Tech Stack

```plaintext
Backend
 └── ASP.NET Core 9.0

Architecture
 └── Clean Architecture · DDD

Database
 └── SQL Server · EF Core

Caching
 └── Redis · IMemoryCache

Resilience
 └── Polly · Microsoft.Extensions.Resilience

Gateway
 └── YARP Reverse Proxy

Real-Time
 └── SignalR · SSE

AI
 └── Meta Llama

Payments
 └── Paymob

Storage
 └── AWS S3

Search
 └── Lucene.NET

DevOps
 └── Docker · Docker Compose
```

---

# 📂 Project Structure

```plaintext
EpicHub/
│
├── Api/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Hubs/
│   └── Extensions/
│
├── Application/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── UseCases/
│   ├── ResultPattern/
│   └── Resilience/
│
├── Domain/
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Exceptions/
│   └── Enums/
│
├── Infrastructure/
│   ├── Persistence/
│   ├── Storage/
│   ├── Search/
│   ├── AI/
│   ├── Payment/
│   └── Email/
│
├── ReverseProxy/
│   ├── Program.cs
│   ├── appsettings.json
│   └── YARP Configuration
│
└── Docker/
    ├── Dockerfile
    └── docker-compose.yml
```

---

# 🔧 Getting Started

## Prerequisites

* .NET 9 SDK
* Docker Desktop
* SQL Server
* Redis

---

## 1️⃣ Trust HTTPS Development Certificate

```bash
dotnet dev-certs https --trust
```

---

## 2️⃣ Run Web.API

```bash
dotnet run --project src/Web.Api
```

Runs on:

```plaintext
https://localhost:5030
http://localhost:5031
```

---

## 3️⃣ Run ReverseProxy

```bash
dotnet run --project src/ReverseProxy
```

Runs on:

```plaintext
https://localhost:5001
http://localhost:5000
```

---

## 4️⃣ Frontend Configuration

Frontend applications should communicate exclusively with:

```plaintext
https://localhost:5001
```

---

## 5️⃣ Run Full Infrastructure

```bash
docker-compose up --build
```

This starts:

* Web.API instances
* YARP Gateway
* SQL Server
* Redis
* Docker networking

---

# 🐳 Deployment Strategy

## 🔒 Internal Service Isolation

Only the Gateway should expose public ports.

The backend monolith remains internal within Docker/Kubernetes networking.

---

## 📈 Horizontal Scaling

```plaintext
Gateway
   ├── Web.API Instance #1
   ├── Web.API Instance #2
   └── Web.API Instance #3
```

This architecture supports future migration toward distributed services.

---

## ☁️ Future Infrastructure Enhancements

* Distributed Tracing
* OpenTelemetry
* API Versioning
* Response Caching
* WAF Integration
* Canary Deployments
* Kubernetes Autoscaling
* Service Discovery

---

# 📌 Project Principles

## 🎯 One-to-One Communication

Messaging architecture is intentionally peer-to-peer.

No group chats. No noisy broadcasts.

---

## 💰 Financial Rigidity

Transactions are final once authorized.

Refund prevention is enforced at the domain layer.

---

## 🛡️ Resilient by Design

Every external dependency call is protected using:

* Retry
* Circuit Breaker
* Timeout
* Bulkhead Isolation

---

## 🤖 AI-Augmented Planning

Meta Llama transforms user event input into:

* Smart planning recommendations
* Vendor shortlists
* Dynamic checklists
* Event summaries

---

# 📡 API Documentation

All endpoints are fully documented and interactively testable.

<div align="center">

<br/>

<a href="https://app.apidog.com/invite/project?token=zG2ZhohbOdBh5J8CGYRgp">
  <img src="https://img.shields.io/badge/📖_Open_API_Documentation-00C2B8?style=for-the-badge&logoColor=white" width="340"/>
</a>

<br/><br/>

*Powered by Apidog · Interactive Testing · Full Schema Reference · Live Environment*

<br/>

</div>

---

# ❤️ Credits

Built as a Graduation Project using modern enterprise backend engineering practices.

---

<div align="center">

### ⭐ If you like this project, consider starring the repository.

</div>
