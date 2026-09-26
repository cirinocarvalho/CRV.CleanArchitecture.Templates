<script setup lang="ts">
// ─────────────────────────────────────────────
// Imports
// ─────────────────────────────────────────────
import { ref } from "vue";
import { useLoginStore } from "../store/loginStore";
import { useForm } from "vee-validate";
import VeeTextbox from "../components/vee-textbox.vue";
// Registers the shared `strongPassword` / `confirmed` rules and renders the list.
import PasswordRequirements from "../components/PasswordRequirements.vue";
import ModalSpinner from "../components/modal-spinner.vue";

// ─────────────────────────────────────────────
// Store
// ─────────────────────────────────────────────
const loginStore = useLoginStore();
const { handleSubmit } = useForm();

// ─────────────────────────────────────────────
// State
// ─────────────────────────────────────────────
const currentPassword = ref("");
const newPassword = ref("");
const confirmPassword = ref("");
const loading = ref(false);

// Modal feedback
const showModal = ref(false);
const modalTitle = ref("");
const modalMessage = ref("");

function openModal(title: string, message: string) {
  modalTitle.value = title;
  modalMessage.value = message;
  showModal.value = true;
}

// ─────────────────────────────────────────────
// Methods
// ─────────────────────────────────────────────
async function doChangePassword() {
  loading.value = true;
  const ok = await loginStore.changePassword({
    currentPassword: currentPassword.value,
    newPassword: newPassword.value,
  });
  loading.value = false;

  if (ok) {
    currentPassword.value = "";
    newPassword.value = "";
    confirmPassword.value = "";
    openModal("Password Changed", "Your password has been changed.");
  } else {
    openModal(
      "Password Change Failed",
      loginStore.error ?? "Unable to change password.",
    );
  }
}

const onSubmit = handleSubmit(doChangePassword);
</script>

<template>
  <div class="container content change-password-page">
    <div class="columns is-centered">
      <div class="column is-12">
        <div class="section-panel">
          <div class="panel change-password-panel">
            <p class="panel-heading">Change Password</p>
            <div class="panel-block">
              <div class="change-password-form">
                <form @submit.prevent="onSubmit" novalidate>
                  <vee-textbox
                    id="currentPassword"
                    v-model="currentPassword"
                    label="Current Password"
                    placeholder="••••••••••••••••"
                    type="password"
                    autocomplete="current-password"
                    rules="required"
                    required
                  />

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

                  <div class="field mt-4 is-grouped is-justify-content-center">
                    <button
                      class="button is-primary"
                      type="submit"
                      :class="{ 'is-loading': loading }"
                      :disabled="loading"
                    >
                      Update Password
                    </button>
                  </div>
                </form>
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
.change-password-page {
  padding: 5px;
}

/* Bordered box wrapping a shadowed panel */
.section-panel {
  border: 1px solid #dbdbdb;
  padding: 10px;
  background: #fff;
}
.change-password-panel .panel-heading {
  background-color: #2176d2;
  color: #fff;
  font-weight: 600;
}
.change-password-form {
  width: 100%;
  padding: 8px 4px;
}
</style>
