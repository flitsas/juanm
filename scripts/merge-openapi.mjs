import fs from "node:fs";

const head = fs.readFileSync("tmp-openapi-head.yaml", "utf8");
const main = fs.readFileSync("tmp-openapi-main.yaml", "utf8");

function extractBetween(yaml, startMarker, endMarker) {
  const start = yaml.indexOf(startMarker);
  const end = yaml.indexOf(endMarker, start + startMarker.length);
  return yaml.slice(start, end).trimEnd();
}

const header = head.split("paths:")[0];
const authPaths = extractBetween(head, "  /auth/login:", "  /health:");
const mainPaths = extractBetween(main, "  /api/v1/dgc/ocr/lotes:", "  /health:");
const health = extractBetween(main, "  /health:", "components:");
const authSchemasBlock = extractBetween(head, "    LoginRequest:", "    HealthResponse:");
const mainSchemasBlock = main.slice(main.indexOf("    ConfirmOcrItemRequest:")).trimEnd();

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
${authSchemasBlock}
${mainSchemasBlock}
`;

fs.writeFileSync("contracts/openapi/core-api.v1.yaml", merged, "utf8");
console.log(`Merged openapi: ${merged.split("\n").length} lines`);
