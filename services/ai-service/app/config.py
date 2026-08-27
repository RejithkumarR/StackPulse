import os
from dataclasses import dataclass


@dataclass(frozen=True)
class Settings:
    ai_provider_url: str = os.getenv("AI_PROVIDER_URL", "")
    ai_provider_api_key: str = os.getenv("AI_PROVIDER_API_KEY", "")
    redis_url: str = os.getenv("REDIS_URL", "redis://localhost:6379/0")
    cache_ttl_seconds: int = int(os.getenv("AI_CACHE_TTL_SECONDS", "300"))


settings = Settings()
