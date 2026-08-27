# StackPulse Implementation Status

## Purpose

This document records the StackPulse architecture, implemented behavior, configuration conventions, database contracts, verification status, and remaining work. It is the working technical reference for the current release.

## Product Scope

StackPulse is a React single-page application backed by an ASP.NET Core Web API. It provides one operational workspace for authentication, engineering integrations, machine inventory, audit activity, and configuration management.

The current implementation uses split storage:

- MySQL stores identity and master configuration data.
- MongoDB stores operational transactions, audit records, and application logs.
- AWS Secrets Manager stores backend connection-string secrets.
- React and TypeScript provide the browser experience.
- The inventory service collects machine information independently from the API.

## Repository Components

### Backend API

Path: `backend/StackPulse.Api`

The API contains:

- Authentication endpoints for login, signup, forgot password, refresh token, and logout.
- JWT access-token generation and refresh-token persistence.
- User and dashboard services.
- Computer-master and integration-access configuration APIs.
- MongoDB-backed audit, application-log, inventory, and integration-sync access.
- AWS Secrets Manager connection-string resolution.
- Global exception handling and request logging middleware.
- Swagger/OpenAPI in development.
- Health endpoint at `/api/health`.

### Inventory Service

Path: `backend/StackPulse.InventoryService`

The worker is responsible for collecting Windows, Linux, and macOS machine information. It can collect services, installed software, and drive information and is intended to write operational transaction data to MongoDB while using MySQL master records for relationships.

### Frontend SPA

Path: `frontend/stackpulse-ui`

The frontend contains:

- Lock/unlock authentication entry screen.
- Login, signup, and forgot-password modes.
- Dashboard, users, profile, settings, inventory, and error views.
- Axios API service integration.
- Global top-right toaster notifications.
- Computer-master and integration-access forms.
- Responsive StackPulse visual theme.

## Implemented and Verified

The following items are complete for the current release and have been verified during development:

### AWS Secrets Manager

- The API can load MySQL and MongoDB connection strings from one AWS Secrets Manager secret.
- Configuration is controlled by `AwsSecretsManager:Enabled`, `AwsSecretsManager:Region`, and `AwsSecretsManager:SecretName`.
- Local AWS credentials are expected through the AWS SDK default credential chain, such as `%USERPROFILE%\\.aws\\credentials` or `AWS_PROFILE`.
- AWS access keys must not be stored in the repository, frontend bundle, spreadsheet, or `appsettings.json`.
- Production workloads should use an IAM role or instance profile instead of long-lived access keys.

Expected secret shape:

```json
{
  "mysqlConnectionString": "server=HOST;database=stackpulse_master;user=USER;password=PASSWORD;",
  "mongoConnectionString": "mongodb://USER:PASSWORD@HOST:27017/stackpulse?authSource=admin"
}
```

The MongoDB URI must use one `?`. Additional options use `&`, for example `?authSource=admin&retryWrites=true&w=majority`.

### User Creation

- Signup is implemented at `POST /api/auth/signup`.
- Duplicate username and email values are rejected.
- Passwords are stored as BCrypt hashes.
- New users receive the `User` role.
- A JWT access token and refresh token are returned after successful creation.
- Refresh tokens are persisted in MySQL.

### Login

- Login is implemented at `POST /api/auth/login`.
- Users can log in with username or email.
- Inactive users are rejected.
- JWT issuer, audience, lifetime, and signing key are configured through `JwtSettings`.
- Refresh and logout flows revoke tokens through the schema-backed `revoked_at` column.

### Toaster Alerts

- Authentication success and failure feedback is displayed through the global toaster.
- The toaster is mounted by `ToastProvider` at the application root.
- Alerts appear in the top-right corner and expire automatically.
- The auth form no longer renders a duplicate inline alert below the submit button.
- Settings save operations also use toaster feedback.

### Computer Master Saving

- Computer-master records can be created and updated through the settings workflow.
- Success and failure states are displayed through toaster notifications.
- MySQL stores computer-master records in `computer_masters`.

## Database Contracts

### MySQL Database

Database name: `stackpulse_master`

The schema uses lowercase table names and snake_case columns. EF Core mappings explicitly match these names.

Tables:

- `roles`
- `users`
- `refresh_tokens`
- `menus`
- `role_accesses`
- `computer_masters`
- `integration_accesses`

Important mapping rules:

- `users.password_hash` maps to `User.PasswordHash`.
- `users.role_id` maps to `User.RoleId`.
- `refresh_tokens.user_id` maps to `RefreshToken.UserId`.
- `refresh_tokens.revoked_at` is the source of truth for revocation.
- `RefreshToken.IsRevoked` is a calculated, non-persisted property.
- `refresh_tokens.token` is `VARCHAR(512)` because it has a unique index.

