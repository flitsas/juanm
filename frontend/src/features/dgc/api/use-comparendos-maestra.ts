"use client";

import { useQuery } from "@tanstack/react-query";
import type { ComparendoMaestraFilters } from "../lib/comparendo-maestra.types";
import { fetchComparendosMaestra } from "./fetch-comparendos-maestra";
import { dgcQueryKeys } from "./query-keys";

export function useComparendosMaestra(filters: ComparendoMaestraFilters) {
  return useQuery({
    queryKey: dgcQueryKeys.comparendosMaestra(filters),
    queryFn: ({ signal }) => fetchComparendosMaestra(filters, { signal }),
  });
}
