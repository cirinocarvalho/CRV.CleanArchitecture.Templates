import { defineRule } from "vee-validate";

// ─────────────────────────────────────────────
// Password policy (client-side mirror of the server)
// ─────────────────────────────────────────────
// These MUST match the ASP.NET Core Identity options configured in the API
// (Infrastructure/DependencyInjection.cs). The API sets RequiredLength = 8,
// RequireDigit = true, RequireNonAlphanumeric = false, and leaves
// RequireUppercase / RequireLowercase at their default of true. So a valid
// password needs: 8+ chars, an uppercase letter, a lowercase letter, and a digit.

/** Human-readable requirements, shown on every password screen. */
export const PASSWORD_REQUIREMENTS = [
  "At least 8 characters",
  "An uppercase letter (A–Z)",
  "A lowercase letter (a–z)",
  "A number (0–9)",
];

/** Single source of truth for whether a password satisfies the policy. */
export function isStrongPassword(value: string): boolean {
  return (
    typeof value === "string" &&
    value.length >= 8 &&
    /[A-Z]/.test(value) &&
    /[a-z]/.test(value) &&
    /[0-9]/.test(value)
  );
}

// ─────────────────────────────────────────────
// Global vee-validate rules (registered on import)
// ─────────────────────────────────────────────
defineRule("strongPassword", (value: string) => {
  if (isStrongPassword(value)) return true;
  return "Password must be at least 8 characters and include an uppercase letter, a lowercase letter, and a number.";
});

// Value must match the referenced target field (e.g. the password field).
defineRule("confirmed", (value: string, [target]: [string]) => {
  if (value === target) return true;
  return "Passwords do not match.";
});
