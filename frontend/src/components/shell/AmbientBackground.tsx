import { ShootingStars } from "@/components/flit/shooting-stars";

/**
 * Fondo ambiental — flitready-suite/src/components/ambient-background.tsx
 * Light: blobs suaves + estelas blancas. Dark: solo estelas azules corporativas.
 */
export function AmbientBackground() {
  return (
    <div aria-hidden className="pointer-events-none fixed inset-0 -z-10 overflow-hidden">
      <div className="absolute inset-0 dark:hidden">
        <div
          className="flit-ambient-blob absolute -top-32 -left-32 h-[55vh] w-[55vh] rounded-full opacity-[0.18]"
          style={{
            background: "radial-gradient(circle, #b3ff1f 0%, transparent 70%)",
            filter: "blur(110px)",
            animation: "blob-drift-a 48s ease-in-out infinite",
          }}
        />
        <div
          className="flit-ambient-blob absolute top-1/3 -right-40 h-[60vh] w-[60vh] rounded-full opacity-[0.20]"
          style={{
            background: "radial-gradient(circle, #003eff 0%, transparent 70%)",
            filter: "blur(120px)",
            animation: "blob-drift-b 62s ease-in-out infinite",
          }}
        />
        <div
          className="flit-ambient-blob absolute bottom-[-20%] left-1/4 h-[55vh] w-[55vh] rounded-full opacity-[0.15]"
          style={{
            background: "radial-gradient(circle, #00DBD5 0%, transparent 70%)",
            filter: "blur(110px)",
            animation: "blob-drift-c 56s ease-in-out infinite",
          }}
        />
        <ShootingStars count={4} tone="white" period={10} />
      </div>

      <div className="absolute inset-0 hidden dark:block">
        <ShootingStars count={4} tone="blue" period={10} />
      </div>
    </div>
  );
}
