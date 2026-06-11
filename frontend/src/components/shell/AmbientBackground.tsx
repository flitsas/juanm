/** Fondo ambiental — adaptado de flitready-suite/src/components/ambient-background.tsx */
export function AmbientBackground() {
  return (
    <div aria-hidden className="pointer-events-none fixed inset-0 -z-10 overflow-hidden">
      <div
        className="absolute -top-32 -left-32 h-[55vh] w-[55vh] rounded-full opacity-[0.18]"
        style={{
          background: "radial-gradient(circle, #b3ff1f 0%, transparent 70%)",
          filter: "blur(110px)",
          animation: "blob-drift-a 48s ease-in-out infinite",
        }}
      />
      <div
        className="absolute top-1/3 -right-40 h-[60vh] w-[60vh] rounded-full opacity-[0.20]"
        style={{
          background: "radial-gradient(circle, #003eff 0%, transparent 70%)",
          filter: "blur(120px)",
          animation: "blob-drift-b 62s ease-in-out infinite",
        }}
      />
      <div
        className="absolute bottom-[-20%] left-1/4 h-[55vh] w-[55vh] rounded-full opacity-[0.15]"
        style={{
          background: "radial-gradient(circle, #00DBD5 0%, transparent 70%)",
          filter: "blur(110px)",
          animation: "blob-drift-c 56s ease-in-out infinite",
        }}
      />
    </div>
  );
}
