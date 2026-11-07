# DonationHub

A donation and fundraising platform built with **ASP.NET Core 10**, **Entity Framework Core**, and **SQL Server**. DonationHub provides a secure REST API for managing users, fundraising campaigns, donations, and categories.

## Table of Contents

- [Overview](#overview)
- [Technologies](#technologies)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Database Setup](#database-setup)
- [Configuration and Secrets](#configuration-and-secrets)
- [Running the Application](#running-the-application)
- [API Documentation and Testing](#api-documentation-and-testing)
- [Design Decisions](#design-decisions)
- [Sprint 1 Features](#sprint-1-features)

## Overview

DonationHub is a RESTful API designed to support a fundraising platform where users can register, create fundraising campaigns (subject to authorization), and manage donations.

The application follows **Clean Architecture** principles to separate business logic, application workflows, infrastructure concerns, and API presentation.

## Technologies

- .NET 10 / ASP.NET Core Web API
- C# 
- Entity Framework Core
- SQL Server 2022
- ASP.NET Core Identity
- JWT Authentication
- Scalar / OpenAPI
- Docker & Docker Compose
- Generic Repository and Unit of Work patterns

## Prerequisites

Make sure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (if using the provided SQL Server container)
- SQL Server 2022 (Docker or local installation)
- Visual Studio 2022 or Visual Studio Code
- Git

## Project Structure

The solution is organized into four main projects:

```text
DonationHub/
│
├── DonationHub.Api/
│   ├── Controllers/
│   ├── Extensions/
│   ├── Program.cs
│   └── appsettings.json
│
├── DonationHub.Application/
│   ├── Authentication/
│   ├── Features/
│   └── Persistence/
│
├── DonationHub.Domain/
│   ├── Common/
│   └── Entities/
│
├── DonationHub.Infrastructure/
│   ├── Persistence/
│   │   ├── Configurations/
│   │   ├── Repositories/
│   │   └── AppDbContext.cs
│   └── Services/
│
├── docker-compose.yml
├── README.md
└── DonationHub.slnx
```

### Layer Responsibilities

- **Domain:** Contains core business entities, value objects, and domain rules. It has no dependency on external infrastructure.
- **Application:** Contains application services, feature workflows, DTOs, interfaces, and business use cases.
- **Infrastructure:** Implements persistence, Entity Framework Core configurations, repositories, and external service integrations.
- **API:** Exposes REST endpoints, handles HTTP requests, authorization, and dependency injection configuration.

## Getting Started

### 1. Clone the repository

```bash
git clone <repository-url>
cd DonationHub
```

### 2. Configure application secrets

Sensitive configuration values must not be committed to source control.

Initialize User Secrets for the API project:

```bash
dotnet user-secrets init --project DonationHub.Api
```

Configure the SQL Server connection string:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<YOUR_CONNECTION_STRING>" --project DonationHub.Api
```

Configure JWT settings:

```bash
dotnet user-secrets set "Jwt:SecretKey" "<YOUR_SECRET_KEY_AT_LEAST_32_BYTES>" --project DonationHub.Api

dotnet user-secrets set "Jwt:Issuer" "DonationHub" --project DonationHub.Api

dotnet user-secrets set "Jwt:Audience" "DonationHub.Client" --project DonationHub.Api

dotnet user-secrets set "Jwt:ExpiryMinutes" "15" --project DonationHub.Api

dotnet user-secrets set "Jwt:RefreshTokenExpiryDays" "7" --project DonationHub.Api
```

Configure the initial Admin account:

```bash
dotnet user-secrets set "SeedAdmin:Email" "<ADMIN_EMAIL>" --project DonationHub.Api

dotnet user-secrets set "SeedAdmin:Password" "<STRONG_ADMIN_PASSWORD>" --project DonationHub.Api

dotnet user-secrets set "SeedAdmin:FullName" "<ADMIN_FULL_NAME>" --project DonationHub.Api
```

Configure Donation settings:

```bash
dotnet user-secrets set "Donation:MinAmount" "1" --project DonationHub.Api

dotnet user-secrets set "Donation:MaxAmount" "10000" --project DonationHub.Api

dotnet user-secrets set "Donation:PendingTimeoutMinutes" "30" --project DonationHub.Api

dotnet user-secrets set "Donation:DefaultCurrency" "USD" --project DonationHub.Api
```

> The values above are development examples. Use the actual configuration keys and values defined in the project. Never commit real passwords, JWT signing keys, or connection strings.

## Database Setup

### 1. Start SQL Server

If using Docker Compose:

```bash
docker compose up -d
```

Make sure the SQL Server container is running before continuing.

### 2. Apply Entity Framework migrations

From the solution root:

```bash
dotnet ef database update --project DonationHub.Infrastructure --startup-project DonationHub.Api
```

To create a new migration after changing the database model:

```bash
dotnet ef migrations add <MigrationName> --project DonationHub.Infrastructure --startup-project DonationHub.Api
```

Apply the migration:

```bash
dotnet ef database update --project DonationHub.Infrastructure --startup-project DonationHub.Api
```

## Running the Application

Build the solution:

```bash
dotnet build
```

Run the API:

```bash
dotnet run --project DonationHub.Api
```

The local development API uses the launch settings configured in `DonationHub.Api/Properties/launchSettings.json`.

Current HTTP development address:

```text
http://localhost:5091
```

API base path:

```text
/api/v1
```

## API Documentation and Testing

DonationHub uses **OpenAPI and Scalar** to provide interactive API documentation.

When running in Development, open:

```text
http://localhost:5091/scalar
```

The OpenAPI document is available at:

```text
http://localhost:5091/openapi/v1.json
```

### Testing Protected Endpoints

Protected endpoints require a valid JWT Bearer access token.

1. Open Scalar.
2. Find `POST /api/v1/auth/login`.
3. Submit your registered user credentials.
4. Copy the returned `accessToken`.
5. Click the Authorize option in Scalar.
6. Enter your access token using the Bearer authentication scheme.
7. Execute protected requests.

Scalar will attach the token to requests requiring authentication.

### Authentication and Authorization

The API uses ASP.NET Core Identity and JWT authentication.

Available roles:

- **Admin:** Administrative operations, including staff and category management.
- **CampaignManager:** Campaign management operations.
- **Donor:** Donation-related operations.

Anonymous users can access public endpoints, while protected endpoints require the appropriate authenticated user and role.

## Design Decisions

### 1. Clean Architecture

**Decision:** Separate the application into Domain, Application, Infrastructure, and API projects.

**Reason:** This separation keeps business rules independent of frameworks and databases, improves maintainability, and makes individual components easier to test.

**Alternative:** A traditional layered architecture or a single-project application. These approaches can be simpler initially but may introduce tighter coupling as the application grows.

### 2. Generic Repository and Unit of Work

**Decision:** Use a Generic Repository together with the Unit of Work pattern for database write operations.

**Reason:** Repositories provide a consistent abstraction for adding, updating, and deleting entities. Unit of Work coordinates persistence through a single `SaveChangesAsync` operation, helping maintain consistency across changes.

**Alternative:** Injecting `DbContext` directly into application services. Although simpler, this couples application workflows to Entity Framework Core and makes persistence responsibilities less explicit.

### 3. Read Operations and DTO Projection

**Decision:** Use read-only queries with `AsNoTracking()` and DTO projection for read operations, following SRS §3.3 rule 5.

**Reason:** Read operations do not need change tracking. `AsNoTracking()` reduces EF Core tracking overhead, while DTO projection returns only the data required by the API and avoids exposing internal entity structures.

**Alternative:** Loading tracked entities and returning them directly. This can introduce unnecessary tracking, expose internal properties, and create serialization or over-fetching issues.

### 4. Entity Framework Core Fluent Configuration

**Decision:** Configure entity relationships, constraints, indexes, and property requirements using Fluent API.

**Reason:** Keeping persistence configuration separate from domain entities preserves the independence of the Domain layer and centralizes database-specific rules.

**Alternative:** Using data annotations directly on entities. This is convenient for simple models but can mix persistence concerns with domain code.

### 5. ASP.NET Core Identity and JWT

**Decision:** Use ASP.NET Core Identity for user management and JWT Bearer authentication for securing API endpoints.

**Reason:** Identity provides established user and role management capabilities, while JWT allows stateless authentication for REST API requests.

**Alternative:** Implementing custom authentication and password management. This would require maintaining security-sensitive functionality that is already provided by a mature framework.

### 6. Category Deletion Restriction

**Decision:** Prevent deleting categories that are referenced by existing campaigns.

**Reason:** This protects referential integrity and preserves the relationship between campaigns and their categories. The rule is enforced in the application workflow and supported by the database relationship using `DeleteBehavior.Restrict`.

**Alternative:** Cascade deletion, which could unintentionally delete campaigns when their category is removed.

### 7. Configuration and Secret Management

**Decision:** Use strongly typed Options classes, validation, and User Secrets for development configuration.

**Reason:** This keeps configuration structured and validated at application startup while preventing sensitive values from being committed to source control.

**Alternative:** Hardcoding configuration values in application code or storing secrets in committed configuration files, which creates security and maintenance risks.

### 8. Scalar and OpenAPI

**Decision:** Use Scalar as the interactive API documentation interface with OpenAPI and Bearer authentication support.

**Reason:** Scalar provides a convenient interface for exploring endpoints and testing authenticated requests without requiring a separate API client.

**Alternative:** Swagger UI through Swashbuckle. It is another established solution, but Scalar is the selected documentation interface for this .NET 10 project.

## Sprint 1 Features

Sprint 1 establishes the core foundation of DonationHub.

### Authentication and Identity

- User registration and login.
- JWT access tokens.
- Refresh token management.
- User logout.
- User profile retrieval.
- Role-based authorization.
- Admin staff creation.
- Initial Admin and role seeding.

### Categories Module

- Public category listing.
- Public category retrieval by ID.
- Admin-only category creation.
- Admin-only category updates.
- Admin-only category deletion.
- Bilingual Arabic and English category names.
- Unique category names per language.
- Protection against deleting categories with existing campaigns.

### Infrastructure and Configuration

- Clean Architecture solution structure.
- SQL Server integration.
- Entity Framework Core migrations.
- Generic Repository and Unit of Work.
- Strongly typed Options and startup validation.
- Docker Compose development database.
- OpenAPI documentation with Scalar.
- JWT Bearer security support.

---

**Project:** DonationHub  
**Framework:** ASP.NET Core 10  
**Database:** SQL Server 2022  
**Documentation:** Scalar / OpenAPI
