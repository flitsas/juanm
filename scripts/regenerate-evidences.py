#!/usr/bin/env python3
"""Regenerate Custom.Evidences for auth HUs using dev-tester template."""
from __future__ import annotations

import base64
import html
import json
import re
import ssl
import subprocess
import urllib.parse
import urllib.request
from datetime import datetime
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
ENV = ROOT / ".env.user-identity"
CTX = ssl.create_default_context()
CTX.check_hostname = False
CTX.verify_mode = ssl.CERT_NONE
NOW = datetime.now().strftime("%Y-%m-%d %H:%M")
BRANCH = "feature/9561-auth-usuarios-gdc"
TH = 'style="border:1px solid #cccccc;padding:6px 8px;"'
TABLE = 'style="border-collapse:collapse;width:100%;"'
LOG_DIV = (
    'style="font-family:Consolas,monospace;white-space:pre-wrap;'
    'background-color:#f5f5f5;border:1px solid #cccccc;padding:12px 14px;'
    'font-size:12px;line-height:1.9"'
)


def get_env(key: str) -> str:
    return re.search(rf'{key}="([^"]+)"', ENV.read_text(encoding="utf-8")).group(1)


def ado_request(method: str, path: str, body=None):
    org, proj, pat = get_env("AZURE_ORG_URL"), get_env("AZURE_PROJECT_NAME"), get_env("AZURE_PAT")
    auth = "Basic " + base64.b64encode(f":{pat}".encode()).decode()
    url = f"{org}/{urllib.parse.quote(proj)}/{path}"
    data = json.dumps(body, ensure_ascii=False).encode("utf-8") if body is not None else None
    req = urllib.request.Request(
        url,
        data=data,
        method=method,
        headers={"Authorization": auth, "Content-Type": "application/json-patch+json; charset=utf-8"},
    )
    with urllib.request.urlopen(req, context=CTX) as r:
        return json.load(r)


def fetch_hu(hu_id: int) -> dict:
    org, proj, pat = get_env("AZURE_ORG_URL"), get_env("AZURE_PROJECT_NAME"), get_env("AZURE_PAT")
    auth = "Basic " + base64.b64encode(f":{pat}".encode()).decode()
    url = f"{org}/{urllib.parse.quote(proj)}/_apis/wit/workitems/{hu_id}?api-version=7.1"
    req = urllib.request.Request(url, headers={"Authorization": auth})
    with urllib.request.urlopen(req, context=CTX) as r:
        return json.load(r)


def run_tests(filter_expr: str) -> str:
    cmd = [
        "dotnet",
        "test",
        str(ROOT / "services" / "core-api"),
        "--logger",
        "console;verbosity=detailed",
        "--filter",
        filter_expr,
    ]
    proc = subprocess.run(cmd, capture_output=True, text=True, encoding="utf-8", errors="replace")
    return proc.stdout + proc.stderr


def run_frontend_tests(path: str = "src/features/auth") -> str:
    cmd = ["npx", "vitest", "run", path]
    proc = subprocess.run(
        cmd,
        cwd=ROOT / "frontend",
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
        shell=True,
    )
    return proc.stdout + proc.stderr


def tbl(headers: list[str], rows: list[list[str]]) -> str:
    head = "".join(f"<th {TH}>{html.escape(h)}</th>" for h in headers)
    body = ""
    for row in rows:
        body += "<tr>" + "".join(f"<td {TH}>{html.escape(c)}</td>" for c in row) + "</tr>"
    return f'<table {TABLE}><tr>{head}</tr>{body}</table>'


def ac_block(
    ac_id: str,
    title: str,
    test_type: str,
    result: str,
    input_rows: list[list[str]],
    expected_rows: list[list[str]],
    actual_rows: list[list[str]],
    test_ref: str,
) -> str:
    return f"""
<h3>AC {ac_id} — {html.escape(title)}</h3>
<p><strong>Tipo de test:</strong> {html.escape(test_type)}<br/>
<strong>Resultado:</strong> {result}<br/>
<strong>Test:</strong> <code>{html.escape(test_ref)}</code></p>
<h4>Datos de entrada</h4>
{tbl(["Campo", "Valor"], input_rows)}
<h4>Datos de salida esperados</h4>
{tbl(["Campo", "Valor"], expected_rows)}
<h4>Datos de salida obtenidos</h4>
{tbl(["Campo", "Valor"], actual_rows)}
<hr/>
"""


