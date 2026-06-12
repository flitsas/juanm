#!/usr/bin/env python3
"""Merge Plantillas model into Reglas snapshot for feature integration."""
from __future__ import annotations

import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SNAPSHOT = ROOT / "services/core-api/src/Gdc.Infrastructure/Migrations/GdcDbContextModelSnapshot.cs"
PLANTILLAS_MARKER = 'modelBuilder.Entity("Gdc.Modules.Plantillas.Domain.Entities.'
REGlas_MARKER = 'modelBuilder.Entity("Gdc.Modules.Reglas.Domain.Entities.'


def git_show(ref: str, path: str) -> str:
    return subprocess.check_output(["git", "show", f"{ref}:{path}"], text=True)


def extract_plantillas_sections(text: str) -> str:
    lines = text.splitlines(keepends=True)
    chunks: list[str] = []
    i = 0
    while i < len(lines):
        if PLANTILLAS_MARKER in lines[i]:
            start = i
            depth = 0
            while i < len(lines):
                depth += lines[i].count("{") - lines[i].count("}")
                i += 1
                if depth <= 0 and lines[i - 1].strip() == "});":
                    break
            chunks.append("".join(lines[start:i]))
        else:
            i += 1
    return "".join(chunks)


def main() -> None:
    head = git_show("HEAD", "services/core-api/src/Gdc.Infrastructure/Migrations/GdcDbContextModelSnapshot.cs")
    designer = git_show(
        "feature/AB-9564-gdc-plantillas",
        "services/core-api/src/Gdc.Infrastructure/Migrations/20260612035742_GdcPlantillasInitialSchema.Designer.cs",
    )

    plantillas = extract_plantillas_sections(designer)
    if not plantillas.strip():
        raise SystemExit("No Plantillas sections extracted from designer.")

    if PLANTILLAS_MARKER in head:
        merged = head
    else:
        anchor = REGlas_MARKER
        idx = head.find(anchor)
        if idx < 0:
            raise SystemExit("Reglas anchor not found in HEAD snapshot.")
        merged = head[:idx] + plantillas + head[idx:]

    # Also merge RolePermission seeds for reglas if missing (HEAD has them)
    SNAPSHOT.write_bytes(merged.encode("utf-8"))
    print(f"Wrote merged snapshot ({len(merged)} chars) to {SNAPSHOT}")


if __name__ == "__main__":
    main()
