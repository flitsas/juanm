import { describe, expect, it } from "vitest";
import {
  EMPTY_RULE_FORM,
  formatRuleTrigger,
  hasRuleErrors,
  validateRuleForm,
} from "./rule-validation";

describe("validateRuleForm", () => {
  it("exige plantilla y disparador estatal", () => {
    const errors = validateRuleForm({
      ...EMPTY_RULE_FORM,
      name: "Recordatorio",
      triggerType: "state",
      triggerEstado: "",
      emailTemplateId: "tpl-1",
    });
    expect(hasRuleErrors(errors)).toBe(true);
    expect(errors.triggerEstado).toBeTruthy();
  });

  it("valida regla cronológica mínima", () => {
    const errors = validateRuleForm({
      ...EMPTY_RULE_FORM,
      name: "D+3",
      emailTemplateId: "tpl-1",
      triggerType: "chronological",
      triggerDays: "3",
      triggerReference: "fecha_notificacion",
    });
    expect(hasRuleErrors(errors)).toBe(false);
  });
});

describe("formatRuleTrigger", () => {
  it("describe disparador cronológico", () => {
    expect(
      formatRuleTrigger({
        triggerType: "chronological",
        triggerDays: 3,
        triggerReference: "fecha_notificacion",
        triggerEstado: null,
      }),
    ).toBe("3 días desde fecha notificación");
  });
});
