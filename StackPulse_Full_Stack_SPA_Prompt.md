# StackPulse — Full-Stack Enterprise SPA

Create a complete, production-ready full-stack web application named **StackPulse**.

The application must use:

- **Frontend:** React + TypeScript + Vite
- **Backend:** ASP.NET Core Web API + C#
- **Database:** SQL Server by default, but keep the data layer configurable for MySQL
- **ORM:** Entity Framework Core
- **Authentication:** JWT + refresh tokens
- **API:** RESTful ASP.NET Core Web API
- **SPA:** React SPA
- **Deployment:** Docker
- **Web server/reverse proxy:** Nginx-ready
- **CI/CD:** Jenkins-ready
- **Production architecture:** React compiled into ASP.NET Core `wwwroot`, so the frontend and backend are served from the same domain/application.

Do NOT use Blazor.

Do NOT use Razor Pages for the frontend.

Do NOT create a separate Node.js backend.

The backend must be ASP.NET Core.

The frontend must be React.

---

# 1. Project Name

The complete project name is:

```text
StackPulse
```

Use these names consistently:

```text
Solution:
StackPulse.sln

Backend:
StackPulse.Api

Frontend:
stackpulse-ui

Root:
StackPulse
```

Use the namespace:

```csharp
StackPulse.Api
```

---

# 2. Repository Structure

Create the following structure:

```text
StackPulse/
│
├── StackPulse.sln
│
├── backend/
│   └── StackPulse.Api/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── UsersController.cs
│       │   └── DashboardController.cs
│       ├── Services/
│       │   ├── Interfaces/
│       │   ├── AuthService.cs
│       │   ├── UserService.cs
│       │   └── DashboardService.cs
│       ├── Repositories/
│       │   ├── Interfaces/
│       │   └── ...
│       ├── Models/
│       │   ├── User.cs
│       │   ├── RefreshToken.cs
│       │   └── ...
│       ├── DTOs/
│       │   ├── Auth/
│       │   ├── Users/
│       │   └── Dashboard/
│       ├── Data/
│       │   ├── StackPulseDbContext.cs
│       │   ├── Configurations/
│       │   └── Migrations/
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── RequestLoggingMiddleware.cs
│       ├── Configuration/
│       │   ├── JwtSettings.cs
│       │   └── DatabaseSettings.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       ├── wwwroot/
│       │   └── .gitkeep
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── StackPulse.Api.csproj
│       └── Dockerfile
│
├── frontend/
│   └── stackpulse-ui/
│       ├── src/
│       │   ├── components/
│       │   │   ├── common/
│       │   │   ├── layout/
│       │   │   ├── navigation/
│       │   │   └── dashboard/
│       │   ├── pages/
│       │   │   ├── auth/
│       │   │   ├── dashboard/
│       │   │   ├── users/
│       │   │   └── errors/
│       │   ├── layouts/
│       │   │   ├── MainLayout.tsx
│       │   │   └── AuthLayout.tsx
│       │   ├── services/
│       │   │   ├── api.ts
│       │   │   ├── authService.ts
│       │   │   ├── userService.ts
│       │   │   └── dashboardService.ts
│       │   ├── hooks/
│       │   ├── models/
│       │   ├── store/
│       │   ├── routes/
│       │   ├── utils/
│       │   ├── constants/
│       │   ├── types/
│       │   ├── App.tsx
│       │   ├── main.tsx
│       │   └── index.css
│       ├── public/
│       ├── package.json
│       ├── tsconfig.json
│       ├── tsconfig.app.json
│       ├── vite.config.ts
│       └── .env.example
│
├── database/
│   ├── scripts/
│   └── README.md
│
├── deployment/
│   ├── nginx/
│   │   └── nginx.conf
│   ├── docker/
│   └── jenkins/
│       └── Jenkinsfile
│
├── scripts/
│   ├── build-frontend.ps1
│   ├── build-frontend.sh
│   └── publish.sh
│
├── docker-compose.yml
├── .gitignore
├── README.md
└── LICENSE
```

---

# 3. Backend Technology

Use the latest stable supported **LTS ASP.NET Core/.NET version available in the development environment**.

Use:

