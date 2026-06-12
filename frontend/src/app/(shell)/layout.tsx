import { AppShell } from "@/components/shell/AppShell";
import { AuthGuard } from "@/features/auth/components/auth-guard";

export default function ShellLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <AuthGuard>
      <AppShell>{children}</AppShell>
    </AuthGuard>
  );
}
