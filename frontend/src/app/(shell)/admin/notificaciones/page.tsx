import { redirect } from "next/navigation";

export default function NotifAdminRedirectPage() {
  redirect("/admin?tab=notificaciones");
}