- ASP.NET Core Web API
- C#
- Entity Framework Core
- JWT Authentication
- Dependency Injection
- REST API
- Swagger/OpenAPI
- Structured logging
- Global exception handling

Use nullable reference types.

Use asynchronous programming throughout the API.

Use `async/await` and `CancellationToken` where appropriate.

Do not block asynchronous calls.

---

# 4. Backend Architecture

Follow a clean and maintainable layered architecture:

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
EF Core DbContext
    ↓
Database
```

Controllers must remain thin.

Business logic belongs in services.

Database access belongs in repositories or the data layer.

Do not put business logic directly inside controllers.

Use interfaces for services and repositories where appropriate.

Examples:

```text
IAuthService
AuthService

IUserService
UserService

IUserRepository
UserRepository
```

Register all dependencies through ASP.NET Core Dependency Injection.

---

# 5. Database

Use Entity Framework Core.

Default database:

```text
SQL Server
```

Design the database layer so MySQL can be enabled through configuration.

Use:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  }
}
```

Never hardcode database credentials.

Support environment variables such as:

```text
ConnectionStrings__DefaultConnection
```

Create:

```text
StackPulseDbContext
```

Create entity configurations using:

```text
IEntityTypeConfiguration<T>
```

Use EF Core migrations.

---

# 6. Initial Database Entities

Create the initial entities:

```text
User
Role
RefreshToken
AuditLog
```

User should contain fields such as:

```text
Id
Username
Email
PasswordHash
FirstName
LastName
IsActive
CreatedAt
UpdatedAt
LastLoginAt
```

Do not store plain-text passwords.

Use secure password hashing.

Create proper primary keys, indexes, foreign keys, timestamps and constraints.

---

# 7. Authentication

Implement secure authentication using:

```text
JWT access token
+
Refresh token
```

Endpoints:

```text
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/logout
GET  /api/auth/me
```

Login should return:

```json
{
  "accessToken": "...",
  "refreshToken": "...",
  "expiresIn": 3600,
  "user": {
    "id": "...",
    "username": "...",
    "email": "..."
  }
}
```

Never return passwords.

Never log passwords or tokens.

JWT configuration must come from environment/configuration.

Do not commit secrets.

---

# 8. API Structure

All APIs must use:

```text
/api
```

Examples:

```text
/api/auth/login
/api/auth/refresh
/api/auth/logout

/api/users
/api/users/{id}

/api/dashboard
/api/dashboard/summary
```

Use proper HTTP status codes:

```text
200 OK
201 Created
204 No Content
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
422 Unprocessable Entity
500 Internal Server Error
```

Use consistent API response models.

Success:

```json
{
  "success": true,
  "message": "Request completed successfully",
  "data": {}
}
```

Error:

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": []
}
```

---

# 9. Global Exception Handling

Create a global exception middleware.

The API must not expose stack traces or sensitive internal information in production.

Return standardized error responses.

Log detailed exception information on the server.

Request flow:

```text
Request
   ↓
ExceptionHandlingMiddleware
   ↓
Controller
   ↓
Service
```

---

# 10. API Documentation

Enable Swagger/OpenAPI in development.

Document:

- Authentication
- Users
- Dashboard
- Error responses
- Request models
- Response models

Configure JWT authentication in Swagger so protected endpoints can be tested.

---

# 11. React Frontend

Use:

- React
- TypeScript
- Vite
- React Router
- Axios

Use functional components.

Use hooks.

Do not use class components.

Create reusable components.

Keep pages, components, services and models separated.

---

# 12. React SPA Routing

Create routes:

```text
/
/login
/dashboard
/users
/profile
/settings
/404
```

Protected routes:

```text
/dashboard
/users
/profile
/settings
```

Unauthenticated users should be redirected to:

```text
/login
```

Authenticated users should be able to access protected pages.

---

# 13. React API Service

Create a centralized Axios instance:

```text
src/services/api.ts
```

All API requests should go through this instance.

Use relative URLs:

```text
/api/...
```

Never hardcode production domain names inside application code.

Example:

```typescript
api.get("/api/users");
```

or configure Axios base URL as `/api` and use:

```typescript
api.get("/users");
```

Use Axios interceptors for:

- JWT token handling
- 401 handling
- Refresh token
- Global error handling

---

# 14. Local Development

During development:

```text
React/Vite:
http://localhost:5173

