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

## Development notes

- Controllers and services are under `backend/StackPulse.Api/Controllers` and `backend/StackPulse.Api/Services`.
- DTOs live in `backend/StackPulse.Api/DTOs`.
- If you need to update DB schema, use your EF Core migration workflow against `backend/StackPulse.Api`.

## Contributing

Please follow the existing code style. Open a PR with a clear description and run both backend and frontend locally to verify changes.

## License

See the `LICENSE` file at the repository root.
