export type ConditionNode = {
  nodeType: string;
  logicOperator?: string | null;
  fieldKey?: string | null;
  comparisonOperator?: string | null;
  comparisonValue?: string | null;
  children?: ConditionNode[] | null;
};

export type ReglasRule = {
  id: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  pdfTemplateId: string;
  emailSubject: string;
  emailBodyHtml: string;
  secretariatContactId?: string | null;
  conditionRoot?: ConditionNode | null;
  createdAt: string;
  updatedAt?: string | null;
};

export type ReglasRuleListResult = {
  items: ReglasRule[];
};

export type SaveReglasRulePayload = {
  name: string;
  description?: string | null;
  isActive: boolean;
  pdfTemplateId: string;
  emailSubject: string;
  emailBodyHtml: string;
  secretariatContactId?: string | null;
  conditionRoot?: ConditionNode | null;
};

export type ReglasSecretariatContact = {
  id: string;
  secretariatCode: string;
  secretariatName: string;
  contactName: string;
  contactEmail: string;
  contactPhone?: string | null;
  isActive: boolean;
};

export type ReglasContactListResult = {
  items: ReglasSecretariatContact[];
};
