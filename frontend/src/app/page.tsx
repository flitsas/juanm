import { getApiBaseUrl } from "@/lib/api-base-url";

const apiBaseUrl = getApiBaseUrl();

async function getHealth(): Promise<{
  status: string;
  env: string;
  database: string;
} | null> {
  try {
    const res = await fetch(`${apiBaseUrl}/health`, { cache: "no-store" });
    if (!res.ok) return null;
    return res.json();
  } catch {
    return null;
  }
}

export default async function HomePage() {
  const health = await getHealth();

  return (
    <main className="mx-auto flex min-h-screen max-w-3xl flex-col gap-8 p-8">
      <header>
        <p className="text-sm font-medium text-neutral-500">GDC 2.0</p>
        <h1 className="mt-2 text-3xl font-semibold tracking-tight">Monorepo listo</h1>
        <p className="mt-2 text-neutral-600">
          Frontend Next.js 16 + backend .NET 10 con PostgreSQL.
        </p>
      </header>

      <section className="rounded-xl border border-neutral-200 p-6 dark:border-neutral-800">
        <h2 className="text-lg font-medium">Estado del API</h2>
        {health ? (
          <dl className="mt-4 grid gap-2 text-sm">
            <div className="flex justify-between gap-4">
              <dt className="text-neutral-500">status</dt>
              <dd className="font-mono">{health.status}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-neutral-500">env</dt>
              <dd className="font-mono">{health.env}</dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-neutral-500">database</dt>
              <dd className="font-mono">{health.database}</dd>
            </div>
          </dl>
        ) : (
          <p className="mt-4 text-sm text-amber-700">
            No se pudo contactar el API en {apiBaseUrl}. Ejecuta{" "}
            <code className="rounded bg-neutral-100 px-1 py-0.5 dark:bg-neutral-900">
              pnpm dev
            </code>{" "}
            y verifica Postgres con{" "}
            <code className="rounded bg-neutral-100 px-1 py-0.5 dark:bg-neutral-900">
              pnpm docker:up
            </code>
            .
          </p>
        )}
      </section>
    </main>
  );
}
