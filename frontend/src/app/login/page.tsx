import { AuthShell } from "@/features/auth/components/auth-shell";
import { LoginForm } from "@/features/auth/components/login-form";

export const metadata = {
  title: "Ingresar · GDC — Gestión De Comparendos",
  description: "Accede a la plataforma GDC (Gestión De Comparendos)",
};

export default function LoginPage() {
  return (
    <AuthShell>
      <LoginForm />
    </AuthShell>
  );
}
