export const notifQueryKeys = {
  all: ["notif"] as const,
  companies: () => [...notifQueryKeys.all, "companies"] as const,
  provider: () => [...notifQueryKeys.all, "provider"] as const,
};
