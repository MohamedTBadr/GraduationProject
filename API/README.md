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

---

## 🛠️ Tech Stack

- **ASP.NET Core Web API**  
- **Entity Framework Core**  
- **SQL Server**  
- **Caching (In-Memory / Distributed)**  
- **SMTP / Email Sender Service**  
- **Idempotent API Middleware**  
- **Docker / Docker Compose**  

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

- 🐳 **Dockerfile & Docker Compose**  
  Enables running the API in a containerized environment.  

---