ASP.NET Core:
https://localhost:7001
```

Configure Vite proxy:

```text
/api
    ↓
https://localhost:7001
```

React code should use:

```text
/api/users
```

instead of:

```text
https://localhost:7001/api/users
```

Do not require CORS for normal local API requests if the Vite proxy handles them.

---

# 15. Production Frontend Build

React source must remain here:

```text
frontend/stackpulse-ui/
```

Never develop React directly inside:

```text
backend/StackPulse.Api/wwwroot/
```

Build React using:

```bash
npm run build
```

This produces:

```text
frontend/stackpulse-ui/dist/
```

Copy the generated contents into:

```text
backend/StackPulse.Api/wwwroot/
```

ASP.NET Core must serve these files.

---

# 16. Production Request Routing

Configure ASP.NET Core so:

```text
/api/*
```

always goes to ASP.NET Core controllers.

Static assets are served from:

```text
wwwroot
```

React routes fall back to:

```text
wwwroot/index.html
```

Examples:

```text
https://stackpulse.example.com/
        ↓
React

https://stackpulse.example.com/dashboard
        ↓
React

https://stackpulse.example.com/users
        ↓
React

https://stackpulse.example.com/api/users
        ↓
ASP.NET Core API
```

Never send `/api/*` to `index.html`.

---

# 17. ASP.NET Core Middleware Order

Configure middleware correctly for:

```text
Exception handling
HTTPS
Static files
Routing
Authentication
Authorization
Controllers
SPA fallback
```

Make sure API requests are processed before SPA fallback.

---

# 18. Frontend Authentication

Implement an authentication context/store providing:

```text
login()
logout()
refreshToken()
isAuthenticated
currentUser
```

Protect routes.

Handle expired access tokens.

Automatically attempt refresh where appropriate.

If refresh fails:

```text
clear authentication state
redirect to /login
```

Do not create infinite refresh loops.

---

# 19. Dashboard

Create an initial StackPulse dashboard containing:

```text
Header
Sidebar
Navigation
User profile
Dashboard cards
Recent activity
System status
Quick actions
```

Use sample API data initially.

Create a clean modern enterprise UI.

The UI must be responsive for:

```text
Desktop
Tablet
Mobile
```

Do not overcomplicate the first version.

---

# 20. Layout

Create:

```text
MainLayout
AuthLayout
```

Main layout:

```text
┌─────────────────────────────────────────────┐
│ StackPulse       Search       User Profile  │
├──────────┬──────────────────────────────────┤
│          │                                  │
│ Dashboard│                                  │
│ Users    │          Page Content            │
│ Reports  │                                  │
│ Settings │                                  │
│          │                                  │
└──────────┴──────────────────────────────────┘
```

Sidebar should support collapsed/expanded mode.

---

# 21. UI Components

Create reusable components:

```text
Button
Input
Modal
Dialog
Table
Pagination
Dropdown
Select
LoadingSpinner
EmptyState
ErrorState
ConfirmDialog
Toast
Card
Badge
Avatar
Sidebar
Header
```

Do not duplicate UI code unnecessarily.

---

# 22. Validation

Frontend:

Use a proper validation approach for forms.

Backend:

Validate all incoming DTOs.

Never trust frontend validation.

Backend validation is mandatory.

---

# 23. Security

Implement security best practices:

- Password hashing
- JWT expiration
- Refresh token rotation where appropriate
- Secure cookies if cookies are used
- HTTPS in production
- Input validation
- SQL injection protection through EF Core
- XSS protection
- CSRF considerations
- Rate limiting readiness
- Security headers
- No secrets in source control
- No sensitive information in logs
- Proper authorization
- Principle of least privilege

Do not store JWT secrets in source code.

---

# 24. Logging

Use ASP.NET Core logging.

Log:

```text
Request
Response status
Errors
Authentication events
Important application events
```

Do NOT log:

```text
Passwords
JWT tokens
Refresh tokens
Connection strings
Secrets
```

---

# 25. Docker

Create a multi-stage Docker build.

The final container must contain:

```text
ASP.NET Core application
+
compiled React SPA
```

Build process:

```text
Stage 1
Node
 ↓
npm install
 ↓
npm run build
 ↓
React dist

Stage 2
.NET SDK
 ↓
restore
 ↓
build
 ↓
publish

Stage 3
ASP.NET Core Runtime
 ↓
copy published backend
 ↓
copy React dist → wwwroot
 ↓
run StackPulse.Api
```

The final production image must not contain:

```text
node_modules
React source code
.NET SDK
development dependencies
```

Only production artifacts should remain.

---

# 26. Docker Compose

Create:

```text
docker-compose.yml
```

Initially support:

```text
stackpulse
database
```

Structure it so additional services can later be added:

```text
Redis
RabbitMQ
Elasticsearch
Monitoring
```

Do not expose the database publicly.

Use environment variables for credentials.

---

# 27. Nginx

Make the application Nginx-ready.

Production architecture:

```text
Internet
   │
   ▼
Nginx
   │
   ▼
StackPulse ASP.NET Core Container
   │
   ├── React SPA
   │
   └── /api/*
          │
          ▼
       Database
```

Nginx should handle:

- HTTPS
- HTTP → HTTPS redirect
- Reverse proxy
- Security headers
- Compression

The ASP.NET Core container should not need to expose the database.

---

# 28. Jenkins CI/CD

Create a Jenkinsfile.

Pipeline stages:

```text
Checkout
   ↓
Install Frontend Dependencies
   ↓
Frontend Lint
   ↓
Frontend Build
   ↓
Backend Restore
   ↓
Backend Build
   ↓
Backend Test
   ↓
Docker Build
   ↓
Docker Image Test
   ↓
Push Docker Image
   ↓
Deploy
   ↓
Health Check
```

The pipeline must fail if:

```text
Frontend build fails
Backend build fails
Tests fail
Docker build fails
Health check fails
```

Do not place passwords directly inside the Jenkinsfile.

Use Jenkins credentials.

---

# 29. Health Checks

Create:

```text
GET /api/health
```

Return:

```json
{
  "status": "Healthy"
}
```

Also configure ASP.NET Core health checks for database connectivity.

Docker should be able to use the health endpoint.

---

# 30. Environment Configuration

Support:

```text
Development
Staging
Production
```

Frontend must not contain production secrets.

Backend configuration must come from:

```text
appsettings.json
appsettings.Development.json
environment variables
Docker secrets/Jenkins credentials where appropriate
```

Create:

```text
.env.example
```

for frontend configuration.

---

# 31. CORS

For production, frontend and backend use the same origin:

```text
https://stackpulse.example.com
```

Therefore avoid unnecessary CORS configuration.

For development, Vite proxy should handle:

```text
/api
```

requests.

If CORS is required for development, make allowed origins configurable.

Never use `AllowAnyOrigin()` with credentials.

---

# 32. Database Migrations

Provide commands for:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Do not automatically destroy or recreate production databases.

Never use `EnsureDeleted()` in production.

---

# 33. Testing

Create backend unit tests.

Test:

```text
Authentication
User service
Validation
Exception handling
Dashboard service
```

Create API integration tests where practical.

Frontend should have a test-ready structure.

---

# 34. Error Pages

Create React pages:

```text
404 Not Found
403 Forbidden
500 Application Error
```

Display user-friendly messages.

Never display raw backend stack traces.

---

# 35. README

Create a comprehensive README containing:

```text
Project overview
Architecture
Prerequisites
Installation
Frontend development
Backend development
Database setup
EF migrations
Authentication
Environment variables
Docker development
Production Docker build
Nginx setup
Jenkins deployment
Testing
Troubleshooting
```

Include architecture diagrams using Mermaid where useful.

---

# 36. Git

Create a proper `.gitignore`.

Never commit:

```text
.env
.env.production
passwords
JWT secrets
database credentials
node_modules
bin/
obj/
dist/
user secrets
private keys
certificates
```

---

# 37. Code Quality

Follow:

- SOLID
- DRY
- Clean code
- Separation of concerns
- Dependency injection
- Strong typing
- Async programming
- Reusable components
- Meaningful naming
- Small focused methods
- Proper error handling
- No unnecessary abstraction
- No duplicated business logic

Avoid overengineering.

---

# 38. API Naming

Use RESTful naming.

Prefer:

```text
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
```

Avoid:

```text
/api/getUsers
/api/createUser
/api/deleteUser
```

---

# 39. Frontend API Models

Keep TypeScript models synchronized with API DTOs.

Example:

```typescript
export interface User {
    id: string;
    username: string;
    email: string;
    firstName: string;
    lastName: string;
    isActive: boolean;
}
```

Keep API response types strongly typed.

Avoid `any` unless absolutely necessary.

---

# 40. Initial Screens

Create:

```text
/login
/dashboard
/users
/users/create
/users/:id
/profile
/settings
/404
```

Login page:

```text
POST /api/auth/login
```

Dashboard:

```text
GET /api/dashboard
```

Users:

```text
GET /api/users
```

---

# 41. Important SPA Behavior

If a user directly opens:

```text
https://stackpulse.example.com/dashboard
```

the application must load correctly.

If the user refreshes:

```text
https://stackpulse.example.com/users
```

the application must load correctly.

ASP.NET Core must return:

```text
wwwroot/index.html
```

for React routes.

But:

```text
https://stackpulse.example.com/api/users
```

must always execute the API controller.

---

# 42. Build Automation

Create scripts so one command can build the complete application.

For example:

```bash
./scripts/publish.sh
```

The script should:

1. Install frontend dependencies.
2. Build React.
3. Clean `wwwroot`.
4. Copy React `dist` contents into `wwwroot`.
5. Restore .NET dependencies.
6. Build backend.
7. Publish backend.
8. Produce the production-ready application.

Provide equivalent PowerShell support for Windows.

---

# 43. Final Production Architecture

The final architecture must be:

```text
                         INTERNET
                            │
                            ▼
                         NGINX
                            │
                         HTTPS
                            │
                            ▼
                  ┌───────────────────┐
                  │ StackPulse.Api    │
                  │ ASP.NET Core      │
                  │                   │
                  │ ┌───────────────┐ │
                  │ │ React SPA     │ │
                  │ │ wwwroot       │ │
                  │ └───────────────┘ │
                  │                   │
                  │ ┌───────────────┐ │
                  │ │ REST API      │ │
                  │ │ /api/*        │ │
                  │ └───────┬───────┘ │
                  └─────────┼─────────┘
                            │
                            ▼
                     EF Core / Data
                            │
                            ▼
                         SQL DB
```

---

# 44. Critical Requirement

The most important requirement is:

```text
React source code
        ↓
npm run build
        ↓
dist/
        ↓
ASP.NET Core wwwroot/
        ↓
Single production application
        ↓
Single domain
```

Production must support:

```text
https://stackpulse.example.com/
https://stackpulse.example.com/login
https://stackpulse.example.com/dashboard
https://stackpulse.example.com/users
https://stackpulse.example.com/settings

https://stackpulse.example.com/api/auth/login
https://stackpulse.example.com/api/users
https://stackpulse.example.com/api/dashboard
```

All of these must work from the same domain.

---

# 45. Implementation Instructions

Do not only create empty folders or placeholder files.

Generate a **working initial application**.

The generated application must:

1. Start successfully.
2. Display the StackPulse login page.
3. Support login through the backend API.
4. Support JWT authentication.
5. Display the dashboard after login.
6. Display a users page.
7. Load users from the ASP.NET Core API.
8. Store data using Entity Framework Core.
9. Provide Swagger in development.
10. Provide a health endpoint.
11. Build React successfully.
12. Copy React build output into `wwwroot`.
13. Serve the React SPA from ASP.NET Core.
14. Serve `/api/*` through ASP.NET Core controllers.
15. Support React client-side routing.
16. Build successfully using Docker.
17. Be ready for Jenkins CI/CD.

Before finishing, verify:

```text
Frontend build → SUCCESS
Backend build → SUCCESS
Backend tests → SUCCESS
Docker build → SUCCESS
API health check → SUCCESS
React SPA → SUCCESS
API call from React → SUCCESS
React refresh on nested route → SUCCESS
Authentication flow → SUCCESS
```

If a technology/version has changed or a package is deprecated, use the current stable equivalent rather than generating obsolete configuration.

Do not silently omit required functionality.

Provide all required source files and configuration files necessary to run the application.
