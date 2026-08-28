import logging
import os
from datetime import datetime, timezone
from logging.handlers import RotatingFileHandler
from pathlib import Path


def configure_logging() -> None:
    configured_root = os.getenv("STACKPULSE_LOG_ROOT")
    service_root = Path(configured_root) if configured_root else Path(__file__).parents[3] / "logs"
    date_folder = datetime.now(timezone.utc).strftime("%Y-%m-%d")
    log_folder = service_root / "ai-service" / date_folder
    log_folder.mkdir(parents=True, exist_ok=True)

    formatter = logging.Formatter(
        "%(asctime)s|%(levelname)s|%(name)s|%(message)s",
        datefmt="%Y-%m-%dT%H:%M:%S%z",
    )
    file_handler = RotatingFileHandler(
        log_folder / f"ai-{os.getpid()}.log",
        maxBytes=10 * 1024 * 1024,
        backupCount=14,
        encoding="utf-8",
    )
    file_handler.setFormatter(formatter)
    stream_handler = logging.StreamHandler()
    stream_handler.setFormatter(formatter)

    root_logger = logging.getLogger()
    root_logger.setLevel(logging.INFO)
    root_logger.handlers.clear()
    root_logger.addHandler(file_handler)
    root_logger.addHandler(stream_handler)
