<script setup lang="ts">
// ─────────────────────────────────────────────
// Imports
// ─────────────────────────────────────────────
import { ref, onMounted, onUnmounted } from "vue";
import { useLoginStore } from "../store/loginStore";
import { useForm } from "vee-validate";
import VeeTextbox from "../components/vee-textbox.vue";
import ModalSpinner from "../components/modal-spinner.vue";

// ─────────────────────────────────────────────
// Store
// ─────────────────────────────────────────────
const loginStore = useLoginStore();
const { handleSubmit } = useForm();

// ─────────────────────────────────────────────
// State
// ─────────────────────────────────────────────
const email = ref("");
const loading = ref(false);

// Modal feedback
const showModal = ref(false);
const modalTitle = ref("");
const modalMessage = ref("");

// ─────────────────────────────────────────────
// Methods
// ─────────────────────────────────────────────
async function doForgotPassword() {
  loading.value = true;
  await loginStore.forgotPassword({ email: email.value });
  loading.value = false;
  // Always show the same message (whether or not the email exists) to avoid
  // revealing which addresses are registered.
  modalTitle.value = "Check Your Email";
  modalMessage.value =
    "If an account exists for that email address, you'll receive a password reset link shortly.";
  showModal.value = true;
}

const onSubmit = handleSubmit(doForgotPassword);

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
            <h2 class="title has-text-centered">Forgot Password</h2>

            <div>
              <p class="has-text-centered mb-5">
                Enter your email address and we'll send you a link to reset your
                password.
              </p>

              <div class="columns is-centered">
                <div class="column is-12">
                  <form @submit.prevent="onSubmit" novalidate>
                    <vee-textbox
                      id="forgotEmail"
                      v-model="email"
                      label="Email Address"
                      placeholder="you@yourdomain.com"
                      type="email"
                      autocomplete="username"
                      rules="required|email"
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
                        Send Reset Link
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
