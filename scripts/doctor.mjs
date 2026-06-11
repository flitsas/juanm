#!/usr/bin/env node
/**
 * doctor.mjs — Diagnóstico del entorno local GDC 2.0
 */
import { execSync } from "node:child_process";
import net from "node:net";

const OK = "✓";
const DIM = "·";

const DEV_PORTS = [
  { port: 40103, label: "frontend (Next.js)" },
  { port: 40203, label: "gateway (YARP)" },
  { port: 40303, label: "core-api (.NET)" },
  { port: 5432, label: "postgres (docker)" },
];

function run(cmd) {
  try {
    return execSync(cmd, { stdio: ["ignore", "pipe", "pipe"] }).toString().trim();
  } catch {
    return null;
  }
}

function checkTool(name, cmd) {
  const out = run(cmd);
  if (out) console.log(`  ${OK} ${name.padEnd(10)} ${out.split("\n")[0]}`);
  else console.log(`  ✗ ${name.padEnd(10)} no encontrado`);
}

function probePort(port) {
  return new Promise((resolve) => {
    const sock = net.createConnection({ host: "127.0.0.1", port, timeout: 400 });
    sock.once("connect", () => {
      sock.end();
      resolve("busy");
    });
    sock.once("timeout", () => {
      sock.destroy();
      resolve("free");
    });
    sock.once("error", () => resolve("free"));
  });
}

async function checkPorts() {
  for (const t of DEV_PORTS) {
    const state = await probePort(t.port);
    const icon = state === "busy" ? OK : DIM;
    console.log(
      `  ${icon} :${String(t.port).padEnd(6)} ${state === "busy" ? "en uso" : "libre "}  ${t.label}`,
    );
  }
}

function checkDockerInfra() {
  const out = run('docker ps --format "{{.Names}}|{{.Status}}"');
  if (out === null) {
    console.log("  ⚠ docker no disponible");
    return;
  }
  const running = out.split("\n").map((l) => l.split("|")[0]?.toLowerCase() || "");
  const hit = running.find((n) => n.includes("gdc-postgres") || n.includes("postgres"));
  if (hit) console.log(`  ${OK} postgres   ${hit}`);
  else console.log(`  ${DIM} postgres   no corriendo (pnpm docker:up)`);
}

async function main() {
  console.log("GDC doctor — entorno DEV\n");
  console.log("Toolchain:");
  checkTool("node", "node --version");
  checkTool("pnpm", "pnpm --version");
  checkTool("dotnet", "dotnet --version");
  console.log("\nPuertos:");
  await checkPorts();
  console.log("\nInfra:");
  checkDockerInfra();
}

main();
