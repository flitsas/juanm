#!/usr/bin/env python3
import base64, json, re, ssl, sys, urllib.parse, urllib.request
from pathlib import Path
hu = sys.argv[1]
ctx = ssl.create_default_context(); ctx.check_hostname = False; ctx.verify_mode = ssl.CERT_NONE
env = Path(__file__).resolve().parents[1] / ".env.user-identity"
text = env.read_text(encoding="utf-8")
g = lambda k: re.search(rf'{k}="([^"]+)"', text).group(1)
org, proj, pat = g("AZURE_ORG_URL"), g("AZURE_PROJECT_NAME"), g("AZURE_PAT")
auth = "Basic " + base64.b64encode(f":{pat}".encode()).decode()
url = f"{org}/{urllib.parse.quote(proj)}/_apis/wit/workitems/{hu}?api-version=7.1"
with urllib.request.urlopen(urllib.request.Request(url, headers={"Authorization": auth}), context=ctx) as r:
    d = json.load(r)
f = d["fields"]
print("STATE:", f.get("System.State"))
print("TITLE:", f.get("System.Title"))
print("AC:", f.get("Microsoft.VSTS.Common.AcceptanceCriteria", ""))
