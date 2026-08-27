# StackPulse AI Service

Private FastAPI service for read-only AI summaries and recommendations. The ASP.NET Core API should authenticate the user, collect authorized data, redact secrets, and call this service over an internal network.

## Run locally

```powershell
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
$env:REDIS_URL = "redis://localhost:6379/0"
$env:AI_PROVIDER_URL = "https://your-provider.example/v1/summarize"
uvicorn app.main:app --reload --port 8000
```

## Provider contract

The configured provider endpoint receives:

```json
{
  "source_ids": ["JIRA-101"],
  "context": "Sanitized operational context"
}
```

It must return:

```json
{
  "summary": "A concise operational summary.",
  "recommendations": ["Review the overdue issue"]
}
```

## Security

- Do not send credentials, tokens, passwords, connection strings, or JWT keys.
- Configure `AI_PROVIDER_API_KEY` through a secret store, not source control.
- Set `AI_SERVICE_TOKEN` for the ASP.NET Core to Python service call.
- Keep this service private; the browser must call the ASP.NET Core API only.
