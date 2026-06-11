"""Resolución de binarios OCR en Windows/Linux para dev local y Docker."""

from __future__ import annotations

import os
import shutil
from pathlib import Path


def _first_existing(paths: list[str]) -> str | None:
    for path in paths:
        if path and Path(path).exists():
            return path
    return None


def resolve_tesseract_cmd() -> str | None:
    env = os.environ.get("TESSERACT_CMD")
    if env and Path(env).exists():
        return env

    found = shutil.which("tesseract")
    if found:
        return found

    windows_candidates = [
        r"C:\Program Files\Tesseract-OCR\tesseract.exe",
        r"C:\Program Files (x86)\Tesseract-OCR\tesseract.exe",
    ]
    return _first_existing(windows_candidates)


def resolve_poppler_path() -> str | None:
    env = os.environ.get("POPPLER_PATH")
    if env and Path(env).exists():
        return env

    found = shutil.which("pdftoppm")
    if found:
        return str(Path(found).parent)

    winget_root = Path.home() / "AppData/Local/Microsoft/WinGet/Packages"
    if winget_root.exists():
        for poppler_bin in winget_root.glob("oschwartz10612.Poppler*/poppler*/Library/bin"):
            if (poppler_bin / "pdftoppm.exe").exists():
                return str(poppler_bin)

    return None


def resolve_tessdata_prefix() -> str | None:
    env = os.environ.get("TESSDATA_PREFIX")
    if env and Path(env).exists():
        return env

    project_tessdata = Path(__file__).resolve().parents[3] / "tessdata"
    if (project_tessdata / "spa.traineddata").exists():
        return str(project_tessdata)

    system_tessdata = Path(r"C:\Program Files\Tesseract-OCR\tessdata")
    if system_tessdata.exists():
        return str(system_tessdata)

    return None


def configure_ocr_runtime() -> tuple[str | None, str | None]:
    tesseract_cmd = resolve_tesseract_cmd()
    poppler_path = resolve_poppler_path()
    tessdata_prefix = resolve_tessdata_prefix()

    if tesseract_cmd:
        os.environ.setdefault("TESSERACT_CMD", tesseract_cmd)
        try:
            import pytesseract

            pytesseract.pytesseract.tesseract_cmd = tesseract_cmd
        except Exception:
            pass

    if tessdata_prefix:
        os.environ.setdefault("TESSDATA_PREFIX", tessdata_prefix)

    if poppler_path:
        os.environ.setdefault("POPPLER_PATH", poppler_path)

    return tesseract_cmd, poppler_path
