#!/usr/bin/env python3
import base64, json, re, ssl, sys, urllib.parse, urllib.request
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
ENV = ROOT / ".env.user-identity"
CTX = ssl.create_default_context(); CTX.check_hostname = False; CTX.verify_mode = ssl.CERT_NONE

def g(k): return re.search(rf'{k}="([^"]+)"', ENV.read_text(encoding="utf-8")).group(1)

def ado(method, path, body=None):
    auth = "Basic " + base64.b64encode(f":{g('AZURE_PAT')}".encode()).decode()
    url = f"{g('AZURE_ORG_URL')}/{urllib.parse.quote(g('AZURE_PROJECT_NAME'))}/{path}"
    data = json.dumps(body, ensure_ascii=False).encode() if body else None
    h = {"Authorization": auth}
    if body: h["Content-Type"] = "application/json-patch+json; charset=utf-8"
    with urllib.request.urlopen(urllib.request.Request(url, data=data, method=method, headers=h), context=CTX) as r:
        return json.load(r)

hu, action = int(sys.argv[1]), sys.argv[2]
if action == "activate":
    name, email = g("USER_REAL_NAME"), g("USER_REAL_EMAIL")
    c = f'<div>🤖 [frontend-agent] usando <b>@skill-gestion-hu</b>: Iniciando desarrollo bajo supervisión de <a href="mailto:{email}">@{name}</a>. Historia activada y en progreso.</div>'
    ado("PATCH", f"_apis/wit/workitems/{hu}?api-version=7.1", [{"op":"replace","path":"/fields/System.State","value":"Active"},{"op":"add","path":"/fields/System.History","value":c}])
    print(f"HU #{hu} activated")
