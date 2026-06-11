export const notifQueryKeys = {
  all: ["notif"] as const,
  companies: () => [...notifQueryKeys.all, "companies"] as const,
  profile: () => [...notifQueryKeys.all, "profile"] as const,
  provider: () => [...notifQueryKeys.all, "provider"] as const,
  templates: () => [...notifQueryKeys.all, "templates"] as const,
  rules: () => [...notifQueryKeys.all, "rules"] as const,
  queue: () => [...notifQueryKeys.all, "queue"] as const,
  switch: () => [...notifQueryKeys.all, "switch"] as const,
};
