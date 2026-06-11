import type { Metadata } from "next";
import { Providers } from "./providers";
import "./globals.css";

export const metadata: Metadata = {
  title: "Flit Ready",
  description: "Plataforma GDC — Gestión de comparendos",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="es">
      <head>
        <link
          href="https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;700&display=swap"
          rel="stylesheet"
        />
      </head>
      <body style={{ fontFamily: '"Poppins", ui-sans-serif, system-ui, sans-serif' }}>
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}