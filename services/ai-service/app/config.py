import os
import binascii
from dataclasses import dataclass, field

from dotenv import load_dotenv

from .crypto import AESCrypto


load_dotenv(os.path.join(os.path.dirname(__file__), ".env"))


def _decrypt_env(name: str) -> str | None:
    value = os.getenv(name)
    if not value:
        return None

    try:
        return AESCrypto().decrypt(value)
    except (ValueError, TypeError, UnicodeDecodeError, binascii.Error):
        return value


def _ollama_models() -> frozenset[str]:
    configured = os.getenv("OLLAMA_MODELS", "")
    return frozenset(model.strip() for model in configured.split(",") if model.strip())


@dataclass(frozen=True)
class Settings:
    ollama_url: str = os.getenv("OLLAMA_URL", "http://localhost:11434").rstrip("/")
    ollama_token: str | None = field(default_factory=lambda: _decrypt_env("OLLAMA_TOKEN"))
    ollama_model: str = os.getenv("OLLAMA_MODEL", "qwen3-coder:30b")
    ollama_models: frozenset = field(default_factory=_ollama_models)
    ollama_embed_model: str = os.getenv("OLLAMA_EMBED_MODEL", "nomic-embed-text")
    redis_url: str = os.getenv("REDIS_URL", "redis://localhost:6379/0")
    cache_ttl_seconds: int = int(os.getenv("AI_CACHE_TTL_SECONDS", "300"))
    prompt_service_url: str = os.getenv("PROMPT_SERVICE_URL", "http://localhost:5062/api/ai/prompts").rstrip("/")
    prompt_service_token: str | None = field(default_factory=lambda: _decrypt_env("AI_SERVICE_TOKEN"))

    # --- Vector DB ---
    vector_db_url: str = os.getenv("VECTOR_DB_URL", "http://localhost:6333").rstrip("/")
    vector_db_token: str | None = field(default_factory=lambda: _decrypt_env("VECTOR_DB_TOKEN"))
    vector_db_collection: str = os.getenv("VECTOR_DB_COLLECTION", "stackpulse")

settings = Settings()
