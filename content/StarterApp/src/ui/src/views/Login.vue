<script setup lang="ts">
// ─────────────────────────────────────────────
// Imports
// ─────────────────────────────────────────────
import { ref, computed, onBeforeMount, onMounted, onUnmounted } from "vue";
import { useLoginStore } from "../store/loginStore";
import { useForm } from "vee-validate";
import router from "../router/index";
import VeeTextbox from "../components/vee-textbox.vue";

// ─────────────────────────────────────────────
// Store
// ─────────────────────────────────────────────
const loginStore = useLoginStore();
const { handleSubmit } = useForm();

// ─────────────────────────────────────────────
// State
// ─────────────────────────────────────────────
const pageTitle = "Welcome to StarterApp";
// Registration and password reset exist only with local (JWT) identity; under
// Entra ID those routes are not generated, so their links must not render.
//#if (useLocalIdentity)
const hasLocalIdentity = true;
//#else
const hasLocalIdentity = false;
//#endif
const email = ref("");
const password = ref("");
const loading = ref(false);

const authError = computed<string | null>(() => loginStore.error);

// ─────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────
function routeUser() {
  if (loginStore.isAuthenticated) {
    router.push({ name: "Home" });
  }
}

// ─────────────────────────────────────────────
// Lifecycle
// ─────────────────────────────────────────────
onBeforeMount(() => {
  routeUser();
});

onMounted(() => {
  document.body.style.overflow = "hidden";
});

onUnmounted(() => {
  document.body.style.overflow = "";
});

// ─────────────────────────────────────────────
// Methods
// ─────────────────────────────────────────────
async function doLogin() {
  if (!email.value || !password.value) return;
  loading.value = true;
  const ok = await loginStore.signIn({
    email: email.value,
    password: password.value,
  });
  loading.value = false;
  if (ok) routeUser();
}

const onSubmit = handleSubmit(doLogin);
</script>

<template>
  <div class="auth-page">
    <div class="content">
      <div class="container">
        <div class="columns">
          <div class="column card is-three-fifths is-offset-one-fifth">
            <div class="section">
              <div class="content">
                <h2 class="has-text-centered">{{ pageTitle }}</h2>
                <!-- Replace with a short description of what your application does. -->
                <p>Sign in to continue to StarterApp.</p>
              </div>

              <div class="columns is-centered">
                <div class="column is-12">
                  <h1 class="title is-4 mb-4">Sign In</h1>
                  <form @submit.prevent="onSubmit" novalidate>
                    <vee-textbox
                      id="loginEmail"
                      v-model="email"
                      label="Email address"
                      placeholder="Email address"
                      type="email"
                      autocomplete="username"
                      rules="required|email"
                      required
                    />

                    <vee-textbox
                      id="loginPassword"
                      v-model="password"
                      label="Password"
                      placeholder="••••••••••••"
                      type="password"
                      autocomplete="current-password"
                      rules="required"
                      required
                    />

                    <div
                      v-if="authError"
                      class="notification is-danger is-light"
                    >
                      {{ authError }}
                    </div>
                    <div v-if="hasLocalIdentity" class="mt-3 forgot-password">
                      <router-link
                        :to="{ name: 'ForgotPassword' }"
                        class="router-link"
                      >
                        <b>Forgot your password?</b>
                      </router-link>
                    </div>
                    <div
                      class="field mt-4 is-grouped is-justify-content-center"
                    >
                      <div class="control is-12">
                        <button
                          class="button is-primary is-fullwidth"
                          type="submit"
                          :class="{ 'is-loading': loading }"
                          :disabled="loading"
                        >
                          Sign in
                        </button>
                        <router-link
                          v-if="hasLocalIdentity"
                          :to="{ name: 'Register' }"
                          class="button light-button is-fullwidth mt-2"
                        >
                          <b>Create an account</b>
                        </router-link>
                      </div>
                    </div>
                  </form>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.auth-page {
  min-height: 100vh;
  overflow: hidden;
  position: relative;

  &::before {
    content: "";
    position: fixed;
    inset: 0;
    background: linear-gradient(135deg, #0f4d8f 0%, #2a7fd4 100%);
    z-index: 0;
  }

  &::after {
    content: "";
    position: fixed;
    inset: 0;
    background: rgba(255, 255, 255, 0.75);
    z-index: 1;
  }

  .container {
    position: relative;
    z-index: 2;
  }
}

.vertical-align-top-all {
  * {
    vertical-align: top;
  }
}

// Match the left text inset of the input controls (Bulma input padding-left).
.forgot-password {
  padding-left: calc(0.75em - 5px);
}
</style>
