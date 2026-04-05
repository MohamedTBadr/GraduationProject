# 📌 Graduation Project – Eventora API

This repository contains the **backend implementation** of the Eventora ecosystem.
The API is built with **ASP.NET Core Web API** and follows **Clean Architecture** principles to serve as a high-performance gateway and core logic provider.

The architecture is **production-oriented**, focusing on resilience, scalability, security, and financial integrity.

-----

## 🛡️ Resilience & Fault Tolerance (New)

The system implements a centralized **Resilience Layer** using **Polly** and `Microsoft.Extensions.Resilience`. This protects the system from cascading failures.

  * **Database Resilience:** Custom pipelines for SQL Server handling transient failures with **Exponential Backoff** and **Jitter**.
  * **Infrastructure Shielding:** **Circuit Breakers** protect the API from failing external dependencies (AWS S3, Paymob).
  * **Request Timeouts:** Strict 2026-standard timeouts on all external I/O to prevent thread exhaustion.
  * **Bulkhead Isolation:** Ensures a failure in the AI or Email service doesn't crash the Payment or Auth flows.

-----

## 🚀 Key Features

### ⚡ Clean Architecture & Core Logic

  * **Domain-Driven Design (DDD):** Pure entities and business rules isolated from infrastructure.
  * **JWT-based Security:** Secure RBAC (Role-Based Access Control) with refresh token rotation.
  * **Idempotent API Design:** Middleware-level protection against duplicate network retries and payment callbacks.

### 💳 Paymob Payment Integration

  * **Secure Initiation:** Server-to-server payment intent generation.
  * **Financial Integrity:** Append-only transaction history and webhook-based state synchronization.
  * **Business Rules:** Strictly **No Refund** policy enforced at the domain level.
  * **SCA Ready:** Full support for 3-D Secure card payments.

### 💬 Real-Time & Notifications

  * **One-to-One Chat:** SignalR-powered chat optimized for direct vendor-to-client communication.
  * **SSE (Server-Sent Events):** Lightweight, real-time stream for unidirectional in-app notifications.
  * **Mail Support:** Automated SMTP-based transactional emails for critical alerts (Verification, Resets).
  * **Presence Tracking:** Real-time user connection status and live updates.

### 📦 Infrastructure Services

  * **AWS S3 Storage:** Secure file handling via pre-signed URLs; metadata in SQL, binaries in S3.
  * **Gemini AI:** Integrated for intelligent summarization and event recommendation logic.
  * **YARP Reverse Proxy:** Centralized HTTPS entry point with load balancing across backend instances.
  * **Caching:** Multi-level strategy (In-memory/Redis) for high-frequency data.

-----

## 🛠️ Tech Stack

  * **Backend:** ASP.NET Core 9.0 (Web API)
  * **Resilience:** Polly & Microsoft Resilience Extensions
  * **Database:** SQL Server & Entity Framework Core
  * **Real-time:** SignalR & SSE
  * **Cloud:** AWS S3 (Storage) & Google Gemini (AI)
  * **DevOps:** Docker, Docker Compose, YARP Proxy
  * **Documentation:** Apidog

-----

## 📂 Project Structure

```plaintext
├── Api             // Controllers, Hubs (SignalR/SSE), Middlewares (Entry Point)
├── Application     // Use Cases, DTOs, Interfaces, Resilience Policy Definitions
├── Domain          // Entities, Value Objects, Domain Exceptions (Zero Dependencies)
├── Infrastructure  // Persistence (EF Core), AWS S3, Paymob, Gemini AI, Email Service
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

You can run the full stack via Docker Compose or manually start multiple instances behind the YARP Proxy (ports 5001, 5002, 5003).

-----
## 📌 Project Principles

  * **One-to-One focus:** Chat and notification architecture is strictly peer-to-peer.
  * **Financial Rigidity:** Once a transaction is authorized, it is final (**No Refunds**).
  * **Resilient by Design:** Every external call is wrapped in a safety pipeline.


**Lead Developer:** Mohamed Tarek  
**Architecture:** Clean Architecture + Resilience Patterns  
**Purpose:** Graduation Project – Backend Excellence


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


