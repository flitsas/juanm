#!/usr/bin/env node
/**
 * dev.mjs — Arranca API (.NET), frontend (Next.js) y python-ml en paralelo.
 */
import { spawn } from "node:child_process";

const isWin = process.platform === "win32";

const services = [
  {
    name: "api",
    command: "dotnet",
    args: [
      "watch",
      "--project",
      "services/core-api/src/Gdc.Api/Gdc.Api.csproj",
      "--property:PublishAot=false",
      "--property:IsAotCompatible=false",
      "--property:RunAnalyzersDuringBuild=false",
      "run",
      "--launch-profile",
      "http",
    ],
  },
  {
    name: "web",
    command: isWin ? "pnpm.cmd" : "pnpm",
    args: ["--filter", "@gdc/frontend", "dev"],
  },
  {
    name: "python-ml",
    command: "py",
    args: [
      "-m",
      "uvicorn",
      "app.main:app",
      "--app-dir",
      "services/python-ml",
      "--host",
      "127.0.0.1",
      "--port",
      "4012",
      "--reload",
    ],
  },
];

const children = [];

function prefixOutput(name, data) {
  for (const line of data.toString().split(/\r?\n/)) {
    if (line.length > 0) process.stdout.write(`[${name}] ${line}\n`);
  }
}

function shutdown(signal = "SIGTERM") {
  for (const child of children) {
    if (!child.killed) child.kill(signal);
  }
}

for (const { name, command, args } of services) {
  const child = spawn(command, args, {
    cwd: process.cwd(),
    stdio: ["inherit", "pipe", "pipe"],
    env: process.env,
    shell: isWin,
  });

  children.push(child);
  child.stdout?.on("data", (data) => prefixOutput(name, data));
  child.stderr?.on("data", (data) => prefixOutput(name, data));
  child.on("exit", (code, signal) => {
    if (signal) {
      console.log(`[${name}] terminado (${signal})`);
      return;
    }
    if (code !== 0) {
      console.error(`[${name}] salió con código ${code}`);
      shutdown();
      process.exit(code ?? 1);
    }
  });
}

process.on("SIGINT", () => {
  shutdown("SIGINT");
  process.exit(0);
});
process.on("SIGTERM", () => {
  shutdown("SIGTERM");
  process.exit(0);
});
