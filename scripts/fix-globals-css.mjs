import { execSync } from "node:child_process";
import fs from "node:fs";

const head = execSync("git show HEAD:frontend/src/app/globals.css", { encoding: "utf8" });
const shellTail = execSync("git show main:frontend/src/app/globals.css", { encoding: "utf8" });

const rootEnd = head.indexOf("\n}\n\nbody");
const shellStart = shellTail.indexOf("@utility bg-gradient-flit");
const authStart = head.indexOf(".flit-input {");

let merged = `${head.slice(0, rootEnd)}\n  /* Aliases shell / PrimeReact → tokens FLIT */\n  --radius: 0.875rem;\n  --lime: var(--flit-lime);\n  --action: var(--flit-action);\n  --deep: var(--flit-text-primary);\n  --tech: var(--flit-tech);\n  --amber-acc: var(--flit-amber);\n  --alert: var(--flit-state-danger);\n  --table-head: var(--flit-table-head);\n  --gradient-flit: var(--flit-gradient-flit);\n  --background: var(--flit-bg-app);\n  --foreground: var(--flit-text-primary);\n  --card: var(--flit-bg-card);\n  --muted: var(--flit-bg-hover);\n  --muted-foreground: var(--flit-text-secondary);\n  --border: var(--flit-border-input);\n}${head.slice(rootEnd + 2, authStart > 0 ? authStart : undefined)}`;

if (shellStart > 0) {
  merged += shellTail.slice(shellStart);
}

fs.writeFileSync("frontend/src/app/globals.css", merged, "utf8");
console.log(`globals.css written (${merged.length} chars)`);
