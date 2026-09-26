<script setup lang="ts">
// ─────────────────────────────────────────────
// AccountMenu
// ─────────────────────────────────────────────
// Signed-in user's dropdown in the app header. Lives in its own component so
// App.vue carries no authentication-specific markup and stays the same file
// whether or not the project was generated with authentication.
import { onMounted, onUnmounted, ref } from "vue";
import { useLoginStore } from "../store/loginStore";
import router from "../router/index";

const loginStore = useLoginStore();

const isOpen = ref(false);
const root = ref<HTMLElement | null>(null);

function toggle() {
  isOpen.value = !isOpen.value;
}

function close() {
  isOpen.value = false;
}

// Close when a click lands anywhere outside the menu.
function onDocumentClick(e: MouseEvent) {
  if (root.value && !root.value.contains(e.target as Node)) close();
}

onMounted(() => document.addEventListener("click", onDocumentClick));
onUnmounted(() => document.removeEventListener("click", onDocumentClick));

function logOff() {
  close();
  loginStore.signOut();
  router.push({ name: "Login" });
}
</script>

<template>
  <div
    v-if="loginStore.isAuthenticated"
    ref="root"
    class="dropdown is-right account-menu"
    :class="{ 'is-active': isOpen }"
    @mouseleave="close"
    @keydown.esc="close"
  >
    <div class="dropdown-trigger">
      <button
        type="button"
        class="account-menu__trigger"
        aria-haspopup="true"
        aria-controls="account-menu-items"
        :aria-expanded="isOpen"
        @click="toggle"
      >
        <span class="icon" aria-hidden="true">
          <i class="fas fa-user-circle"></i>
        </span>
        <span>{{ loginStore.profile?.fullName || "Account" }}</span>
      </button>
    </div>
    <div id="account-menu-items" class="dropdown-menu">
      <ul class="dropdown-content">
        <li>
          <router-link
            :to="{ name: 'ChangePassword' }"
            class="dropdown-item"
            @click="close"
          >
            Change Password
          </router-link>
        </li>
        <li>
          <button type="button" class="dropdown-item" @click="logOff">
            Sign Out
          </button>
        </li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
.account-menu__trigger {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  padding: 0.5rem 0.75rem;
  color: #fff;
  background: none;
  border: none;
  font: inherit;
  font-weight: 700;
  cursor: pointer;
}

.account-menu__trigger:hover,
.account-menu__trigger:focus-visible {
  text-decoration: underline;
}

.dropdown-content {
  list-style: none;
  margin: 0;
}

/* "Sign Out" performs a JS action, so it is a real <button> for keyboard
   operability; strip the native chrome so it matches the link above it. */
button.dropdown-item {
  width: 100%;
  text-align: left;
  background: none;
  border: none;
  font: inherit;
  cursor: pointer;
}

button.dropdown-item:hover {
  background-color: whitesmoke;
}
</style>
