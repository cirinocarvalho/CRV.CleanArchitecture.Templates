// ─────────────────────────────────────────────
// Shared helpers for the vee-* form field components
// ─────────────────────────────────────────────
// Every vee-* component is a native form control styled with Bulma and wired
// to vee-validate through useField. The props below are the ones they have in
// common; each component adds whatever its control needs.

export type OptionValue = string | number | boolean;

// Options for dropdowns, radios and checkbox groups: either bare values (used
// as both the label and the value) or { text, value } objects.
export type FieldOption =
  | OptionValue
  | { text: string; value: OptionValue; disabled?: boolean };

export interface BaseFieldProps {
  // Doubles as the vee-validate field name when `name` is not given.
  id?: string;
  name?: string;
  label?: string;
  // Help text shown under the control and linked with aria-describedby.
  desc?: string;
  rules?: string;
  required?: boolean;
  disabled?: boolean;
}

export const optionText = (option: FieldOption): string =>
  typeof option === "object" ? option.text : String(option);

export const optionValue = (option: FieldOption): OptionValue =>
  typeof option === "object" ? option.value : option;

export const optionDisabled = (option: FieldOption): boolean =>
  typeof option === "object" && !!option.disabled;

// Resolves the field name and the ids that tie the label, help text and error
// message to the control for assistive technology.
export function fieldIds(props: BaseFieldProps) {
  const name = props.name || props.id || "";
  const id = props.id || props.name || "";
  return {
    name,
    id,
    descId: `${id}-desc`,
    errorId: `${id}-error`,
  };
}

export function describedBy(
  ids: ReturnType<typeof fieldIds>,
  desc: string | undefined,
  errorMessage: string | undefined,
): string | undefined {
  const parts = [desc ? ids.descId : "", errorMessage ? ids.errorId : ""];
  return parts.filter(Boolean).join(" ") || undefined;
}