For an existing database previously created with a `LONGTEXT` token column, run:

```sql
ALTER TABLE refresh_tokens
  MODIFY COLUMN token VARCHAR(512) NOT NULL;
```

The schema and seed files are:

- `database/mysql/stackpulse_master_schema.sql`
- `database/mysql/stackpulse_master_seed.sql`

The seed script creates the default `Admin` and `User` roles and default menus.

### MongoDB Database

Database name: `stackpulse`

Collections:

- `audit_logs` for audit activity.
- `stackpulse_logs` for request and exception application logs.
- `transactions` for machine inventory and integration synchronization transactions.

The API maps these collections through `MongoDbSettings`:

```json
{
  "DatabaseName": "stackpulse",
  "AuditCollection": "audit_logs",
  "ApplicationLogCollection": "stackpulse_logs",
  "MachineInventoryCollection": "transactions",
  "IntegrationSyncCollection": "transactions"
}
```

## Local Development

### Backend

From the repository root:

```powershell
dotnet restore
dotnet build .\StackPulse.sln
dotnet run --project .\backend\StackPulse.Api
```

The API uses AWS Secrets Manager when enabled. For local development, configure an AWS profile outside the repository:

```powershell
aws configure --profile stackpulse-local
$env:AWS_PROFILE = "stackpulse-local"
```

The AWS CLI is optional when credentials are supplied through another supported AWS SDK credential source.

### Frontend

```powershell
Set-Location .\frontend\stackpulse-ui
npm install
npm run dev
```

Production build:

```powershell
npm run build
```

The Vite development proxy forwards `/api` requests to the API, normally at `http://localhost:5062`.

## Security Rules

- Never put AWS access keys, MongoDB passwords, MySQL passwords, JWT production keys, or integration tokens in the frontend.
- Never commit local AWS credentials or secret files.
- Rotate credentials that were exposed in chat, source files, spreadsheets, or logs.
- Store integration credentials in AWS Secrets Manager and store only a secret reference in MySQL.
- Use least-privilege IAM permissions for the API and inventory service.
- Use HTTPS for API and database connections in deployed environments.
- Keep the generic error response for clients and retain detailed exception information only in protected server logs.

## Next Implementation Phase

### Jira Integration

- Load Jira connection details from the integration-access master record.
- Resolve the referenced secret from AWS Secrets Manager.
- Authenticate to Jira without exposing credentials to React.
- Fetch issues using a service-owned client.
- Persist synchronization results in MongoDB `transactions`.
- Store master integration IDs and synchronization timestamps with each transaction.
- Add retry, timeout, rate-limit, and failure logging behavior.
- Add an API endpoint and dashboard view for the latest Jira synchronization state.

### Utility Access Tokens

The same pattern should be used for Bitbucket, Confluence, CI/CD, DevOps, Webex, Teams, email, and other utility integrations:

1. Save non-secret integration metadata in MySQL `integration_accesses`.
2. Save only an AWS Secrets Manager reference in `secret_reference`.
3. Resolve the secret in the backend or worker.
4. Use the token server-side for the provider API call.
5. Never return the token to the frontend.
6. Persist only safe metadata and provider results in MongoDB.

### Monitoring

- Add scheduled Jira and utility synchronization jobs.
- Record start time, completion time, status, item count, and error details in `transactions`.
- Add health checks for MySQL, MongoDB, AWS Secrets Manager, and provider connectivity.
- Add dashboard cards for last successful sync, current status, and failures.
- Add alerting for repeated failures, expired tokens, and stale synchronization timestamps.
- Add structured correlation IDs across API requests, worker jobs, and provider calls.

## Verification Checklist

- [x] API builds successfully.
- [x] Frontend TypeScript and Vite production build succeeds.
- [x] AWS Secrets Manager resolver is wired into API startup.
- [x] User signup is connected to the API.
- [x] Login is connected to the API.
- [x] Auth feedback uses the top-right toaster.
- [x] Computer-master save feedback uses the toaster.
- [x] EF table mappings use lowercase MySQL names.
- [x] EF column mappings use snake_case MySQL names.
- [x] Refresh-token persistence matches `user_id` and `revoked_at`.
- [ ] Jira synchronization is implemented and monitored.
- [ ] Utility access-token resolution is implemented for all providers.
- [ ] Scheduled monitoring and provider failure alerting are implemented.

## Current Known Constraints

- The checked-in MySQL schema does not include an `audit_logs` table; audit logging falls back to MySQL only when the corresponding table exists and MongoDB is unavailable.
- MongoDB authentication depends on the correct user, password, and `authSource` in the secret connection string.
- `Database.Migrate()` cannot create the supplied SQL schema because the project does not currently contain EF migrations; apply the SQL schema and seed scripts explicitly.
- The API should be restarted after changing AWS secret values or local configuration.
