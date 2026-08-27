from datetime import datetime, timezone
from pydantic import BaseModel, Field


class DashboardSummaryRequest(BaseModel):
    source_ids: list[str] = Field(default_factory=list, max_length=100)
    context: str = Field(min_length=1, max_length=20000)


class DashboardSummaryResponse(BaseModel):
    summary: str
    recommendations: list[str]
    source_ids: list[str]
    generated_at: datetime
    cached: bool = False
