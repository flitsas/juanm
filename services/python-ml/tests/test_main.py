"""Tests del API FastAPI (health + endpoint OCR con imagen invalida)."""

from __future__ import annotations

import io

from fastapi.testclient import TestClient
from PIL import Image

from app.main import app

client = TestClient(app)


def test_health() -> None:
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json() == {"status": "ok"}


def test_ocr_comparendo_extract_imagen_invalida() -> None:
    response = client.post(
        "/ocr/comparendo:extract",
        files={"file": ("foto.png", b"no-soy-imagen", "image/png")},
    )
    assert response.status_code == 200
    body = response.json()
    assert body["confidence"] == 0.0
    assert "foto.png" in body["rawText"]


def test_ocr_comparendo_extract_imagen_valida_sin_ocr() -> None:
    buffer = io.BytesIO()
    Image.new("RGB", (8, 8), color="white").save(buffer, format="PNG")
    response = client.post(
        "/ocr/comparendo:extract",
        files={"file": ("blanco.png", buffer.getvalue(), "image/png")},
    )
    assert response.status_code == 200
    # Sin Tesseract en CI, devuelve resultado "no disponible" con confidence 0.
    assert response.json()["confidence"] in {0.0, 0.75}
