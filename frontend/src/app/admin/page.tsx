import { AdminConsole } from "@/features/auth/components/admin-console";
import { SuperAdminGuard } from "@/features/auth/components/super-admin-guard";

export const metadata = {
  title: "Administración · GDC 2.0",
  description: "Multi-compañía, usuarios y roles",
};

export default function AdminPage() {
  return (
    <SuperAdminGuard>
      <AdminConsole />
    </SuperAdminGuard>
  );
}
