"use client";

import { QueryClientProvider } from "@tanstack/react-query";
import { addLocale, locale, PrimeReactProvider } from "primereact/api";
import "primereact/resources/primereact.min.css";
import "primeicons/primeicons.css";
import { useState } from "react";
import { ThemeProvider } from "@/components/shell/theme-provider";
import { createQueryClient } from "@/lib/query-client";

addLocale("es", {
  firstDayOfWeek: 1,
  dayNames: ["domingo", "lunes", "martes", "miércoles", "jueves", "viernes", "sábado"],
  dayNamesShort: ["dom", "lun", "mar", "mié", "jue", "vie", "sáb"],
  dayNamesMin: ["D", "L", "M", "X", "J", "V", "S"],
  monthNames: [
    "enero",
    "febrero",
    "marzo",
    "abril",
    "mayo",
    "junio",
    "julio",
    "agosto",
    "septiembre",
    "octubre",
    "noviembre",
    "diciembre",
  ],
  monthNamesShort: [
    "ene",
    "feb",
    "mar",
    "abr",
    "may",
    "jun",
    "jul",
    "ago",
    "sep",
    "oct",
    "nov",
    "dic",
  ],
  today: "Hoy",
  clear: "Limpiar",
  chooseDate: "Seleccionar fecha",
});
locale("es");

type ProvidersProps = {
  children: React.ReactNode;
};

export function Providers({ children }: ProvidersProps) {
  const [queryClient] = useState(() => createQueryClient());

  return (
    <ThemeProvider>
      <QueryClientProvider client={queryClient}>
        <PrimeReactProvider
          value={{
            ripple: true,
            pt: {
              tooltip: {
                root: { className: "flit-dock-tooltip-panel" },
              },
            },
          }}
        >
          {children}
        </PrimeReactProvider>
      </QueryClientProvider>
    </ThemeProvider>
  );
}
