"use client";

import { QueryClientProvider } from "@tanstack/react-query";
import { PrimeReactProvider } from "primereact/api";
import "primereact/resources/primereact.min.css";
import "primeicons/primeicons.css";
import { useState } from "react";
import { createQueryClient } from "@/lib/query-client";

type ProvidersProps = {
  children: React.ReactNode;
};

export function Providers({ children }: ProvidersProps) {
  const [queryClient] = useState(() => createQueryClient());

  return (
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
  );
}
