import { describe, expect, it } from "vitest";
import { createDefaultConditionRoot, createGroupNode } from "./condition-tree";

describe("createDefaultConditionRoot", () => {
  it("crea grupo AND con un predicado inicial", () => {
    const root = createDefaultConditionRoot();
    expect(root.nodeType).toBe("group");
    expect(root.logicOperator).toBe("and");
    expect(root.children).toHaveLength(1);
    expect(root.children?.[0]?.nodeType).toBe("predicate");
  });
});

describe("createGroupNode", () => {
  it("crea subgrupo con predicado hijo", () => {
    const group = createGroupNode();
    expect(group.nodeType).toBe("group");
    expect(group.children?.[0]?.nodeType).toBe("predicate");
  });
});