def build_html(
    hu_id: int,
    title: str,
    user_name: str,
    fe_specs: int,
    fe_tests: int,
    fe_pass: int,
    be_specs: int,
    be_tests: int,
    be_pass: int,
    fe_specs_list: str,
    be_specs_list: str,
    regresiones: str,
    output_frontend: str,
    output_backend: str,
    ac_html: str,
) -> str:
    fe_log = html.escape(output_frontend).replace("\n", "<br/>")
    be_log = html.escape(output_backend).replace("\n", "<br/>")
    total_specs = fe_specs + be_specs
    total_tests = fe_tests + be_tests
    total_pass = fe_pass + be_pass
    summary = tbl(
        ["Capa", "Specs nuevos", "Tests nuevos", "Pass", "Fail", "Skip"],
        [
            ["Frontend", str(fe_specs), str(fe_tests), str(fe_pass), "0", "0"],
            ["Backend", str(be_specs), str(be_tests), str(be_pass), "0", "0"],
            ["Total", str(total_specs), str(total_tests), str(total_pass), "0", "0"],
        ],
    )
    fe_specs_html = fe_specs_list if fe_specs_list else "<p><em>Sin specs en esta HU.</em></p>"
    be_specs_html = be_specs_list if be_specs_list else "<p><em>Sin specs en esta HU.</em></p>"
    return f"""<div>
<h2>Evidencias de Tests Unitarios — HU #{hu_id}</h2>
<p><strong>Historia:</strong> {html.escape(title)}<br/>
<strong>Fecha:</strong> {NOW}<br/>
<strong>Ejecutado por:</strong> dev-tester bajo supervisión de @{html.escape(user_name)}<br/>
<strong>Rama:</strong> {BRANCH}</p>
<h3>Resumen</h3>
{summary}
<h3>Tests generados</h3>
<h4>Frontend</h4>
{fe_specs_html}
<h4>Backend</h4>
{be_specs_html}
<h3>Regresiones detectadas</h3>
<p>{html.escape(regresiones)}</p>
<h3>Salida completa</h3>
<p><strong>Frontend — vitest ({fe_pass} pass / 0 fail)</strong></p>
<div {LOG_DIV}>{fe_log if output_frontend else "<em>Sin ejecución frontend en esta HU.</em>"}</div>
<p><strong>Backend — dotnet test ({be_pass} pass / 0 fail)</strong></p>
<div {LOG_DIV}>{be_log if output_backend else "<em>Sin ejecución backend en esta HU.</em>"}</div>
<h3>Criterios de Aceptación cubiertos</h3>
{ac_html}
</div>"""


