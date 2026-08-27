# StackPulse Technical Release Notes

## Release Summary

This release updates StackPulse into a split-storage operations platform with MySQL for authentication/master data and MongoDB for transaction, audit, inventory, and application log data. AWS Secrets Manager integration, user creation, login, toaster feedback, and computer-master saving have been verified. The next phase is Jira and utility integration-token resolution with synchronization monitoring.

## Backend API

- Configured MySQL as the primary relational provider for authentication and master data.
- Added and verified AWS Secrets Manager integration using one application secret for MySQL and MongoDB connection strings.
- Added MongoDB connection support for audit logs, application logs, machine inventory transactions, and integration sync transactions.
- Added master configuration APIs under `api/master-configuration`.
- Added support for computer master configuration.
- Added support for Jira, Confluence, and Bitbucket access configuration.
- Updated dashboard activity and audit counts to read from MongoDB when configured.
- Updated inventory and integration read APIs to use MongoDB first, with EF fallback for local development.
- Added and verified real signup, login, refresh-token, logout, and forgot-password flows.
- Fixed local development authentication behavior when using EF InMemory fallback.
- Added JWT defaults for local development.
- Added explicit lowercase table and snake_case column mappings for the MySQL schema.
- Updated refresh-token persistence to use `user_id` and `revoked_at`.

## Authentication

- Login now uses the backend `/api/auth/login` endpoint.
- Signup now uses the backend `/api/auth/signup` endpoint.
- Forgot password now uses the backend `/api/auth/forgot-password` endpoint.
- Demo token authentication has been removed.
- Unauthenticated users remain on the login screen.
- Authenticated users are routed to the dashboard.
- Refresh token persistence is configured through the backend.
- User creation and login were verified through the frontend/API flow.

## Database Design

### MySQL

MySQL stores authentication and master data:

- Users
- Roles
- Refresh tokens
- Menus
- Role access mappings
- Computer masters
- Integration access masters

SQL scripts were added under:

- `database/mysql/stackpulse_master_schema.sql`
- `database/mysql/stackpulse_master_seed.sql`

### MongoDB

MongoDB stores transaction and operational data:

- Audit logs
- Application logs
- Machine inventory and integration sync transactions in `transactions`
- Application request and exception logs in `stackpulse_logs`

MongoDB schema/example files were added under:

- `database/mongodb/audit_logs.schema.json`
- `database/mongodb/application_logs.schema.json`
- `database/mongodb/transactions.example.json`

MongoDB documents include MySQL master IDs where applicable, such as:

- `masterComputerId`
- `masterIntegrationAccessId`
- `masterUserId`
- `masterEntityId`

The configured MongoDB database is `stackpulse`, with collections `audit_logs`, `stackpulse_logs`, and `transactions`.

## AWS Secrets Manager

The application now uses one AWS Secrets Manager secret for all application connection strings.

Expected secret JSON:

```json
{
  "mysqlConnectionString": "server=mysql-host;database=stackpulse_master;user=stackpulse;password=change-me;",
  "mongoConnectionString": "mongodb://user:password@mongo-host:27017/stackpulse?authSource=admin"
}
```

Application configuration:

```json
{
  "AwsSecretsManager": {
    "Enabled": true,
    "Region": "us-east-1",
    "SecretName": "stackpulse/app/config"
  }
}
```

### EC2 Configuration

- EC2 should use an IAM role / instance profile.
- The role needs `secretsmanager:GetSecretValue` permission for the StackPulse secret.
- Access keys should not be stored on the EC2 instance.

### Datacenter / On-Premise Configuration

Datacenter servers can use the AWS SDK default credential chain:

- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- Optional `AWS_SESSION_TOKEN`
- Shared AWS credentials file
- `AWS_PROFILE`

If AWS Secrets Manager is not reachable from the datacenter, set `AwsSecretsManager:Enabled` to `false` and inject connection strings through local secret storage, environment variables, or deployment tooling.

Detailed deployment guidance was added at:

- `deployment/inventory-service-configuration.md`

## Inventory Worker

The inventory worker owns bulk data collection and transaction writes.

- Reads MySQL master rows for relation IDs.
- Writes bulk inventory and integration transactions to MongoDB.
- Does not bulk-write transaction data into MySQL.
- Supports Windows Service hosting.
- Supports Linux systemd hosting.
- Supports console or launchd-managed execution on macOS.

### Windows Collection

- WMI service data
- Registry installed software
- Drive information

### Linux Collection

- systemd service data
- dpkg package metadata when available
- Drive information

### macOS Collection

- launchd service data
- Application bundle inventory
- Drive information

## Frontend

- Updated the visual design to a white/light application theme.
- Header and footer use `#0987c0`.
- Header and footer text uses white.
- Added a footer to the main layout.
- Updated sidebar, cards, forms, tables, and login views to match the light theme.
- Added master configuration UI for computers and integration access details.
- Verified computer-master saving with success and failure toaster feedback.
- Added a global top-right toaster for authentication and settings feedback.
- Fixed Vite API proxy configuration.
- Default frontend dev proxy target is `http://localhost:5062`.

## Login Experience

- Login screen initially shows a lock screen.
- Clicking the lock icon reveals the authentication card.
- Unlock icon is shown in the login card.
- Login, signup, and forgot-password are available as tabs.
- Added animated water/ripple background effect.

## Verification

The following checks were completed successfully:

```bash
dotnet build StackPulse.sln
npm run build
```

Runtime checks completed successfully:

- API health endpoint returned `200`.
- Frontend Vite proxy to `/api/health` returned `200`.
- Backend signup endpoint returned `200`.
- Backend login endpoint returned `200`.
- Backend forgot-password endpoint returned `200`.
- AWS Secrets Manager integration was verified for backend secret resolution.
- User creation and login were verified.
- Computer-master saving was verified.
- Signup no longer displays a duplicate inline alert; feedback is shown in the top-right toaster.

## Next Phase

- Implement Jira API synchronization using server-side credentials resolved from AWS Secrets Manager.
- Implement utility access-token saving through secret references, not plaintext tokens in MySQL or React.
- Apply the same pattern to Bitbucket, Confluence, CI/CD, DevOps, Webex, Teams, email, and other providers.
- Write synchronization results to MongoDB `transactions` with status, timestamps, counts, and safe error metadata.
- Add scheduled monitoring, provider health checks, stale-sync detection, token-expiry checks, retries, and dashboard alerts.

Detailed implementation, database contracts, security rules, and the verification checklist are documented in `release/implementation-status.md`.

Development URLs:

- API: `http://localhost:5062`
- Frontend: `http://127.0.0.1:5173/`
