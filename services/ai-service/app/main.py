import hashlib
import os
from datetime import datetime, timezone

import httpx
from fastapi import FastAPI, Header, HTTPException
from redis.asyncio import Redis

from .config import settings
from .models import DashboardSummaryRequest, DashboardSummaryResponse
from .redaction import redact_sensitive_data

app = FastAPI(title="StackPulse AI Service", version="0.1.0")
redis_client = Redis.from_url(settings.redis_url, decode_responses=True)


def require_service_token(token: str | None) -> None:
    expected = os.getenv("AI_SERVICE_TOKEN", "")
    if expected and token != expected:
        raise HTTPException(status_code=401, detail="Invalid AI service token")


@app.get("/health")
async def health() -> dict[str, str]:
    return {"status": "healthy", "provider_configured": str(bool(settings.ai_provider_url)).lower()}


@app.post("/ai/dashboard-summary", response_model=DashboardSummaryResponse)
async def dashboard_summary(
    request: DashboardSummaryRequest,
    x_service_token: str | None = Header(default=None),
) -> DashboardSummaryResponse:
    require_service_token(x_service_token)
    sanitized_context = redact_sensitive_data(request.context)
    cache_key = "ai:dashboard-summary:" + hashlib.sha256(
        sanitized_context.encode("utf-8")
    ).hexdigest()

    cached = await redis_client.get(cache_key)
    if cached:
        response = DashboardSummaryResponse.model_validate_json(cached)
        response.cached = True
        return response

    if not settings.ai_provider_url:
        raise HTTPException(status_code=503, detail="AI provider is not configured")

    headers = {"Content-Type": "application/json"}
    if settings.ai_provider_api_key:
        headers["Authorization"] = f"Bearer {settings.ai_provider_api_key}"

    payload = {"source_ids": request.source_ids, "context": sanitized_context}
    async with httpx.AsyncClient(timeout=30) as client:
        provider_response = await client.post(settings.ai_provider_url, json=payload, headers=headers)
        provider_response.raise_for_status()
        provider_data = provider_response.json()

    response = DashboardSummaryResponse(
        summary=provider_data["summary"],
        recommendations=provider_data.get("recommendations", []),
        source_ids=request.source_ids,
        generated_at=datetime.now(timezone.utc),
    )
    await redis_client.set(cache_key, response.model_dump_json(), ex=settings.cache_ttl_seconds)
    return response
