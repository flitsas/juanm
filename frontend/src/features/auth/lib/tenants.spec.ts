import { describe, expect, it } from "vitest";
import { groupUsersByTenant } from "./tenants";

describe("groupUsersByTenant", () => {
  it("agrupa usuarios por tenantId", () => {
    const groups = groupUsersByTenant([
      {
        id: "1",
        tenantId: "tenant-a",
        email: "a@x.com",
        status: "Active",
      },
      {
        id: "2",
        tenantId: "tenant-a",
        email: "b@x.com",
        status: "Pending",
      },
      {
        id: "3",
        tenantId: "tenant-b",
        email: "c@x.com",
        status: "Active",
      },
    ]);

    expect(groups).toHaveLength(2);
    expect(groups.find((g) => g.tenantId === "tenant-a")?.users).toHaveLength(2);
  });
});
