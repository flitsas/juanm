#!/usr/bin/env python3
import base64
import json
import re
import ssl
import sys
import urllib.parse
import urllib.request
from pathlib import Path

hu_id = sys.argv[1]
ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE
env = Path(__file__).resolve().parents[1] / ".env.user-identity"
text = env.read_text(encoding="utf-8")


def get(key: str) -> str:
    return re.search(rf'{key}="([^"]+)"', text).group(1)


org = get("AZURE_ORG_URL")
proj = get("AZURE_PROJECT_NAME")
pat = get("AZURE_PAT")
name = get("USER_REAL_NAME")
email = get("USER_REAL_EMAIL")
auth = "Basic " + base64.b64encode(f":{pat}".encode()).decode()
proj_enc = urllib.parse.quote(proj)
comment = (
    f'<div>🤖 [Auto] usando <b>@skill-gestion-hu</b>: Iniciando desarrollo bajo supervisión de '
    f'<a href="mailto:{email}">@{name}</a>. Historia activada y en progreso.</div>'
)
patch = [
    {"op": "replace", "path": "/fields/System.State", "value": "Active"},
    {"op": "add", "path": "/fields/System.History", "value": comment},
]
url = f"{org}/{proj_enc}/_apis/wit/workitems/{hu_id}?api-version=7.1"
req = urllib.request.Request(
    url,
    data=json.dumps(patch, ensure_ascii=False).encode("utf-8"),
    method="PATCH",
    headers={"Authorization": auth, "Content-Type": "application/json-patch+json; charset=utf-8"},
)
with urllib.request.urlopen(req, context=ctx) as r:
    data = json.load(r)
print(f"HU #{hu_id} -> {data['fields']['System.State']} rev={data['rev']}")
