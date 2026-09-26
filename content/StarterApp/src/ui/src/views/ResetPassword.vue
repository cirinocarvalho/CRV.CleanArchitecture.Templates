<script setup lang="ts">
// ─────────────────────────────────────────────
// Imports
// ─────────────────────────────────────────────
import { ref, computed, onMounted, onUnmounted } from "vue";
import { useRoute } from "vue-router";
import { useLoginStore } from "../store/loginStore";
import { useForm } from "vee-validate";
import VeeTextbox from "../components/vee-textbox.vue";
// Registers the shared `strongPassword` / `confirmed` rules and renders the list.
import PasswordRequirements from "../components/PasswordRequirements.vue";
import ModalSpinner from "../components/modal-spinner.vue";

// ─────────────────────────────────────────────
// Store / route
// ─────────────────────────────────────────────
const route = useRoute();
const loginStore = useLoginStore();
const { handleSubmit } = useForm();

// The email and reset token arrive as query params on the link from the email.
const email = ref(typeof route.query.email === "string" ? route.query.email : "");
const token = typeof route.query.token === "string" ? route.query.token : "";
const hasToken = computed(() => token.length > 0);

// ─────────────────────────────────────────────
// State
// ─────────────────────────────────────────────
const newPassword = ref("");
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
async function doResetPassword() {
  loading.value = true;
  const ok = await loginStore.resetPassword({
    email: email.value,
    token,
    newPassword: newPassword.value,
  });
  loading.value = false;

  if (ok) {
    newPassword.value = "";
    confirmPassword.value = "";
    // Close returns to Login.
    openModal(
      "Password Reset",
      "Your password has been reset. You can now sign in with your new password.",
      "Login",
    );
  } else {
    openModal(
      "Password Reset Failed",
      loginStore.error ?? "Password reset failed.",
    );
  }
}

const onSubmit = handleSubmit(doResetPassword);

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
            <h2 class="title has-text-centered">Reset Password</h2>

            <!-- Missing/invalid link -->
            <div v-if="!hasToken">
              <div class="notification is-warning">
                This password reset link is invalid or incomplete. Please start a
                new password reset.
              </div>
              <p class="has-text-centered mt-3">
                <router-link :to="{ name: 'ForgotPassword' }">
                  Back to Forgot Password
                </router-link>
              </p>
            </div>

            <!-- Reset form -->
            <div v-else>
              <p class="has-text-centered mb-5">
                Enter a new password for
                <strong>{{ email || "your account" }}</strong
                >.
              </p>

              <div class="columns is-centered">
                <div class="column is-12">
                  <form @submit.prevent="onSubmit" novalidate>
                    <vee-textbox
                      id="newPassword"
                      v-model="newPassword"
                      label="New Password"
                      placeholder="••••••••••••••••"
                      type="password"
                      autocomplete="new-password"
                      rules="required|strongPassword"
                      required
                    />

                    <PasswordRequirements />

                    <vee-textbox
                      id="confirmPassword"
                      v-model="confirmPassword"
                      label="Confirm New Password"
                      placeholder="••••••••••••••••"
                      type="password"
                      autocomplete="new-password"
                      :rules="`required|confirmed:${newPassword}`"
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
                        Reset Password
                      </button>
                    </div>

                    <p class="has-text-centered mt-3">
                      <router-link :to="{ name: 'Login' }">
                        Back to Sign In
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
