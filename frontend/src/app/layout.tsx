import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "GDC 2.0",
  description: "Plataforma GDC — React 19 + Next.js 16",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="es">
      <body>{children}</body>
    </html>
  );
}
