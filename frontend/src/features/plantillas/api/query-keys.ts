export const plantillasQueryKeys = {
  all: ["plantillas"] as const,
  templates: () => [...plantillasQueryKeys.all, "templates"] as const,
  template: (id: string) => [...plantillasQueryKeys.all, "template", id] as const,
  systemVariables: () => [...plantillasQueryKeys.all, "system-variables"] as const,
};
