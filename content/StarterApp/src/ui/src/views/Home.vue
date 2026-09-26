<script setup lang="ts">
// ─────────────────────────────────────────────
// Home
// ─────────────────────────────────────────────
// Landing page placeholder. Replace the contents with your application's first
// real screen — the surrounding shell (header, nav, footer, auth guards) is
// already wired up in App.vue and router/index.ts.
//
// Everything conditional is computed here rather than branched in the template,
// so this file has the same shape however the project was generated.
import { computed } from "vue";
//#if (useAuth)
import { useLoginStore } from "../store/loginStore";

const loginStore = useLoginStore();
//#endif

const greeting = computed(() => {
  //#if (useAuth)
  return `Welcome, ${loginStore.profile?.fullName || "there"}`;
  //#else
  return "Welcome";
  //#endif
});

interface Feature {
  text: string;
  routeName?: string;
  linkText?: string;
}

const features: Feature[] = [
  { text: "Vue 3 + TypeScript + Vite with Pinia and Vue Router" },
  { text: "An Axios client at src/services/api.ts pointed at VITE_API_URL" },
  //#if (useAuth)
  { text: "JWT sign-in, registration, password reset, and admin-only routes" },
  //#endif
  {
    text: "A file upload screen at",
    routeName: "Attachments",
    linkText: "Attachments",
  },
  {
    text: "A help center with editable content at",
    routeName: "Help",
    linkText: "Help",
  },
  { text: "Playwright end-to-end and WCAG 2.2 AA accessibility tests in e2e/" },
];
</script>

<template>
  <div class="container home">
    <div class="mt-5 mb-5">
      <h1 class="title is-3">{{ greeting }}</h1>
      <p class="subtitle is-5">
        This is the starter home page. Replace it with your first real screen.
      </p>

      <div class="content mt-5">
        <p>What is already wired up for you:</p>
        <ul>
          <li v-for="(feature, index) in features" :key="index">
            {{ feature.text }}
            <router-link
              v-if="feature.routeName"
              :to="{ name: feature.routeName }"
            >
              {{ feature.linkText }}
            </router-link>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.home {
  max-width: 60rem;
}
</style>