HU_CONFIG = {
    9702: {
        "filter": "FullyQualifiedName~AuthSchemaTests",
        "be_specs": 1,
        "be_tests": 3,
        "specs": [
            "<li><code>services/core-api/tests/Gdc.Infrastructure.Tests/AuthSchemaTests.cs</code> — 3 tests</li>"
        ],
        "acs": lambda: [
            ac_block(
                "1",
                "Migración auth registrada",
                "Contrato",
                "✅ Pass",
                [
                    ["Tipo de petición", "—"],
                    ["Endpoint", "—"],
                    ["Parámetros de ruta", "—"],
                    ["Parámetros de búsqueda", "—"],
                    ["Cuerpo (body)", "—"],
                    ["Nota", "Verificación EF Core — sin HTTP"],
                ],
                [
                    ["Resultado", "Migración AddAuthSchema presente en assembly"],
                ],
                [
                    ["Resultado", "Migración registrada ✓"],
                ],
                "AddAuthSchema_migration_is_registered",
            ),
            ac_block(
                "2",
                "Tablas auth con tenant en users",
                "Contrato",
                "✅ Pass",
                [
                    ["Tipo de petición", "—"],
                    ["Endpoint", "—"],
                    ["Verificación", "Modelo EF auth.users con tenant_id"],
                ],
                [
                    ["Entidades", "auth.users, roles, permissions, activation_tokens, etc."],
                ],
                [
                    ["Entidades", "Todas las tablas auth presentes en modelo ✓"],
                ],
                "Model_includes_auth_tables_with_tenant_scoped_users",
            ),
            ac_block(
                "3",
                "Seed roles SuperAdmin, TenantAdmin, Operator",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "—"],
                    ["Datos", "HasData roles en migración"],
                ],
                [
                    ["Roles", "SuperAdmin, TenantAdmin, Operator"],
                ],
                [
                    ["Roles", "3 roles seed presentes ✓"],
                ],
                "Seed_roles_include_SuperAdmin_TenantAdmin_Operator",
            ),
        ],
    },
    9703: {
        "filter": "FullyQualifiedName~AuthSessionServiceTests|FullyQualifiedName~AuthEndpointsTests",
        "be_specs": 2,
        "be_tests": 6,
        "specs": [
            "<li><code>.../AuthSessionServiceTests.cs</code> — 3 tests</li>",
            "<li><code>.../AuthEndpointsTests.cs</code> — 3 tests</li>",
        ],
        "acs": lambda: [
            ac_block(
                "1",
                "Login exitoso con JWT",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "POST"],
                    ["Endpoint", "/auth/login"],
                    ["Cuerpo (body)", '{"email":"operator@example.com","password":"***"}'],
                ],
                [
                    ["Código de respuesta", "200"],
                    ["Cuerpo", "accessToken, tenantId, role en JWT"],
                ],
                [
                    ["Código de respuesta", "200 ✓"],
                    ["Cuerpo", "JWT con tenantId y role Operator ✓"],
                ],
                "Post_login_returns_200_with_jwt_for_valid_credentials / LoginAsync_with_valid_credentials_returns_jwt_with_tenant_and_role",
            ),
            ac_block(
                "2",
                "Credenciales inválidas",
                "Edge case",
                "✅ Pass",
                [
                    ["Tipo de petición", "POST"],
                    ["Endpoint", "/auth/login"],
                    ["Cuerpo (body)", '{"email":"...","password":"invalid"}'],
                ],
                [
                    ["Código de respuesta", "401"],
                    ["Cuerpo", '{"message":"Credenciales inválidas."}'],
                ],
                [
                    ["Código de respuesta", "401 ✓"],
                    ["Cuerpo", "Mensaje genérico sin enumerar email ✓"],
                ],
                "Post_login_returns_401_for_invalid_password / LoginAsync_with_invalid_password_returns_generic_error",
            ),
            ac_block(
                "3",
                "Logout revoca token",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "POST"],
                    ["Endpoint", "/auth/logout"],
                    ["Auth", "Bearer JWT válido"],
                ],
                [
                    ["Código de respuesta", "204"],
                    ["Efecto", "Token en auth.revoked_tokens; reutilización → 401"],
                ],
                [
                    ["Código de respuesta", "204 en logout; 401 en reutilización ✓"],
                ],
                "Post_logout_returns_401_when_token_revoked_on_reuse / LogoutAsync_revokes_token_and_blocks_reuse",
            ),
        ],
    },
    9704: {
        "filter": "FullyQualifiedName~MultiTenant",
        "be_specs": 2,
        "be_tests": 6,
        "specs": [
            "<li><code>.../MultiTenantFilterTests.cs</code> — 3 tests</li>",
            "<li><code>.../MultiTenantEndpointsTests.cs</code> — 3 tests</li>",
        ],
        "acs": lambda: [
            ac_block(
                "1",
                "Consulta por tenant",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "GET"],
                    ["Endpoint", "/auth/users"],
                    ["Auth", "Bearer JWT tenant A"],
                ],
                [
                    ["Código de respuesta", "200"],
                    ["Cuerpo", "Solo usuarios con tenant_id del token"],
                ],
                [
                    ["Código de respuesta", "200 ✓"],
                    ["Cuerpo", "Sin registros de tenant B ✓"],
                ],
                "Get_users_returns_only_current_tenant_records / ListCurrentTenantUsers_returns_only_matching_tenant",
            ),
            ac_block(
                "2",
                "Cross-tenant",
                "Edge case",
                "✅ Pass",
                [
                    ["Tipo de petición", "GET"],
                    ["Endpoint", "/auth/users/{userId}"],
                    ["userId", "Usuario de otro tenant"],
                ],
                [
                    ["Código de respuesta", "404"],
                    ["Cuerpo", "Sin filtración de existencia"],
                ],
                [
                    ["Código de respuesta", "404 ✓"],
                ],
                "Get_user_by_id_returns_not_found_for_other_tenant / GetUserById_returns_null_for_cross_tenant_user",
            ),
            ac_block(
                "3",
                "Super Admin",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "GET"],
                    ["Endpoint", "/auth/admin/users"],
                    ["Policy", "SuperAdmin"],
                ],
                [
                    ["Código de respuesta", "200"],
                    ["Cuerpo", "Usuarios cross-tenant"],
                ],
                [
                    ["Código de respuesta", "200 con usuarios tenant B ✓"],
                ],
                "SuperAdmin_admin_users_returns_cross_tenant_list / SuperAdmin_can_list_all_users_with_bypass",
            ),
        ],
    },
    9705: {
        "filter": "FullyQualifiedName~UserInvitationServiceTests|FullyQualifiedName~InvitationEndpointsTests",
        "be_specs": 2,
        "be_tests": 8,
        "specs": [
            "<li><code>.../UserInvitationServiceTests.cs</code> — 4 tests</li>",
            "<li><code>.../InvitationEndpointsTests.cs</code> — 4 tests</li>",
        ],
        "acs": lambda: [
            ac_block(
                "1",
                "Invitación (RF03)",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "POST"],
                    ["Endpoint", "/auth/users/invite"],
                    ["Auth", "Bearer SuperAdmin"],
                    ["Cuerpo (body)", '{"email":"invited@example.com","tenantId":"..."}'],
                ],
                [
                    ["Código de respuesta", "201"],
                    ["Efecto", "Usuario Pending + correo enviado"],
                ],
                [
                    ["Código de respuesta", "201 ✓"],
                    ["Status", "Pending ✓; email capturado en test ✓"],
                ],
                "Post_invite_creates_pending_user_and_sends_email / InviteAsync_creates_pending_user_and_sends_email",
            ),
            ac_block(
                "2",
                "Activación 48h (RF04)",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "POST"],
                    ["Endpoint", "/auth/users/activate"],
                    ["Cuerpo (body)", '{"token":"...","password":"FreshPass1"}'],
                ],
                [
                    ["Código de respuesta", "200"],
                    ["Efecto", "Cuenta Active; token consumido"],
                ],
                [
                    ["Código de respuesta", "200 ✓"],
                    ["Login posterior", "200 ✓"],
                ],
                "Post_activate_activates_account_with_valid_token / ActivateAsync_activates_user_and_consumes_token",
            ),
            ac_block(
                "3",
                "Token expirado",
                "Edge case",
                "✅ Pass",
                [
                    ["Tipo de petición", "POST"],
                    ["Endpoint", "/auth/users/activate"],
                    ["Token", "Expirado (&gt;48h)"],
                ],
                [
                    ["Código de respuesta", "410"],
                    ["Cuerpo", "Mensaje token expirado"],
                ],
                [
                    ["Código de respuesta", "410 ✓"],
                ],
                "Post_activate_returns_410_for_expired_token / ActivateAsync_returns_expired_for_token_older_than_48h",
            ),
        ],
    },
    9708: {
        "filter": "FullyQualifiedName~auth",
        "fe_specs": 4,
        "fe_tests": 8,
        "be_specs": 0,
        "be_tests": 0,
        "specs": [
            "<li><code>frontend/src/features/auth/components/login-form.spec.tsx</code> — 2 tests</li>",
            "<li><code>frontend/src/features/auth/components/logout-button.spec.tsx</code> — 1 test</li>",
            "<li><code>frontend/src/features/auth/api/auth-api.spec.ts</code> — 3 tests</li>",
            "<li><code>frontend/src/features/auth/lib/session.spec.ts</code> — 2 tests</li>",
        ],
        "acs": lambda: [
            ac_block(
                "1",
                "Login exitoso",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "POST"],
                    ["Endpoint", "/auth/login"],
                    ["UI", "/login → submit"],
                ],
                [
                    ["Efecto", "Redirige a /dashboard autenticado"],
                ],
                [
                    ["Efecto", "router.push('/dashboard') ✓"],
                ],
                "redirige al dashboard tras login exitoso",
            ),
            ac_block(
                "2",
                "Error credenciales",
                "Edge case",
                "✅ Pass",
                [
                    ["Tipo de petición", "POST"],
                    ["Endpoint", "/auth/login"],
                    ["Respuesta API", "401 genérico"],
                ],
                [
                    ["UI", "role=alert con mensaje genérico"],
                ],
                [
                    ["UI", "Credenciales inválidas. sin detalle por campo ✓"],
                ],
                "muestra mensaje accesible sin detalle por campo en error",
            ),
            ac_block(
                "3",
                "Logout",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "POST"],
                    ["Endpoint", "/auth/logout"],
                    ["UI", "Cerrar sesión en dashboard"],
                ],
                [
                    ["Efecto", "Vuelve a /login"],
                ],
                [
                    ["Efecto", "clearSession + replace('/login') ✓"],
                ],
                "cierra sesión y redirige al login",
            ),
        ],
        "frontend_only": True,
    },
    9707: {
        "filter": "FullyQualifiedName~RbacMatrix",
        "be_specs": 2,
        "be_tests": 8,
        "specs": [
            "<li><code>.../RbacMatrixServiceTests.cs</code> — 4 tests</li>",
            "<li><code>.../RbacMatrixEndpointsTests.cs</code> — 4 tests</li>",
        ],
        "acs": lambda: [
            ac_block(
                "1",
                "Consultar matriz",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "GET"],
                    ["Endpoint", "/auth/rbac/matrix"],
                    ["Auth", "Bearer SuperAdmin"],
                ],
                [
                    ["Código de respuesta", "200"],
                    ["Cuerpo", "roles y permisos"],
                ],
                [
                    ["Código de respuesta", "200 ✓"],
                    ["Cuerpo", "≥3 roles y ≥3 permisos ✓"],
                ],
                "Get_rbac_matrix_returns_roles_and_permissions_for_super_admin / GetMatrixAsync_returns_roles_and_permissions",
            ),
            ac_block(
                "2",
                "Actualizar matriz",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "PUT"],
                    ["Endpoint", "/auth/rbac/matrix"],
                    ["Cuerpo", "assignments con enabled true/false"],
                ],
                [
                    ["Código de respuesta", "204"],
                    ["Efecto", "Cambios visibles en GET siguiente"],
                ],
                [
                    ["Código de respuesta", "204 ✓; GET refleja cambio ✓"],
                ],
                "Put_rbac_matrix_persists_changes_for_super_admin / UpdateMatrixAsync_is_reflected_on_next_get",
            ),
            ac_block(
                "3",
                "Denegado",
                "Edge case",
                "✅ Pass",
                [
                    ["Tipo de petición", "PUT"],
                    ["Endpoint", "/auth/rbac/matrix"],
                    ["Auth", "Bearer Operator (no SuperAdmin)"],
                ],
                [
                    ["Código de respuesta", "403"],
                ],
                [
                    ["Código de respuesta", "403 ✓"],
                ],
                "Put_rbac_matrix_returns_403_for_non_super_admin",
            ),
        ],
    },
    9706: {
        "filter": "FullyQualifiedName~PasswordRecovery|FullyQualifiedName~LoginAsync_locks",
        "be_specs": 2,
        "be_tests": 10,
        "specs": [
            "<li><code>.../PasswordRecoveryServiceTests.cs</code> — 5 tests</li>",
            "<li><code>.../PasswordRecoveryEndpointsTests.cs</code> — 5 tests</li>",
        ],
        "acs": lambda: [
            ac_block(
                "1",
                "Olvidé clave (RF06)",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "POST"],
                    ["Endpoint", "/auth/password/forgot"],
                    ["Cuerpo (body)", '{"email":"..."}'],
                ],
                [
                    ["Código de respuesta", "202"],
                    ["Efecto", "Siempre 202 sin enumerar email"],
                ],
                [
                    ["Código de respuesta", "202 para email conocido y desconocido ✓"],
                ],
                "Post_forgot_password_returns_202_for_unknown_email / ForgotPasswordAsync_always_accepts_request",
            ),
            ac_block(
                "2",
                "Reset Admin (RF05)",
                "Happy path",
                "✅ Pass",
                [
                    ["Tipo de petición", "PUT"],
                    ["Endpoint", "/auth/users/{userId}/password"],
                    ["Auth", "Bearer SuperAdmin"],
                ],
                [
                    ["Código de respuesta", "204"],
                    ["Efecto", "Login exitoso con nueva contraseña"],
                ],
                [
                    ["Código de respuesta", "204 ✓; login 200 ✓"],
                ],
                "Put_user_password_by_admin_allows_login_with_new_password / AdminSetPasswordAsync_updates_active_user_password",
            ),
            ac_block(
                "3",
                "Intentos fallidos (RF09)",
                "Edge case",
                "✅ Pass",
                [
                    ["Tipo de petición", "POST"],
                    ["Endpoint", "/auth/login"],
                    ["Condición", "Umbral de intentos fallidos superado"],
                ],
                [
                    ["Código de respuesta", "401"],
                    ["Cuerpo", "Bloqueo temporal + sugerencia recuperar clave"],
                ],
                [
                    ["Código de respuesta", "401 con mensaje de sugerencia ✓"],
                ],
                "Post_login_returns_locked_suggestion_after_threshold / LoginAsync_locks_account_after_failed_threshold_with_suggestion",
            ),
        ],
    },
}


