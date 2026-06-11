import type { RuleTriggerType, TriggerReference } from "./notif.types";

export type RuleFormState = {
  emailTemplateId: string;
  name: string;
  triggerType: RuleTriggerType | "";
  triggerDays: string;
  triggerReference: TriggerReference | "";
  triggerEstado: string;
  isActive: boolean;
};

export type RuleFieldErrors = Partial<Record<keyof RuleFormState, string>>;

export const EMPTY_RULE_FORM: RuleFormState = {
  emailTemplateId: "",
  name: "",
  triggerType: "",
  triggerDays: "3",
  triggerReference: "fecha_notificacion",
  triggerEstado: "",
  isActive: true,
};

export function validateRuleForm(state: RuleFormState): RuleFieldErrors {
  const errors: RuleFieldErrors = {};

  if (!state.name.trim()) {
    errors.name = "El nombre es obligatorio.";
  }

  if (!state.emailTemplateId) {
    errors.emailTemplateId = "Seleccione una plantilla.";
  }

  if (!state.triggerType) {
    errors.triggerType = "Seleccione el tipo de disparador.";
  }

  if (state.triggerType === "chronological") {
    const days = Number.parseInt(state.triggerDays, 10);
    if (Number.isNaN(days) || days < 0) {
      errors.triggerDays = "Los días deben ser un número mayor o igual a 0.";
    }
    if (!state.triggerReference) {
      errors.triggerReference = "Seleccione la fecha de referencia.";
    }
  }

  if (state.triggerType === "state" && !state.triggerEstado.trim()) {
    errors.triggerEstado = "Indique el estado del comparendo.";
  }

  return errors;
}

export function hasRuleErrors(errors: RuleFieldErrors): boolean {
  return Object.keys(errors).length > 0;
}

export function formatRuleTrigger(rule: {
  triggerType: string;
  triggerDays: number | null;
  triggerReference: string | null;
  triggerEstado: string | null;
}): string {
  if (rule.triggerType === "chronological") {
    const ref =
      rule.triggerReference === "fecha_comparendo" ? "fecha comparendo" : "fecha notificación";
    return `${rule.triggerDays ?? 0} días desde ${ref}`;
  }
  if (rule.triggerType === "state") {
    return `Estado: ${rule.triggerEstado ?? "—"}`;
  }
  return rule.triggerType;
}
