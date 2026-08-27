import re


_SECRET_PATTERNS = (
    re.compile(r"(?i)(password|secret|token|api[_-]?key|access[_-]?key)\s*[:=]\s*[^\s,;]+"),
    re.compile(r"(?i)mongodb(?:\+srv)?://[^\s]+"),
    re.compile(r"(?i)(?:aws_access_key_id|aws_secret_access_key)\s*=\s*[^\s]+"),
)


def redact_sensitive_data(value: str) -> str:
    result = value
    for pattern in _SECRET_PATTERNS:
        result = pattern.sub("[REDACTED]", result)
    return result
