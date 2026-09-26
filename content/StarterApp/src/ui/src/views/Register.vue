<script setup lang="ts">
// ─────────────────────────────────────────────
// Imports
// ─────────────────────────────────────────────
import { ref, onMounted, onUnmounted } from "vue";
import { useLoginStore } from "../store/loginStore";
import { useForm } from "vee-validate";
import VeeTextbox from "../components/vee-textbox.vue";
// Registers the shared `strongPassword` / `confirmed` rules and renders the list.
import PasswordRequirements from "../components/PasswordRequirements.vue";
import ModalSpinner from "../components/modal-spinner.vue";

// ─────────────────────────────────────────────
// Store / Router
// ─────────────────────────────────────────────
const loginStore = useLoginStore();
const { handleSubmit } = useForm();

// ─────────────────────────────────────────────
// State
// ─────────────────────────────────────────────
const fullName = ref("");
const email = ref("");
const password = ref("");
const confirmPassword = ref("");
const loading = ref(false);

// Modal feedback
const showModal = ref(false);
const modalTitle = ref("");
const modalMessage = ref("");
const modalRoute = ref("");

function openModal(title: string, message: string, routeName = "") {
  modalTitle.value = title;
  modalMessage.value = message;
  modalRoute.value = routeName;
  showModal.value = true;
}

// ─────────────────────────────────────────────
// Methods
// ─────────────────────────────────────────────
async function doRegister() {
  loading.value = true;
  const ok = await loginStore.register({
    fullName: fullName.value,
    email: email.value,
    password: password.value,
  });
  loading.value = false;

  if (ok) {
    // Account is created but inactive until an admin approves it. Close returns
    // to Login.
    openModal(
      "Registration Received",
      "Your account has been created and is pending administrator approval. You'll receive an email once it's activated, and then you can sign in.",
      "Login",
    );
  } else {
    openModal("Registration Failed", loginStore.error ?? "Registration failed.");
  }
}

const onSubmit = handleSubmit(doRegister);

onMounted(() => {
  document.body.style.overflow = "hidden";
});
onUnmounted(() => {
  document.body.style.overflow = "";
});
</script>

<template>
  <div class="auth-page">
    <div class="container">
      <div class="columns">
        <div class="column card is-three-fifths is-offset-one-fifth">
          <div class="section">
            <h2 class="title has-text-centered">Create an Account</h2>

            <div>
              <div class="columns is-centered">
                <div class="column is-12">
                  <form @submit.prevent="onSubmit" novalidate>
                    <vee-textbox
                      id="regFullName"
                      v-model="fullName"
                      label="Full Name"
                      placeholder="Jane Doe"
                      autocomplete="name"
                      rules="required"
                      required
                    />

                    <vee-textbox
                      id="regEmail"
                      v-model="email"
                      label="Email Address"
                      placeholder="you@yourdomain.com"
                      type="email"
                      autocomplete="username"
                      rules="required|email"
                      required
                    />

                    <vee-textbox
                      id="regPassword"
                      v-model="password"
                      label="Password"
                      placeholder="••••••••••••••••"
                      type="password"
                      autocomplete="new-password"
                      rules="required|strongPassword"
                      required
                    />

                    <PasswordRequirements />

                    <vee-textbox
                      id="regConfirmPassword"
                      v-model="confirmPassword"
                      label="Confirm Password"
                      placeholder="••••••••••••••••"
                      type="password"
                      autocomplete="new-password"
                      :rules="`required|confirmed:${password}`"
                      required
                    />

                    <div
                      class="field mt-4 is-grouped is-justify-content-center"
                    >
                      <button
                        class="button is-primary"
                        type="submit"
                        :class="{ 'is-loading': loading }"
                        :disabled="loading"
                      >
                        Register
                      </button>
                    </div>

                    <p class="has-text-centered mt-3">
                      Already have an account?
                      <!-- Underlined: a link inside body text must not rely on
                           colour alone to be recognisable (WCAG 1.4.1). -->
                      <router-link :to="{ name: 'Login' }" class="is-underlined">
                        Sign in
                      </router-link>
                    </p>
                  </form>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <ModalSpinner
      v-model="showModal"
      :title="modalTitle"
      :message="modalMessage"
      :is-loading="false"
      :route-name="modalRoute"
    />
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
</style>
