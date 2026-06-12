"""Tests de los caminos de extract_comparendo y carga de imagen (deps externas mockeadas)."""

from __future__ import annotations

import io
import sys
import types
from typing import Any

import pytest
from PIL import Image

from app.adapters.ml import tesseract_comparendo as tc


def _png_bytes() -> bytes:
    buffer = io.BytesIO()
    Image.new("RGB", (8, 8), color="white").save(buffer, format="PNG")
    return buffer.getvalue()


def test_extract_pdf_text_layer_sin_fitz(monkeypatch: pytest.MonkeyPatch) -> None:
    # Sin modulo `fitz` disponible, la funcion captura el ImportError y devuelve None.
    monkeypatch.setitem(sys.modules, "fitz", None)
    assert tc._extract_pdf_text_layer(b"%PDF-fake") is None


def test_extract_pdf_text_layer_con_fitz_mock(monkeypatch: pytest.MonkeyPatch) -> None:
    fake_fitz = types.ModuleType("fitz")

    class _Page:
        def get_text(self, _mode: str) -> str:
            return "Comparendo No. 11001000000012345678 Placa: ABC123"

    class _Doc:
        page_count = 1

        def __getitem__(self, _index: int) -> _Page:
            return _Page()

        def close(self) -> None:
            return None

    fake_fitz.open = lambda **_kwargs: _Doc()  # type: ignore[attr-defined]
    monkeypatch.setitem(sys.modules, "fitz", fake_fitz)

    text = tc._extract_pdf_text_layer(b"%PDF-fake")
    assert text is not None
    assert "ABC123" in text


def test_load_image_bytes_imagen_valida() -> None:
    image, error = tc._load_image_bytes(_png_bytes(), "image/png")
    assert image is not None
    assert error is None


def test_load_image_bytes_imagen_invalida() -> None:
    image, error = tc._load_image_bytes(b"no-soy-imagen", "image/png")
    assert image is None
    assert error == "Formato de imagen no reconocido."


def test_pdf_to_image_error_poppler(monkeypatch: pytest.MonkeyPatch) -> None:
    fake_pdf2image = types.ModuleType("pdf2image")

    def _boom(*_args: Any, **_kwargs: Any) -> Any:
        raise RuntimeError("poppler not installed")

    fake_pdf2image.convert_from_bytes = _boom  # type: ignore[attr-defined]
    monkeypatch.setitem(sys.modules, "pdf2image", fake_pdf2image)

    image, error = tc._pdf_to_image(b"%PDF-fake")
    assert image is None
    assert error is not None
    assert "Poppler" in error


def test_extract_comparendo_pdf_con_texto_embebido(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(
        tc,
        "_extract_pdf_text_layer",
        lambda _content: "Comparendo No. 11001000000012345678 Placa: ABC123 Valor: $ 390.000",
    )
    payload = tc.extract_comparendo(b"%PDF-fake", "application/pdf", "comparendo.pdf")
    assert payload["placa"] == "ABC123"
    assert payload["valor"] == 390000.0
    assert payload["confidence"] == 0.85


def test_extract_comparendo_pdf_sin_texto_ni_imagen(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(tc, "_extract_pdf_text_layer", lambda _content: None)
    monkeypatch.setattr(tc, "_pdf_to_image", lambda _content: (None, "Poppler ausente"))
    payload = tc.extract_comparendo(b"%PDF-fake", "application/pdf", "comparendo.pdf")
    assert payload["confidence"] == 0.0
    assert "comparendo.pdf" in payload["rawText"]


def test_extract_comparendo_imagen_con_ocr_mock(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(
        tc,
        "_extract_text",
        lambda _image: ("Placa: ABC123 Valor: $ 100.000", 0.75, None),
    )
    payload = tc.extract_comparendo(_png_bytes(), "image/png", "foto.png")
    assert payload["placa"] == "ABC123"
    assert payload["valor"] == 100000.0


def test_extract_comparendo_imagen_invalida() -> None:
    payload = tc.extract_comparendo(b"no-soy-imagen", "image/png", "foto.png")
    assert payload["confidence"] == 0.0
    assert "foto.png" in payload["rawText"]


def test_extract_comparendo_imagen_sin_texto(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(tc, "_extract_text", lambda _image: ("", 0.0, "sin texto"))
    payload = tc.extract_comparendo(_png_bytes(), "image/png", "foto.png")
    assert payload["confidence"] == 0.0
    assert "sin texto" in payload["rawText"]


def test_extract_text_sin_tesseract(monkeypatch: pytest.MonkeyPatch) -> None:
    # configure_ocr_runtime devuelve (None, None) => Tesseract no instalado.
    monkeypatch.setattr(tc, "configure_ocr_runtime", lambda: (None, None))
    image = Image.new("RGB", (8, 8), color="white")
    text, confidence, error = tc._extract_text(image)
    assert text == ""
    assert confidence == 0.0
    assert error is not None
