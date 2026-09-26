<script setup lang="ts">
import { type NavLinkItem } from "./navigation";

defineProps<{ links: NavLinkItem[] }>();
</script>

<template>
  <!-- Hidden on phones: the header's phone menu carries these links there. -->
  <footer class="app-footer is-hidden-mobile">
    <nav aria-label="Footer">
      <ul>
        <li v-for="link in links" :key="link.text">
          <router-link :to="link.href">{{ link.text }}</router-link>
        </li>
      </ul>
    </nav>
  </footer>
</template>

<style lang="scss" scoped>
.app-footer {
  padding: 0.75rem 1.5rem;
  color: #fff;
  background-color: #444;

  ul {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem 2rem;
    list-style: none;
    margin: 0;
  }

  a {
    color: #fff;
    text-decoration: underline;

    &:hover,
    &:focus-visible {
      color: #fff;
      text-decoration: none;
    }
  }
}
</style>
