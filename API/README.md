This updated README provides a high-level architectural deep dive, specifically tailored for your graduation project. It highlights your technical leadership as the **Backend Head** and demonstrates a "Senior Engineer" approach to documentation.

-----

# 📌 Eventora: Resilient Backend Architecture

**Production-Grade API Ecosystem for Event Management**

This repository houses the core backend for **Eventora**, a digital marketplace bridging event service vendors and clients. Built on **ASP.NET Core 9.0**, the system is engineered for high availability, financial precision, and real-time engagement.

-----

## 🏛️ Architectural Blueprint: Clean & Resilient

The project follows the **Clean Architecture (Onion)** pattern, ensuring that business rules remain independent of external frameworks, databases, or UI.

### 🏗️ Layered Responsibility

  * **Domain:** Core Entities (`Vendor`, `Order`, `Payment`), Value Objects, and Domain Exceptions. Encapsulates the "No Refund" business invariant.
  * **Application:** Use cases, MediatR handlers, and **Resilience Policy Definitions**.
  * **Infrastructure:** Concrete implementations for **AWS S3**, **Paymob**, **EF Core**, and **SignalR Hubs**.
  * **Presentation (API):** Controller logic, YARP configuration, and Middleware.

-----

## 🛡️ Resilience Strategy (Polly & .NET 9)

We treat failure as a first-class citizen. Every external I/O operation is protected by a **Resilience Pipeline**.

### 1\. Database Persistence Pipeline

  * **Exponential Backoff + Jitter:** Prevents the "Thundering Herd" effect during database recovery.
  * **Circuit Breaker:** If the SQL cluster enters a failing state, the circuit opens for 30s to prevent application hang.
  * **Command Isolation:** Save operations are wrapped in atomic pipelines to ensure `SaveChangesAsync` respects the timeout.

### 2\. Infrastructure Shielding

  * **AWS S3 Pipeline:** Implements a strict 30s timeout for file uploads to protect worker threads.
  * **Standard HTTP Handler:** Paymob and external API calls utilize `AddStandardResilienceHandler`, providing a unified shield of retries and circuit breaking.

-----

## 💳 Financial Integrity & Paymob Integration

The payment subsystem is designed for **Zero-Loss** and **Zero-Duplicate** transactions.

  * **Idempotency Keying:** Every payment intent is linked to a unique Domain `OrderId`. Retries at the network level will never result in double charges.
  * **Webhook Synchronization:** We treat the Paymob Webhook as the "Source of Truth." The system uses an **Append-Only Transaction Log** to track payment states.
  * **Security:** All payment keys are generated server-side. Sensitive card data never touches our servers (PCI-DSS Compliance).
  * **Policy:** The system enforces a **Non-Refundable** transaction model at the domain layer.

-----

## 💬 Real-Time Engine (SignalR)

Eventora features a low-latency chat system optimized for vendor-client negotiations.

  * **One-to-One Architecture:** Strictly peer-to-peer messaging to ensure privacy and focus.
  * **State Management:** Uses `OnConnectedAsync` and `OnDisconnectedAsync` to track user presence in real-time.
  * **Scalability:** Configured for a **Redis Backplane**, allowing SignalR to broadcast across multiple Dockerized API instances behind the proxy.

-----

## 📦 Cloud & AI Integration

### AWS S3 Storage

  * **Decoupled Storage:** Binaries reside in S3; only metadata (URLs, Keys) exists in SQL.
  * **Security:** Access is managed via **IAM Roles** and private bucket policies.

### Google Gemini AI

  * **Intelligent Automation:** Used for content moderation, vendor description summarization, and event recommendation logic.
  * **Model Agnostic:** The AI service is abstracted via interfaces, allowing a switch from Gemini to other LLMs without touching business logic.

-----

## 🔗 Traffic Orchestration (YARP Proxy)

To simulate a high-scale production environment, we use **YARP (Yet Another Reverse Proxy)**.

  * **Load Balancing:** Round-robin distribution across three containerized API instances.
  * **HTTPS Termination:** The proxy handles SSL/TLS, passing clean traffic to the internal Docker network.
  * **Health Probes:** Automatically removes unhealthy instances from the rotation.

-----

## 🛠️ Tech Stack & Requirements

  * **Framework:** .NET 9.0 (ASP.NET Core)
  * **Persistence:** EF Core + SQL Server
  * **Resilience:** Polly 8.0+
  * **Real-time:** SignalR
  * **Cloud:** AWS S3, Google Gemini AI, Paymob
  * **DevOps:** Docker, YARP, Apidog

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

-----

**Lead Developer:** Mohamed Tarek  
**Architecture:** Clean Architecture + Resilience Patterns  
**Purpose:** Graduation Project – Backend Excellence
