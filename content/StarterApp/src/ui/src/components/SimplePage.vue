<script setup lang="ts">
// ─────────────────────────────────────────────
// SimplePage
// ─────────────────────────────────────────────
// Static content page with an optional sidebar. Every help topic renders
// through here, so all of them share one layout.
import Sidebar from "./Sidebar.vue";

interface Page {
  title: string;
  description?: string;
  sidebar?: string;
  content?: string;
  [key: string]: unknown;
}

interface Props {
  page: Page;
  isMobile?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  isMobile: false,
});
</script>
<template>
  <div class="container">
    <div class="columns">
      <div v-if="props.page.sidebar && !props.isMobile" class="column is-3">
        <sidebar :sidebar="props.page.sidebar" />
      </div>
      <div class="column">
        <div class="content content-narrow">
          <h1>
            {{ props.page.title }}
          </h1>
          <p v-if="props.page.description">
            {{ props.page.description }}
          </p>
          <!-- The only v-html in the app. page.content is authored markup from
               src/assets/help-content.ts, never API data or user input. Keep it
               that way: anything dynamic must go through {{ }}. -->
          <!-- eslint-disable-next-line vue/no-v-html -->
          <div v-html="props.page.content"></div>
        </div>
      </div>
      <div v-if="props.page.sidebar && props.isMobile" class="column is-3">
        <sidebar :sidebar="props.page.sidebar" />
      </div>
    </div>
  </div>
</template>
