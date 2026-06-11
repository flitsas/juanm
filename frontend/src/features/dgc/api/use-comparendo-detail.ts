"use client";

import { useQuery } from "@tanstack/react-query";
import { fetchComparendoDetail } from "./fetch-comparendo-detail";
import { dgcQueryKeys } from "./query-keys";

export function useComparendoDetail(comparendoId: string | null) {
  return useQuery({
    queryKey: comparendoId
      ? dgcQueryKeys.comparendoDetail(comparendoId)
      : ["dgc", "comparendos", "detail", "idle"],
    queryFn: () => {
      if (!comparendoId) {
        throw new Error("comparendoId is required");
      }
      return fetchComparendoDetail(comparendoId);
    },
    enabled: Boolean(comparendoId),
  });
}
