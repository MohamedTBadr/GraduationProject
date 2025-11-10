# 📌 Graduation Project – API

This repository contains the **backend implementation** of the graduation project.  
The API is built with **ASP.NET Core Web API** and serves as the foundation of the system.

---

## 🚀 Features

- 🔐 **User Authentication & Authorization**  
  Secure login, registration, and role-based access control.  

- ⚡ **Caching**  
  Improves performance and reduces database load.  

- 📧 **Email Sending**  
  Integrated email service for account verification, password reset, and notifications.  

- 🔄 **Idempotent API**  
  Ensures safe retry of requests (e.g., duplicate submissions, network retries) by preventing unintended side effects.  

- 🐳 **Dockerized Deployment**  
  Containerized backend for easy deployment and scalability.  

- 🔗 **HTTPS Reverse Proxy (YARP)**  
  - API is accessible via an **HTTPS reverse proxy** running on `https://localhost:5000`.  
  - Forwards requests to multiple backend servers (load-balanced) running on `http://localhost:5001`, `5002`, `5003`.  
  - Supports routing all paths (e.g., `/api/...`) securely and transparently.  
  - Central entry point for API requests and load balancing.  

---

## 🛠️ Tech Stack

- **ASP.NET Core Web API**  
- **Entity Framework Core**  
- **SQL Server**  
- **Caching (In-Memory / Distributed)**  
- **SMTP / Email Sender Service**  
- **Idempotent API Middleware**  
- **Docker / Docker Compose**  
- **YARP Reverse Proxy**  

---

## 📂 Project Structure

- ⚙️ **Business Logic Implementation**  
  Core functionality and workflows for the system.  

- 🗄️ **Database Access & Management**  
  Efficient handling of data using Entity Framework Core.  

- 🌐 **RESTful Endpoints**  
  Provides APIs consumed by the frontend application.  

- 🔄 **Idempotent Middleware**  
  Protects API endpoints against accidental duplicate requests.  

- 🔗 **Reverse Proxy Configuration**  
  - Centralizes API routing and enables HTTPS access.  
  - Load balances requests across multiple backend servers.  
  - Configured via `appsettings.json` for easy modification.  

- 🐳 **Dockerfile & Docker Compose**  
  Enables running the API in a containerized environment.  

---

## 🔧 Usage (Reverse Proxy)

1. **Trust the dev certificate** (required for HTTPS):

```bash
dotnet dev-certs https --trust
then run servers
dotnet run --PORT=5001
dotnet run --PORT=5002
dotnet run --PORT=5003
 then proxy
dotnet run
