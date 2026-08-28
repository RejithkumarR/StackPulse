import hashlib
import json
import logging
import os
from datetime import datetime, timezone
from typing import Any

import httpx
from fastapi import FastAPI, Header, HTTPException
from redis.asyncio import Redis

from .config import settings
from .logging_config import configure_logging
from .models import DashboardSummaryRequest, DashboardSummaryResponse
from .redaction import redact_sensitive_data

configure_logging()
logger = logging.getLogger(__name__)
app = FastAPI(title="StackPulse AI Service", version="0.1.0")
redis_client = Redis.from_url(settings.redis_url, decode_responses=True)


def require_service_token(token: str | None) -> None:
    expected = os.getenv("AI_SERVICE_TOKEN", "")
    if expected and token != expected:
        raise HTTPException(status_code=401, detail="Invalid AI service token")


@app.get("/health")
async def health() -> dict[str, str]:
    logger.info("AI service health check")
    return {
        "status": "healthy",
        "ollama_configured": str(bool(settings.ollama_url and settings.ollama_model)).lower(),
        "vector_db_configured": str(bool(settings.vector_db_url)).lower(),
    }


def _auth_headers(token: str | None) -> dict[str, str]:
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    return headers


async def _get_prompt(client: httpx.AsyncClient, key: str) -> str:
    response = await client.get(
        f"{settings.prompt_service_url}/{key}",
        headers=_auth_headers(settings.prompt_service_token),
    )
    response.raise_for_status()
    prompt = response.json().get("data", {}).get("template")
    if not prompt:
        raise HTTPException(status_code=502, detail=f"Prompt '{key}' is unavailable")
    return prompt


async def _retrieve_context(client: httpx.AsyncClient, context: str) -> list[str]:
    embedding_response = await client.post(
        f"{settings.ollama_url}/embed",
        json={"model": settings.ollama_embed_model, "input": context},
        headers=_auth_headers(settings.ollama_token),
    )
    embedding_response.raise_for_status()
    embeddings = embedding_response.json().get("embeddings", [])
    if not embeddings:
        return []

    search_response = await client.post(
        f"{settings.vector_db_url}/collections/{settings.vector_db_collection}/points/search",
        json={"vector": embeddings[0], "limit": 5, "with_payload": True},
        headers=_auth_headers(settings.vector_db_token),
    )
    search_response.raise_for_status()
    matches = search_response.json().get("result", [])
    snippets: list[str] = []
    for match in matches:
        payload = match.get("payload") or {}
        snippet = payload.get("text") or payload.get("content") or payload.get("document")
        if snippet:
            snippets.append(str(snippet))
    return snippets


def _parse_generation(content: str) -> tuple[str, list[str]]:
    try:
        data = json.loads(content)
        return str(data["summary"]), [str(item) for item in data.get("recommendations", [])]
    except (KeyError, TypeError, ValueError):
        return content.strip(), []


async def _generate_analysis(prompt_key: str, context: str) -> dict[str, Any]:
    if not settings.ollama_url or not settings.ollama_model:
        logger.warning("AI provider is not configured")
        raise HTTPException(status_code=503, detail="Ollama is not configured")

    async with httpx.AsyncClient(timeout=60) as client:
        try:
            prompt_template = await _get_prompt(client, prompt_key)
        except httpx.HTTPError as error:
            raise HTTPException(status_code=502, detail="Prompt service request failed") from error

        retrieved_context = []
        if settings.vector_db_url:
            try:
                retrieved_context = await _retrieve_context(client, context)
            except httpx.HTTPError:
                retrieved_context = []

        prompt = prompt_template.replace("{{context}}", context)
        prompt = prompt.replace("{{retrieved_context}}", "\n\n".join(retrieved_context) or "[none]")
        try:
            ollama_response = await client.post(
                f"{settings.ollama_url}/chat",
                json={
                    "model": settings.ollama_model,
                    "messages": [{"role": "user", "content": prompt}],
                    "stream": False,
                    "format": "json",
                    "think": False,
                },
                headers=_auth_headers(settings.ollama_token),
            )
            ollama_response.raise_for_status()
        except httpx.HTTPError as error:
            raise HTTPException(status_code=502, detail="Ollama request failed") from error

    content = ollama_response.json().get("message", {}).get("content", "")
    try:
        result = json.loads(content)
    except (TypeError, ValueError) as error:
        raise HTTPException(status_code=502, detail="Ollama returned invalid JSON") from error
    if not isinstance(result, dict):
        raise HTTPException(status_code=502, detail="Ollama returned an invalid result")
    return result


@app.post("/ai/analyze", response_model=dict[str, Any])
async def analyze(
    request: DashboardSummaryRequest,
    x_service_token: str | None = Header(default=None),
) -> dict[str, Any]:
    require_service_token(x_service_token)
    sanitized_context = redact_sensitive_data(request.context)
    return await _generate_analysis(request.prompt_key, sanitized_context)


@app.post("/ai/dashboard-summary", response_model=DashboardSummaryResponse)
async def dashboard_summary(
    request: DashboardSummaryRequest,
    x_service_token: str | None = Header(default=None),
) -> DashboardSummaryResponse:
    require_service_token(x_service_token)
    if request.prompt_key != "dashboard_summary":
        raise HTTPException(status_code=400, detail="dashboard-summary requires the dashboard_summary prompt")
    sanitized_context = redact_sensitive_data(request.context)
    cache_key = "ai:dashboard-summary:" + hashlib.sha256(
        (request.prompt_key + "\n" + "\n".join(request.source_ids) + "\n" + sanitized_context).encode("utf-8")
    ).hexdigest()

    cached = await redis_client.get(cache_key)
    if cached:
        logger.info("Returning cached dashboard summary")
        response = DashboardSummaryResponse.model_validate_json(cached)
        response.cached = True
        return response

    analysis = await _generate_analysis(request.prompt_key, sanitized_context)
    summary = str(analysis.get("summary", ""))
    recommendations = [str(item) for item in analysis.get("recommendations", [])]

    if not summary:
        raise HTTPException(status_code=502, detail="Ollama returned an empty response")

    response = DashboardSummaryResponse(
        summary=summary,
        recommendations=recommendations,
        source_ids=request.source_ids,
        generated_at=datetime.now(timezone.utc),
    )
    await redis_client.set(cache_key, response.model_dump_json(), ex=settings.cache_ttl_seconds)
    return response
