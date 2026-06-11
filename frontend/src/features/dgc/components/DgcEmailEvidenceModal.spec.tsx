import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { DgcEmailEvidenceModal } from "./DgcEmailEvidenceModal";

describe("DgcEmailEvidenceModal", () => {
  it("elimina scripts maliciosos del HTML de evidencia", () => {
    render(
      <DgcEmailEvidenceModal
        open
        html={'<p>OK</p><script>alert("x")</script>'}
        loading={false}
        error={null}
        onClose={vi.fn()}
      />,
    );

    const container = screen.getByTestId("email-evidence-html");
    expect(container.innerHTML).toContain("<p>OK</p>");
    expect(container.innerHTML).not.toContain("<script>");
  });

  it("renderiza HTML completo de evidencia", () => {
    render(
      <DgcEmailEvidenceModal
        open
        html="<h1>Título correo</h1><p>Cuerpo</p>"
        loading={false}
        error={null}
        onClose={vi.fn()}
      />,
    );

    const container = screen.getByTestId("email-evidence-html");
    expect(container.innerHTML).toContain("<h1>Título correo</h1>");
    expect(container.innerHTML).toContain("<p>Cuerpo</p>");
  });

  it("no renderiza cuando está cerrado", () => {
    render(
      <DgcEmailEvidenceModal
        open={false}
        html="<p>x</p>"
        loading={false}
        error={null}
        onClose={vi.fn()}
      />,
    );

    expect(screen.queryByTestId("dgc-email-evidence-modal")).toBeNull();
  });
});
