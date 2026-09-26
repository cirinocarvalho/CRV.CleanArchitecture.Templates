<script setup lang="ts">
// ─────────────────────────────────────────────
// AppHeader
// ─────────────────────────────────────────────
// Title bar with an optional right-hand slot (the account menu), a tab bar of
// navigation links on tablet and up, and a toggled menu on phones. Which links
// to show is decided by the caller.
import { ref, watch } from "vue";
import { useRoute } from "vue-router";
import { type NavLinkItem } from "./navigation";

defineProps<{
  appTitle: string;
  appSubtitle?: string;
  navLinks: NavLinkItem[];
  // Links for the phone menu. The footer is hidden on phones, so this list
  // usually carries its links as well.
  mobileLinks?: NavLinkItem[];
}>();

const menuOpen = ref(false);

// Close the phone menu once a link in it has been followed.
const route = useRoute();
watch(
  () => route.fullPath,
  () => {
    menuOpen.value = false;
  },
);
</script>

<template>
  <header class="app-header">
    <div class="app-header__bar">
      <div class="app-header__brand">
        <router-link to="/" class="app-header__title">{{ appTitle }}</router-link>
        <p v-if="appSubtitle" class="app-header__subtitle">{{ appSubtitle }}</p>
      </div>
      <div class="app-header__right">
        <slot name="right-nav" />
        <button
          type="button"
          class="app-header__menu-toggle is-hidden-tablet"
          aria-controls="app-mobile-nav"
          :aria-expanded="menuOpen"
          @click="menuOpen = !menuOpen"
        >
          <span class="icon" aria-hidden="true">
            <i :class="menuOpen ? 'fas fa-times' : 'fas fa-bars'"></i>
          </span>
          <span>Menu</span>
        </button>
      </div>
    </div>

    <nav class="app-header__tabs is-hidden-mobile" aria-label="Main">
      <router-link
        v-for="link in navLinks"
        :key="link.text"
        :to="link.href"
        class="app-header__tab"
      >
        {{ link.text }}
      </router-link>
    </nav>

    <nav
      v-show="menuOpen"
      id="app-mobile-nav"
      class="app-header__mobile-nav is-hidden-tablet"
      aria-label="Main menu"
    >
      <ul>
        <li v-for="link in mobileLinks ?? navLinks" :key="link.text">
          <router-link :to="link.href">{{ link.text }}</router-link>
        </li>
      </ul>
    </nav>
  </header>
</template>

<style lang="scss" scoped>
$brand: #0f4d8f;

.app-header__bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.75rem 1.5rem;
  color: #fff;
  background-color: $brand;
}

.app-header__title {
  color: #fff;
  font-size: 1.5rem;
  font-weight: 700;
  line-height: 1.2;

  &:hover,
  &:focus-visible {
    color: #fff;
    text-decoration: underline;
  }
}

.app-header__subtitle {
  font-size: 0.9rem;
}

.app-header__right {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.app-header__menu-toggle {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.5rem 0.75rem;
  color: #fff;
  background: none;
  border: 1px solid rgba(255, 255, 255, 0.6);
  border-radius: 4px;
  font: inherit;
  cursor: pointer;
}

/* Equal-width tabs: an inline grid of 1fr columns shrink-wraps to its content,
   so every column resolves to the widest label and the bar stays even however
   many screens there are. */
.app-header__tabs {
  display: inline-grid;
  grid-auto-flow: column;
  grid-auto-columns: 1fr;
  padding: 0 1.5rem;
}

.app-header__tab {
  padding: 0.75rem 1.5rem;
  color: $brand;
  font-weight: 700;
  text-align: center;
  border-bottom: 4px solid transparent;

  &:hover {
    background-color: #eef4fb;
  }

  &.router-link-active {
    border-bottom-color: $brand;
  }
}

.app-header {
  border-bottom: 1px solid #dbdbdb;
}

.app-header__mobile-nav {
  background-color: $brand;

  ul {
    list-style: none;
    margin: 0;
  }

  a {
    display: block;
    padding: 0.75rem 1.5rem;
    color: #fff;
    border-top: 1px solid rgba(255, 255, 255, 0.2);

    &:hover,
    &.router-link-active {
      background-color: rgba(255, 255, 255, 0.12);
    }
  }
}
</style>
