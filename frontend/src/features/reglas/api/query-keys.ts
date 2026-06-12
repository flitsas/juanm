export const reglasQueryKeys = {
  all: ["reglas"] as const,
  rules: () => [...reglasQueryKeys.all, "rules"] as const,
  contacts: () => [...reglasQueryKeys.all, "contacts"] as const,
};
