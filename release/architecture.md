# StackPulse Architecture

This diagram reflects the currently implemented application topology. Install a Mermaid preview extension in VS Code to render it.

```mermaid
graph TD
    UI[StackPulse React UI\nVite development server :5173] -->|JWT HTTPS API calls| API[StackPulse ASP.NET Core API\n:5062]
    API --> MYSQL[(MySQL\nMaster configuration and identity)]
    API --> MONGO[(MongoDB\nTransactions, inventory, audit logs)]
    API --> SECRETS[AWS Secrets Manager\nConnection-string references]
    API -->|HTTP prompt and AI requests| AI[StackPulse AI Service\nFastAPI :8000]
    AI -->|Ollama chat and embeddings| OLLAMA[Ollama\nConfigured private endpoint]
    AI -->|Vector search| QDRANT[Qdrant\nConfigured collection]
    AI --> REDIS[(Redis\nAI response cache)]
    AI -->|Fetch active versioned prompts| API
    WORKER[StackPulse Inventory Windows Service\n.NET BackgroundService\n5 minute cycle] --> MYSQL
    WORKER --> MONGO
    WORKER --> SECRETS
    WORKER -->|Jira REST API| JIRA[Jira]
    WORKER -->|Bitbucket REST API| BITBUCKET[Bitbucket]
    WORKER -->|GitHub REST API| GITHUB[GitHub]
    WORKER -->|WMI services, Registry software, DriveInfo| HOST[Windows host]
    HOST --> WORKER
    API -->|Reads latest snapshots| MONGO
    UI --> API
```

## Current Data Flow

1. The Windows background worker runs every five minutes and runs integration synchronization only. It does not require a computer-master record.
2. The worker polls configured Jira, Bitbucket, and GitHub endpoints and writes completed integration snapshots to MongoDB, including successful empty results.
3. The API authenticates users, serves dashboard and integration data, reads MySQL master configuration, and reads MongoDB operational snapshots.
4. The AI service retrieves the active versioned prompt from the API, creates embeddings through Ollama, searches Qdrant, and generates structured analysis through Ollama. Redis caches dashboard responses.
5. The React UI calls the authenticated API and presents dashboard, integration, inventory, and configuration views.

## Worker Coverage

Implemented: Jira issue polling, Bitbucket open pull-request polling, GitHub open pull-request polling, MongoDB persistence, MySQL computer/integration lookup, `localhost` computer alias matching, and five-minute scheduling.

Not yet implemented: CPU, memory, process, network, ports, uptime, Confluence, GitHub Actions checks, GitHub security findings, vulnerability scanning, change detection, comment posting, notification delivery, and incident correlation.
