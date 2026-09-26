<script setup lang="ts">
// Importing the policy module also registers the shared `strongPassword` and
// `confirmed` vee-validate rules used by the password screens.
import { PASSWORD_REQUIREMENTS } from "../utils/passwordPolicy";
</script>

<template>
  <div class="password-requirements">
    <p class="req-title">Your password must include:</p>
    <ul>
      <li v-for="req in PASSWORD_REQUIREMENTS" :key="req">{{ req }}</li>
    </ul>
  </div>
</template>

<style scoped>
.password-requirements {
  font-size: 0.8rem;
  color: #555;
  margin: -0.4rem 0 0.85rem;
  padding-left: 1.25rem;
}
.req-title {
  margin-bottom: 2px;
  font-weight: 600;
}
.password-requirements ul {
  margin: 0;
  padding-left: 0;
  list-style: none;
}
.password-requirements li {
  margin: 0;
  padding-left: 1.15rem;
  position: relative;
}
.password-requirements li::before {
  content: "•";
  position: absolute;
  left: 0.25rem;
}
</style>
