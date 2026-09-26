// ─────────────────────────────────────────────
// Help center navigation
// ─────────────────────────────────────────────
// The left-hand menu shown on every help page. Each `href` must have a matching
// route in src/router/index.ts and a matching entry in help-content.ts — add all
// three together, or the link will dead-end.
//
// A .ts module rather than a .json one so the topics can be conditioned on the
// options this project was generated with (JSON imported by Vite may not carry
// comments).
import type { Sidebar } from "../store/sidebarStore";

export const helpSidebar: Sidebar = {
  slug: "help-sidebar",
  items: [
    {
      href: "/help",
      text: "Overview",
    },
    //#if (useAuth)
    {
      href: "/help/getting-started",
      text: "Getting Started",
      children: [
        //#if (useLocalIdentity)
        {
          href: "/help/getting-started/account",
          text: "Creating an Account",
        },
        //#endif
        {
          href: "/help/getting-started/signing-in",
          text: "Signing In",
        },
      ],
    },
    //#endif
    {
      href: "/help/attachments",
      text: "Attachments",
    },
    //#if (useAuth)
    {
      href: "/help/admin",
      text: "Administration",
    },
    //#endif
    {
      href: "/help/faq",
      text: "FAQ",
    },
    {
      href: "/help/contact",
      text: "Contact Support",
    },
    {
      href: "/help/policies",
      text: "Policies",
      children: [
        {
          href: "/help/terms-of-use",
          text: "Terms of Use",
        },
        {
          href: "/help/accessibility",
          text: "Accessibility",
        },
        {
          href: "/help/privacy",
          text: "Privacy Policy",
        },
      ],
    },
  ],
};
