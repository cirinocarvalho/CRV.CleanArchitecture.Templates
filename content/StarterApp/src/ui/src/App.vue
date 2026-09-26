<script setup lang="ts">
// ─────────────────────────────────────────────
// Imports
// ─────────────────────────────────────────────
import { computed, onMounted, onUnmounted, ref, type Component } from "vue";
import { useAppStore } from "./store/appStore";
import AppHeader from "./components/AppHeader.vue";
import AppFooter from "./components/AppFooter.vue";
import { type NavLinkItem } from "./components/navigation";
//#if (useAuth)
import { useLoginStore } from "./store/loginStore";
import { isTokenExpired } from "./services/auth";
import AccountMenu from "./components/AccountMenu.vue";
import router from "./router/index";
//#endif

// ─────────────────────────────────────────────
// Store
// ─────────────────────────────────────────────
const appStore = useAppStore();
//#if (useAuth)
const loginStore = useLoginStore();
//#endif

// ─────────────────────────────────────────────
// Header
// ─────────────────────────────────────────────
// Everything the header renders is decided here rather than with conditionals
// in the template, so this file is identical in shape whether or not the
// project was generated with authentication.
//#if (useAuth)
const accountMenu = ref<Component | null>(AccountMenu);
//#else
const accountMenu = ref<Component | null>(null);
//#endif

const navLinks = computed<NavLinkItem[]>(() => [
  { href: { name: "Home" }, text: "Home" },
  { href: { name: "Attachments" }, text: "Attachments" },
  //#if (useAuth)
  ...(loginStore.roles.includes("Admin")
    ? [{ href: { name: "Admin" }, text: "Admin" }]
    : []),
  //#endif
  { href: { name: "Help" }, text: "Help Center" },
]);

// Optional line under the header showing the signed-in user's organisation.
const contextLabel = computed<string | null>(() => {
  //#if (useAuth)
  if (!loginStore.isAuthenticated) return null;
  return loginStore.profile?.company || null;
  //#else
  return null;
  //#endif
});

const footerLinks: NavLinkItem[] = [
  { href: { name: "Help-terms-of-use" }, text: "Terms of Use" },
  { href: { name: "Help-privacy" }, text: "Privacy Policy" },
  { href: { name: "Help-accessibility" }, text: "Accessibility" },
];

// The footer is hidden on phones (see the template below), so the policy links
// it carries move into the mobile menu — otherwise they would be unreachable
// on a small screen.
const mobileLinks = computed(() => [...navLinks.value, ...footerLinks]);

//#if (useAuth)
// ─────────────────────────────────────────────
// Session expiry
// ─────────────────────────────────────────────
// When the access token expires, sign out and return to Login automatically,
// so the user isn't left on a page where every request silently fails.
let expiryTimer: ReturnType<typeof setInterval> | undefined;

function enforceTokenExpiry() {
  if (loginStore.isAuthenticated && isTokenExpired()) {
    loginStore.signOut();
    if (router.currentRoute.value.name !== "Login") {
      router.push({ name: "Login" });
    }
  }
}

//#endif
// ─────────────────────────────────────────────
// Lifecycle
// ─────────────────────────────────────────────
onMounted(() => {
  appStore.generateAndSetSessionToken();
  //#if (useAuth)
  enforceTokenExpiry();
  expiryTimer = setInterval(enforceTokenExpiry, 30000);
  //#endif
});

onUnmounted(() => {
  //#if (useAuth)
  if (expiryTimer) clearInterval(expiryTimer);
  //#endif
});
</script>

<template>
  <div id="app">
    <AppHeader
      app-title="StarterApp"
      app-subtitle="Replace this subtitle with what your application does"
      :nav-links="navLinks"
      :mobile-links="mobileLinks"
    >
      <template #right-nav>
        <component :is="accountMenu" v-if="accountMenu" />
      </template>
    </AppHeader>
    <main>
      <div class="has-text-centered" v-if="contextLabel">
        <p class="is-4 company-context">
          <strong>{{ contextLabel }}</strong>
        </p>
      </div>
      <router-view />
    </main>
    <AppFooter :links="footerLinks" />
  </div>
</template>

<style lang="scss" scoped>
/* Header, content, footer: the content grows so the footer sits at the bottom
   of the viewport on short pages and after the content on long ones. */
#app {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}

main {
  flex: 1;
  padding-bottom: 2rem;
}
</style>
