import { AdminConsole } from "@/features/auth/components/admin-console";
import { SuperAdminGuard } from "@/features/auth/components/super-admin-guard";

export const metadata = {
  title: "Super Admin · GDC 2.0",
  description: "Consola de usuarios y matriz RBAC",
};

export default function AdminPage() {
  return (
    <SuperAdminGuard>
      <AdminConsole />
    </SuperAdminGuard>
  );
}