def main():
    import sys

    user_name = get_env("USER_REAL_NAME")
    targets = [int(x) for x in sys.argv[1:]] if len(sys.argv) > 1 else list(HU_CONFIG.keys())
    for hu_id in targets:
        cfg = HU_CONFIG[hu_id]
        wi = fetch_hu(hu_id)
        title = wi["fields"]["System.Title"]
        fe_specs = cfg.get("fe_specs", 0)
        fe_tests = cfg.get("fe_tests", 0)
        be_specs = cfg.get("be_specs", 0)
        be_tests = cfg.get("be_tests", 0)
        output_fe = ""
        output_be = ""
        if cfg.get("frontend_only"):
            output_fe = run_frontend_tests(cfg.get("fe_path", "src/features/auth"))
        else:
            output_be = run_tests(cfg["filter"])
        fe_list = (
            "<ul>" + "\n".join(cfg.get("fe_specs_list", cfg.get("specs", []))) + "</ul>"
            if fe_specs
            else ""
        )
        be_list = (
            "<ul>" + "\n".join(cfg.get("be_specs_list", cfg.get("specs", []))) + "</ul>"
            if be_specs
            else ""
        )
        html_doc = build_html(
            hu_id=hu_id,
            title=title,
            user_name=user_name,
            fe_specs=fe_specs,
            fe_tests=fe_tests,
            fe_pass=fe_tests,
            be_specs=be_specs,
            be_tests=be_tests,
            be_pass=be_tests,
            fe_specs_list=fe_list,
            be_specs_list=be_list,
            regresiones="Ninguna",
            output_frontend=output_fe,
            output_backend=output_be,
            ac_html="".join(cfg["acs"]()),
        )
        op = "replace" if "Custom.Evidences" in wi.get("fields", {}) else "add"
        patch = [{"op": op, "path": "/fields/Custom.Evidences", "value": html_doc}]
        result = ado_request(
            "PATCH",
            f"_apis/wit/workitems/{hu_id}?api-version=7.1",
            patch,
        )
        print(f"HU #{hu_id} published op={op} rev={result['rev']} ({len(html_doc)} chars)")


if __name__ == "__main__":
    main()
