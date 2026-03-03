# 📌 Graduation Project – API

This repository contains the **backend implementation** of the graduation project.
The API is built with **ASP.NET Core Web API** and serves as the **core backend and gateway** for the entire system.

The architecture is **production-oriented**, focusing on scalability, security, idempotency, and financial correctness.

---

## 🚀 Features

### 🔐 Authentication & Authorization

* Secure user registration and login
* JWT-based authentication
* Role-based access control (RBAC)
* Token expiration and refresh handling

---

### ⚡ Caching Layer

* In-memory caching for high-frequency reads
* Optional distributed caching (Redis-ready)
* Reduces database load and improves response time

---

### 📧 Email Services

* SMTP-based email delivery
* Email verification
* Password reset and recovery
* System notifications

---

### 💬 Real-Time Chat (SignalR Hubs)

The system supports **real-time communication** using **ASP.NET Core SignalR**.

#### Capabilities

* One-to-one and group chat support
* Real-time message delivery
* User connection and presence tracking
* Scalable hub-based architecture

#### Design Highlights

* SignalR Hubs isolated from business logic
* Authentication integrated with JWT
* Ready for scale-out using Redis backplane
* Clean separation between Chat domain and API layer

---

### 📦 File Storage – AWS S3

The API integrates with **Amazon S3** for secure and scalable file storage.

#### Supported Features

* Upload and download files securely
* Pre-signed URLs for controlled client access
* Public and private bucket support
* Metadata and content-type preservation

#### Storage Architecture

* Files are stored outside the application server
* Database stores only file metadata and references
* S3 access handled via server-side credentials
* Environment-based bucket configuration

> ⚠️ Large files never pass through the database

---

### 🔄 Idempotent API Design

* Prevents duplicate processing of:

  * Network retries
  * Payment callbacks
  * Client resubmissions
* Ensures **financial and business data consistency**

---

### 💳 Paymob Payment Gateway Integration (NEW)

The system includes a **full Paymob payment integration** designed according to real-world financial standards.

#### Supported Capabilities

* Card payments (3-D Secure / SCA supported)
* Wallet & InstaPay ready (extensible)
* Secure server-side payment initiation
* Webhook-based payment confirmation
* Refund and reconciliation support

#### Payment Flow

1. Business order is created internally
2. Backend creates a Paymob payment intent (order)
3. Payment key is generated securely
4. User is redirected to Paymob hosted payment iframe
5. Paymob sends webhook callbacks
6. Backend validates and updates payment state

> ⚠️ Frontend never communicates with Paymob directly

#### Payment Architecture Highlights

* Order ≠ Payment ≠ Transaction separation
* Append-only transaction history
* Idempotent webhook handling
* Future-ready for multiple payment providers

---

### 🤖 Gemini AI Integration

The API integrates with **Google Gemini AI** to provide intelligent features:

* Text generation
* Classification
* Summarization
* Idea generation
* Recommendation logic

The AI layer follows **Clean Architecture principles**, allowing:

* Easy model replacement
* Scalability for future AI providers
* Clear separation from business logic

---

### 🔗 HTTPS Reverse Proxy (YARP)

A centralized **YARP reverse proxy** exposes the API securely:

* HTTPS endpoint:

  ```
  https://localhost:5000
  ```

* Load-balanced backend instances:

  * [http://localhost:5001](http://localhost:5001)
  * [http://localhost:5002](http://localhost:5002)
  * [http://localhost:5003](http://localhost:5003)

#### Capabilities

* Centralized routing (`/api/*`)
* HTTPS termination
* Load balancing across instances
* Single entry point for frontend clients

---

### 🐳 Dockerized Deployment

* Fully containerized backend
* Dockerfile for API services
* Docker Compose for local orchestration
* Environment-based configuration support

---

## 🛠️ Tech Stack

* **ASP.NET Core Web API**
* **SignalR (Real-Time Communication)**
* **Entity Framework Core**
* **SQL Server**
* **JWT Authentication**
* **Caching (In-Memory / Distributed)**
* **SMTP Email Service**
* **AWS S3 File Storage**
* **Paymob Payment Gateway**
* **Google Gemini AI**
* **Idempotent Middleware**
* **YARP Reverse Proxy**
* **Docker & Docker Compose**

---

## 📂 Project Structure

```
├── Api
│   ├── Controllers
│   ├── Hubs
│   ├── Middlewares
│   └── Filters
│
├── Application
│   ├── Services
│   │   ├── Payment
│   │   ├── Email
│   │   ├── AI
│   │   ├── Chat
│   │   └── Authentication
│   └── Interfaces
│
├── Domain
│   ├── Entities
│   │   ├── Order
│   │   ├── Payment
│   │   ├── PaymentTransaction
│   │   ├── Refund
│   │   └── ChatMessage
│   └── Enums
│
├── Infrastructure
│   ├── Data
│   ├── Repositories
│   ├── Paymob
│   ├── Gemini
│   ├── Email
│   └── Storage
│       └── S3
│
├── ReverseProxy
│   └── YarpConfig
│
├── Docker
│   ├── Dockerfile
│   └── docker-compose.yml
│
└── README.md
```

---



> Financial data is append-only and webhook-driven

---

## 🔧 Usage – Reverse Proxy Setup

### 1️⃣ Trust HTTPS Development Certificate

```bash
dotnet dev-certs https --trust
```

---

### 2️⃣ Run Backend Instances

```bash
dotnet run --urls=http://localhost:5001
dotnet run --urls=http://localhost:5002
dotnet run --urls=http://localhost:5003
```

---

### 3️⃣ Run Reverse Proxy

```bash
dotnet run --project ReverseProxy
```

---

## ✅ Design Goals Achieved

* Secure and scalable architecture
* Financially correct payment handling
* Clean separation of concerns
* Production-ready deployment
* Extensible for future services

---

## 📌 Notes

* All sensitive operations (payments, AI, email, storage) are server-side only
* Webhook endpoints are idempotent and auditable
* File storage is externalized via AWS S3
* Chat system is real-time and horizontally scalable

---
<div align="center">

# 🚀 Interactive API Playground

Stop manual testing and **start collaborating**.  
Join our official Postman Workspace for instant access to pre-configured collections, environment variables, and documentation.

<br>

<a href="https://app.getpostman.com/join-team?invite_code=a545087eeefb497e352e99484f2e99d3fce6402cec5d39107de64f9b765b154b&target_code=aace9867947f2b0034869f3790b48615">
  <img src="https://img.shields.io/badge/🚀_JOIN_WORKSPACE-FF6C37?style=for-the-badge&logo=postman&logoColor=white" alt="Join Workspace" width="300">
</a>

<br>

<sub>✨ Pre-concluded collections • Environment variables • Live docs</sub>

</div>

---
If this repository is used for evaluation or demonstration, it reflects **real-world backend engineering practices**, not tutorial-level implementations.
