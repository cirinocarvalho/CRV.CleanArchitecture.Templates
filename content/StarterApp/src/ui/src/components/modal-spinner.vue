<script setup lang="ts">
import { nextTick, ref, useId, watch } from "vue";
import router from "../router";
import { useAppStore } from "../store/appStore";
import LoadingSpinner from "./loading-spinner.vue";

const appStore = useAppStore();

// One label/value row of the optional detail list.
interface ModalDetail {
  label: string;
  value: string;
}

const props = withDefaults(
  defineProps<{
    modelValue: boolean;
    title: string;
    // Plain text, NOT markup. Rendered through {{ }} so anything reaching it -
    // API error strings, user emails, URL query values - cannot inject script.
    // Line breaks: embed "\n"; the .modal-message rule below honours them.
    message: string;
    isLoading: boolean;
    // Optional label/value rows rendered as an escaped list under the message.
    details?: ModalDetail[];
    // Optional: enables the Yes/No confirm mode; plain message
    // modals leave it defaulted to false.
    isProceed?: boolean;
    routeName?: string;
  }>(),
  {
    modelValue: false,
    title: "",
    message: "",
    isLoading: false,
    details: () => [],
    isProceed: false,
    routeName: "",
  },
);

const emit = defineEmits<{
  (e: "update:modelValue", value: boolean): void;
  (e: "proceed"): void;
}>();

function close() {
  if (props.routeName) {
    router.push({
      name: props.routeName,
      query: {
        token: appStore.sessionToken,
      },
    });
  }
  emit("update:modelValue", false);
}

const titleId = useId();
const footer = ref<HTMLElement | null>(null);

// Move focus into the dialog when it opens, so keyboard and screen-reader users
// land on its actions and Escape reaches the keydown handler.
watch(
  () => props.modelValue,
  async (open) => {
    if (!open) return;
    await nextTick();
    footer.value?.querySelector<HTMLButtonElement>("button")?.focus();
  },
  { immediate: true },
);

function handleProceed() {
  emit("proceed");
}
</script>
<template>
  <div
    v-if="modelValue"
    class="modal is-active"
    role="dialog"
    aria-modal="true"
    :aria-labelledby="titleId"
    @keydown.esc="close"
  >
    <div class="modal-background"></div>
    <div class="modal-card">
      <header class="modal-card-head">
        <h2 :id="titleId" class="modal-card-title">{{ title }}</h2>
      </header>

      <section class="modal-card-body">
        <div v-if="!isLoading" class="content">
          <p v-if="message" class="modal-message">{{ message }}</p>
          <ul v-if="details.length">
            <li v-for="detail in details" :key="detail.label">
              <strong>{{ detail.label }}:</strong> {{ detail.value }}
            </li>
          </ul>
        </div>

        <div v-else class="content">
          <LoadingSpinner :loading="true" />
        </div>
      </section>

      <footer ref="footer" class="modal-card-foot">
        <button v-if="!isProceed" class="button is-primary" @click="close">
          Close
        </button>
        <button v-if="isProceed" class="button is-primary" @click="close">
          No
        </button>
        <button
          v-if="isProceed"
          class="button is-primary"
          @click="handleProceed"
        >
          Yes
        </button>
      </footer>
    </div>
  </div>
</template>

<style scoped>
/* Callers pass plain text with "\n" where they would otherwise pass <br />. */
.modal-message {
  white-space: pre-line;
}

.modal-card-foot {
  justify-content: flex-end;
  gap: 0.5rem;
}
</style>
