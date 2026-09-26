import { type RouteLocationRaw } from "vue-router";

// A link in the header tabs, the phone menu or the footer.
export interface NavLinkItem {
  href: RouteLocationRaw;
  text: string;
}
