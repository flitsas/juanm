import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  reactStrictMode: true,
  // Export estatico: genera HTML/JS en `out/` para servir con nginx (sin servidor Node).
  output: "export",
  // `next/image` (FlitLogo) requiere loader desactivado en export estatico.
  images: { unoptimized: true },
};

export default nextConfig;
