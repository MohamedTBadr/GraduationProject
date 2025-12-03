# 📌 Graduation Project – API

This repository contains the **backend implementation** of the graduation project.
The API is built with **ASP.NET Core Web API** and serves as the core backend of the entire system.

---

## 🚀 Features

* 🔐 **User Authentication & Authorization**
  Secure login, registration, JWT authentication, and role-based access control.

* ⚡ **Caching**
  Improves performance and reduces database load using in-memory or distributed caching.

* 📧 **Email Sending**
  Integrated email service for verification, notifications, and password recovery.

* 🔄 **Idempotent API**
  Prevents duplicate processing of network retries or repeated submissions.

* 🤖 **Gemini AI Model Integration**
  The API integrates with **Google Gemini** for AI-powered features such as:

  * Text generation
  * Classification
  * Summarization
  * Idea generation
  * Intelligent recommendations
    The AI layer follows a clean architecture design to ensure easy replacement or scaling of future models.

* 🐳 **Dockerized Deployment**
  Containerized backend for reliable and portable deployments.

* 🔗 **HTTPS Reverse Proxy (YARP)**

  * API exposed through a secure **HTTPS reverse proxy** at `https://localhost:5000`
  * Requests load-balanced across backend instances running at:

    * `http://localhost:5001`
    * `http://localhost:5002`
    * `http://localhost:5003`
  * Supports routing for all paths (e.g., `/api/...`)
  * Centralized gateway for request routing and load balancing.

---

## 🛠️ Tech Stack

* **ASP.NET Core Web API**
* **Entity Framework Core**
* **SQL Server**
* **Caching (In-Memory / Distributed)**
* **SMTP Email Service**
* **Idempotent Middleware**
* **Google Gemini AI Integration**
* **Docker / Docker Compose**
* **YARP Reverse Proxy**

---

## 📂 Project Structure

* ⚙️ **Business Logic Layer**
  Implements domain rules and workflows.

* 🗄️ **Data Access Layer**
  Manages interaction with SQL Server using EF Core.

* 🌐 **RESTful API Endpoints**
  Exposes all functionalities to the frontend.

* 🔄 **Idempotent Middleware**
  Ensures safe retriable operations.

* 🤖 **AI Integration Layer**
  Encapsulates communication with the Gemini API using clean service abstractions.

* 🔗 **Reverse Proxy Configuration**
  Handles secure HTTPS traffic and distributes workload across backend instances.

* 🐳 **Dockerfile & Docker Compose**
  Supports running the API in isolated containers for Dev/Prod.

---

## 🔧 Usage (Reverse Proxy Setup)

1. **Trust the development certificate** (necessary for HTTPS):

```bash
dotnet dev-certs https --trust
```

2. **Run the backend servers**

```bash
dotnet run --PORT=5001
dotnet run --PORT=5002
dotnet run --PORT=5003
```

3. **Run the reverse proxy**

```bash
dotnet run
```

---

