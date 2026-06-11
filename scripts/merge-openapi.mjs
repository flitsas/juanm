import { execSync } from "node:child_process";
import { writeFileSync } from "node:fs";

function gitBlob(rev, path) {
  return execSync(`git show ${rev}:${path}`, { encoding: "utf8" }).replace(/^\uFEFF/, "");
}

function extractBetween(yaml, startMarker, endMarker) {
  const start = yaml.indexOf(startMarker);
  if (start === -1) {
    throw new Error(`Marker not found: ${startMarker}`);
  }
  const end = yaml.indexOf(endMarker, start + startMarker.length);
  if (end === -1) {
    throw new Error(`End marker not found after ${startMarker}: ${endMarker}`);
  }
  return yaml.slice(start, end).trimEnd();
}

const authBlob = gitBlob("30a9f16", "contracts/openapi/core-api.v1.yaml");
const mainBlob = gitBlob("f4c0e5d", "contracts/openapi/core-api.v1.yaml");

const header = authBlob.split("paths:")[0];
const authPaths = extractBetween(authBlob, "  /auth/login:", "  /health:");
const mainPaths = extractBetween(mainBlob, "  /api/v1/dgc/ocr/lotes:", "  /health:");
const health = extractBetween(mainBlob, "  /health:", "components:");
const authSchemas = extractBetween(authBlob, "    LoginRequest:", "    HealthResponse:");
const mainSchemasStart = mainBlob.indexOf("    ConfirmOcrItemRequest:");
if (mainSchemasStart === -1) {
  throw new Error("ConfirmOcrItemRequest schema not found in f4c0e5d blob");
}
const mainSchemas = mainBlob.slice(mainSchemasStart).trimEnd();

const merged = `${header}paths:
${authPaths}
${mainPaths}
${health}
components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT
  schemas:
${authSchemas}
${mainSchemas}
`;

writeFileSync("contracts/openapi/core-api.v1.yaml", merged, { encoding: "utf8" });

const pathCount = [...merged.matchAll(/^  \/\S+/gm)].length;
console.log(
  `Wrote contracts/openapi/core-api.v1.yaml (${merged.split("\n").length} lines, ${pathCount} paths)`,
);
