# Trendora ⌚

Trendora is a premium, fully featured E-commerce platform dedicated to watches. Built using **.NET MVC** and adhering to the principles of **Clean Architecture**, this project is designed for high scalability, maintainability, and testability.

---

## 🚀 Features

* **Premium Catalog:** Browse, filter, and search a curated collection of luxury and casual watches.
* **Shopping Cart & Checkout:** Seamless add-to-cart functionality with a secure checkout workflow.
* **User Authentication:** Secure user registration, login, and role-based access control (Admin/Customer) via ASP.NET Core Identity.
* **Admin Dashboard:** Comprehensive management system for inventory, categories, orders, and user management.
* **Responsive Design:** A sleek, modern UI optimized for desktop, tablet, and mobile devices.

---

## 🏗️ Architecture & Tech Stack

Trendora is built using **Clean Architecture** (Onion Architecture) to ensure the core business logic remains independent of external frameworks, databases, or UI implementations.

### Project Structure
* **Trendora.Domain:** Contains enterprise logic, core entities, enums, and interfaces. (No external dependencies).
* **Trendora.Application:** Contains business logic, DTOs, mapping profiles, and CQRS/Repository interfaces.
* **Trendora.Infrastructure:** Implements data access (Entity Framework Core), database migrations, and external services (e.g., Email, Payment gateways).
* **Trendora.WebUI:** The presentation layer built with ASP.NET Core MVC, Razor Views, and TailwindCSS/Bootstrap.

### Technology Stack
* **Backend:** .NET 8.0 / ASP.NET Core MVC
* **ORM:** Entity Framework Core
* **Database:** SQL Server (or PostgreSQL/MySQL depending on configuration)
* **Authentication:** ASP.NET Core Identity
* **Frontend:** HTML5, CSS3, JavaScript, Bootstrap / TailwindCSS

---

## 🛠️ Getting Started

Follow these steps to get a local copy of Trendora up and running.

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express or Developer edition)
* An IDE like Visual Studio 2022, VS Code, or JetBrains Rider


  
