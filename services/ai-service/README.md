# StackPulse AI Service

Private FastAPI service for read-only AI summaries and recommendations. It uses Ollama for generation and embeddings, with Qdrant as the vector store. The ASP.NET Core API should authenticate the user, collect authorized data, redact secrets, and call this service over an internal network.

## Run locally

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
$env:REDIS_URL = "redis://localhost:6379/0"
$env:PROMPT_SERVICE_URL = "http://localhost:5062/api/ai/prompts"
uvicorn app.main:app --reload --port 8000
```

The `prompt_key` in the request selects the active, highest-version prompt from the backend database. Supported seeded keys are `dashboard_summary`, `jira_validate`, `bitbucket_review`, `inventory_risk`, and `notification_decision`.

Use `POST /ai/analyze` for the non-dashboard workflows. Use `POST /ai/dashboard-summary` only with `prompt_key: "dashboard_summary"`; it maps the AI JSON into the stable dashboard response contract.

## Service contract

The configured provider endpoint receives:

```json
{
  "source_ids": ["JIRA-101"],
  "context": "Sanitized operational context"
}
```

The service returns:

```json
{
  "summary": "A concise operational summary.",
  "recommendations": ["Review the overdue issue"]
}
```

## Security

- Do not send credentials, tokens, passwords, connection strings, or JWT keys.
- Configure `OLLAMA_TOKEN` and `VECTOR_DB_TOKEN` through a secret store, not source control. A service-local `.env` is loaded for local development.
- Set `AI_SERVICE_TOKEN` for the ASP.NET Core to Python service call.
- Keep this service private; the browser must call the ASP.NET Core API only.
