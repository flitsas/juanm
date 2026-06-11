#!/usr/bin/env python3
"""Escaneo rápido de desviaciones UX/UI FLIT en archivos frontend."""

from __future__ import annotations

import re
import sys
from pathlib import Path

HEX_PATTERN = re.compile(r"#[0-9a-fA-F]{3,8}\b")
FORBIDDEN_COPY = re.compile(r"\b(Tenant|Save changes|User list)\b", re.IGNORECASE)
REQUIRED_PATTERNS = [
    (re.compile(r"var\(--flit-"), "tokens --flit-*"),
    (re.compile(r"flit-(card|table|input|btn-primary|badge)"), "clases flit-*"),
]


def audit_file(path: Path) -> list[str]:
    issues: list[str] = []
    text = path.read_text(encoding="utf-8")

    for match in HEX_PATTERN.finditer(text):
        snippet = text[max(0, match.start() - 20) : match.end() + 20]
        if "flit" in snippet.lower() or ":root" in snippet:
            continue
        issues.append(f"  hex suelto {match.group()} en {path.name}")

    for match in FORBIDDEN_COPY.finditer(text):
        if "tenantId" in text[max(0, match.start() - 10) : match.end() + 10]:
            continue
        issues.append(f"  copy EN '{match.group()}' en {path.name}")

    return issues


def audit_dir(root: Path) -> int:
    if not root.exists():
        print(f"Ruta no encontrada: {root}")
        return 1

    files = list(root.rglob("*"))
    tsx_files = [f for f in files if f.suffix in {".tsx", ".ts", ".css"} and f.is_file()]

    all_issues: list[str] = []
    for f in tsx_files:
        all_issues.extend(audit_file(f))

    if all_issues:
        print("FAIL — hallazgos flit-design-guardian:")
        for issue in all_issues:
            print(issue)
        return 1

    print(f"PASS — {len(tsx_files)} archivos sin desviaciones obvias.")
    return 0


if __name__ == "__main__":
    target = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("frontend/src/features")
    raise SystemExit(audit_dir(target))
