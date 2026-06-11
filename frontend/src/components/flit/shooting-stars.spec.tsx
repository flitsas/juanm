import { cleanup, render } from "@testing-library/react";
import { afterEach, describe, expect, it } from "vitest";
import { ShootingStars } from "./shooting-stars";

describe("ShootingStars", () => {
  afterEach(() => {
    cleanup();
  });

  it("renderiza el número de estelas solicitado", () => {
    const { container } = render(<ShootingStars count={4} />);
    const streaks = container.querySelectorAll("span");
    expect(streaks).toHaveLength(4);
  });

  it("marca el contenedor como decorativo", () => {
    const { container } = render(<ShootingStars />);
    expect(container.firstElementChild?.getAttribute("aria-hidden")).toBe("true");
  });
});
