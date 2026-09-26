<script setup lang="ts">
// ─────────────────────────────────────────────
// Help
// ─────────────────────────────────────────────
// Every /help route renders this one component; the topic is chosen by path.
// Content lives in src/assets/help-content.ts and the menu in help-sidebar.ts.
import { computed } from "vue";
import { useRoute } from "vue-router";
import SimplePage from "../components/SimplePage.vue";
import { useSidebarStore } from "../store/sidebarStore";
import { helpSidebar } from "../assets/help-sidebar";
import { helpContent } from "../assets/help-content";

const sidebarStore = useSidebarStore();
sidebarStore.addSidebar(helpSidebar);

const route = useRoute();

// Falls back to the overview if the path isn't recognised, so a stale link
// lands somewhere useful rather than on a blank page.
const pageData = computed(() => {
  const path = route.path.replace(/\/+$/, "").toLowerCase() || "/help";
  const page = helpContent[path] ?? helpContent["/help"];
  return {
    title: page.title,
    description: page.description,
    sidebar: "help-sidebar",
    content: page.content,
  };
});

// The version CI stamped into the bundle (VITE_APP_VERSION), shown so support can
// ask which build someone is on. The help center is the one place every visitor
// can reach, signed in or not. A local dev server has no version.
const appVersion = import.meta.env.VITE_APP_VERSION || "local build";
</script>

<template>
  <simple-page :page="pageData" />
  <!-- grey-dark, not grey: Bulma's grey on white is 4.48:1, just under the 4.5:1
       WCAG AA needs for text this size, and the help pages are audited for it. -->
  <p class="container app-version has-text-grey-dark is-size-7 has-text-right">
    StarterApp version {{ appVersion }}
  </p>
</template>

<style scoped lang="scss">
.app-version {
  padding: 0 0.75rem 1rem;
}
</style>
