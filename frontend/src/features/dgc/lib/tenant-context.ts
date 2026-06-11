export function getTenantId(): string {
  return (
    process.env.NEXT_PUBLIC_TENANT_ID ?? "22222222-2222-2222-2222-222222222222"
  );
}
