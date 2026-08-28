# StackPulse

**StackPulse - One pulse for your entire engineering workflow.**

## Overview

StackPulse is a centralized developer productivity and engineering operations portal that brings important work information into a single, unified dashboard.

It integrates services such as Jira, Bitbucket, Confluence, CI/CD, DevOps support, Webex, Microsoft Teams, and email so users can quickly see their assigned tasks, pull requests, mentions, comments, notifications, conversations, documentation, and support activities without switching between multiple applications.

The platform is designed as a React-based single-page application (SPA) with an ASP.NET Core Web API backend, providing a scalable, secure, modular, and Docker-ready architecture.

## Repository Description

StackPulse is a centralized engineering dashboard that unifies Jira, Bitbucket, Confluence, DevOps, collaboration, and communication activities into a single React SPA powered by ASP.NET Core.

## Key Capabilities

- Unified dashboard for engineering work, tasks, and notifications.
- Integration-ready architecture for Jira, Bitbucket, Confluence, CI/CD, DevOps support, collaboration tools, and email.
- React SPA frontend built for a responsive developer portal experience.
- ASP.NET Core Web API backend designed for modular services and secure APIs.
- Docker-ready project structure for local development and deployment workflows.

## Current implementation

- MySQL stores authentication and master data, including users, roles, refresh tokens, menus, access mappings, computer masters, and integration access.
- MongoDB stores audit logs, application logs, machine inventory transactions, and integration sync transactions.
- Authentication is connected to the backend API with login, signup, forgot-password, JWT access tokens, and refresh-token persistence.
- Master configuration APIs and frontend screens support computer records and Jira, Confluence, and Bitbucket access settings.
- The inventory worker is hosted as a Windows Service and currently runs integration synchronization only. It polls Jira, Bitbucket, and GitHub every five minutes and writes a completed snapshot to MongoDB, including empty successful results. It does not collect computer inventory or require a computer-master record in this mode.
- AWS Secrets Manager integration has been verified for providing MySQL and MongoDB connection strings through a single application secret.
- User creation, login, refresh-token persistence, and logout are implemented and connected to the frontend.
- Authentication and settings feedback uses a global top-right toaster alert; signup no longer renders a duplicate inline message.
- Computer-master saving is implemented with success and failure toaster feedback.
- The login experience includes an unlock screen, login/signup/forgot-password tabs, and a full-page background image while keeping the authentication form on its card surface.

## Repository layout

- `backend/StackPulse.Api` - .NET Web API
- `frontend/stackpulse-ui` - Vite + React SPA
- `database/` - database-related files
- `deployment/` - deployment scripts and configs

## Prerequisites

- .NET 8 SDK (or the version used by the solution)
- Node.js 18+ and npm or yarn
- A running database (configure connection string in the backend appsettings)

## Backend (API)

From the repository root:

```bash
dotnet restore
dotnet build
dotnet run --project backend/StackPulse.Api
```

Configuration files:

- backend/StackPulse.Api/appsettings.json
- backend/StackPulse.Api/appsettings.Development.json
- backend/StackPulse.Api/Configuration/DatabaseSettings.cs
- backend/StackPulse.Api/Configuration/JwtSettings.cs

Set the database connection string and JWT secrets before running locally.

Start the inventory worker in a second terminal from the repository root:

```powershell
dotnet run --project .\backend\StackPulse.InventoryService
```

The worker runs immediately and then every five minutes. On Windows it collects WMI services, registry-installed software, and drives, writes snapshots to MongoDB, and polls Jira and Bitbucket when their worker settings are configured.

## Frontend (SPA)

```bash
cd frontend/stackpulse-ui
npm install
npm run dev
```

Build for production:

```bash
npm run build
```

The frontend calls the API via code in `frontend/stackpulse-ui/src/services/api.ts`.

During local development, Vite proxies `/api` requests to `http://localhost:5062` by default.

The frontend includes a light StackPulse theme, dashboard navigation, master configuration screens, system inventory display, and a footer in the main application layout.

Open `http://localhost:5173` after starting Vite. The API uses `http://localhost:5062` by default.

## Storage and secrets

Set the MySQL and MongoDB connection strings before running the backend. AWS Secrets Manager is configured through `AwsSecretsManager` and can provide both values from one secret:

```json
{
	"mysqlConnectionString": "server=mysql-host;database=stackpulse_master;user=stackpulse;password=change-me;",
	"mongoConnectionString": "mongodb://user:password@mongo-host:27017/stackpulse?authSource=admin"
}
```

The MongoDB database is `stackpulse`. The configured collections are `audit_logs`, `stackpulse_logs`, and `transactions`. Use one `?` in a MongoDB URI; additional options are separated with `&`.

For local AWS credentials, use the standard AWS profile outside the repository:

```powershell
aws configure --profile stackpulse-local
$env:AWS_PROFILE = "stackpulse-local"
```

Never place AWS keys, database passwords, JWT production keys, or provider access tokens in the frontend or source control.

See `deployment/inventory-service-configuration.md` for inventory worker hosting and AWS deployment guidance.

For the current worker integration fetchers, configure `Jira:BaseUrl`, `Jira:Username`, `Jira:ApiToken`, and `Jira:Jql`, plus `Bitbucket:BaseUrl`, `Bitbucket:Username`, `Bitbucket:AppPassword`, and `Bitbucket:Workspace` in the worker's environment or `appsettings.json`. Do not commit real credentials. The Settings page stores integration metadata and secret references, but the worker does not yet resolve those references for Jira/Bitbucket polling.

GitHub pull-request synchronization uses a fine-grained token with read access to pull requests and repository metadata. Configure `GitHub:BaseUrl`, `GitHub:Token`, and a comma-separated `GitHub:Repositories` value such as `owner/repository-one,owner/repository-two`. Keep the token in the server environment or secret store.

## Development notes

- Controllers and services are under `backend/StackPulse.Api/Controllers` and `backend/StackPulse.Api/Services`.
- DTOs live in `backend/StackPulse.Api/DTOs`.
- MySQL schema and seed scripts live under `database/mysql`; the current SQL contract uses lowercase snake_case names.
- Detailed implementation status and the next Jira/utility-token monitoring phase are documented in `release/implementation-status.md`.
- GitHub pull-request synchronization is implemented when `GitHub:Token` and `GitHub:Repositories` are configured. The next GitHub items are Actions checks, security findings, change detection, comment posting, and AI review automation. Other next items are Confluence/CI connectors, worker-side integration-secret resolution, notifications, and AI incident correlation.

## Contributing

Please follow the existing code style. Open a PR with a clear description and run both backend and frontend locally to verify changes.

## License

See the `LICENSE` file at the repository root.
