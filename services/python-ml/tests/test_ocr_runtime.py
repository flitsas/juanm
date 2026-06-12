"""Tests de la resolucion de binarios OCR (Tesseract/Poppler/tessdata)."""

from __future__ import annotations

from pathlib import Path

import pytest

from app.adapters.ml import ocr_runtime


def test_first_existing(tmp_path: Path) -> None:
    existing = tmp_path / "bin"
    existing.write_text("x")
    assert ocr_runtime._first_existing(["", str(existing)]) == str(existing)
    assert ocr_runtime._first_existing(["", str(tmp_path / "missing")]) is None


def test_resolve_tesseract_cmd_desde_env(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    binary = tmp_path / "tesseract"
    binary.write_text("x")
    monkeypatch.setenv("TESSERACT_CMD", str(binary))
    assert ocr_runtime.resolve_tesseract_cmd() == str(binary)


def test_resolve_tesseract_cmd_desde_which(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.delenv("TESSERACT_CMD", raising=False)
    monkeypatch.setattr(ocr_runtime.shutil, "which", lambda _name: "/usr/bin/tesseract")
    assert ocr_runtime.resolve_tesseract_cmd() == "/usr/bin/tesseract"


def test_resolve_tesseract_cmd_no_encontrado(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.delenv("TESSERACT_CMD", raising=False)
    monkeypatch.setattr(ocr_runtime.shutil, "which", lambda _name: None)
    assert ocr_runtime.resolve_tesseract_cmd() is None


def test_resolve_poppler_path_desde_which(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.delenv("POPPLER_PATH", raising=False)
    found = str(Path("/usr/bin/pdftoppm"))
    monkeypatch.setattr(ocr_runtime.shutil, "which", lambda _name: found)
    assert ocr_runtime.resolve_poppler_path() == str(Path(found).parent)


def test_resolve_poppler_path_no_encontrado(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.delenv("POPPLER_PATH", raising=False)
    monkeypatch.setattr(ocr_runtime.shutil, "which", lambda _name: None)
    monkeypatch.setattr(ocr_runtime.Path, "home", staticmethod(lambda: Path("/nonexistent-home")))
    assert ocr_runtime.resolve_poppler_path() is None


def test_resolve_tessdata_prefix_desde_env(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setenv("TESSDATA_PREFIX", str(tmp_path))
    assert ocr_runtime.resolve_tessdata_prefix() == str(tmp_path)


def test_resolve_tessdata_prefix_no_encontrado(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.delenv("TESSDATA_PREFIX", raising=False)
    # Forzar que ninguna ruta candidata exista.
    monkeypatch.setattr(ocr_runtime.Path, "exists", lambda _self: False)
    assert ocr_runtime.resolve_tessdata_prefix() is None


def test_configure_ocr_runtime(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setattr(ocr_runtime, "resolve_tesseract_cmd", lambda: None)
    monkeypatch.setattr(ocr_runtime, "resolve_poppler_path", lambda: None)
    monkeypatch.setattr(ocr_runtime, "resolve_tessdata_prefix", lambda: None)
    assert ocr_runtime.configure_ocr_runtime() == (None, None)
