/* eslint-disable vue/multi-word-component-names */
// ─────────────────────────────────────────────
// Imports
// ─────────────────────────────────────────────
import { createApp } from "vue";
import { createI18n } from "vue-i18n";
import { createPinia } from "pinia";
import router from "./router";
import App from "./App.vue";
import "./assets/scss/styles.scss";
import "./assets/scss/main.scss";
import { Form as VeeForm, Field, defineRule } from "vee-validate";
import { email } from "@vee-validate/rules";
import VeeTextbox from "./components/vee-textbox.vue";
import VeeTextarea from "./components/vee-textarea.vue";
import VeeCheckbox from "./components/vee-checkbox.vue";
import VeeRadio from "./components/vee-radio.vue";
import VeePhone from "./components/vee-phone.vue";
import VeeDropdown from "./components/vee-dropdown.vue";
import VeeDatePicker from "./components/vee-datepicker.vue";
import VueGtag from "vue-gtag";

// ─────────────────────────────────────────────
// i18n Setup
// ─────────────────────────────────────────────
const i18n = createI18n({
  locale: "en-US",
  legacy: false,
  messages: {
    "en-US": {
      message: {
        hello: "hello world!",
      },
    },
    es: {
      message: {
        hello: "Hola parceros!",
      },
    },
  },
});

// ─────────────────────────────────────────────
// VeeValidate Rules
// ─────────────────────────────────────────────
defineRule("required", (value: string | FileList) => {
  if (value instanceof FileList) {
    return value.length > 0 || "This field is required";
  }
  if (value === undefined || value === null || value === "") {
    return "This field is required";
  }
  return true;
});
defineRule("email", email);
defineRule("minMaxLength", (value: string, [min, max]: [number, number]) => {
  if (!value) {
    return true;
  }
  if (value.length < min) {
    return `This field must be at least ${min} characters long`;
  }
  if (value.length > max) {
    return `This field must be at most ${max} characters long`;
  }
  return true;
});
defineRule("uploadTypes", (value: FileList | null, params: string[]) => {
  if (!value || value.length === 0) return true;
  if (params.includes("*/*")) return true;
  const files = Array.from(value);
  const isValid = files.every((file) => params.includes(file.type));
  return (
    isValid || `Files must be one of the following types: ${params.join(", ")}.`
  );
});
defineRule("uploadMaxFiles", (value: FileList | null, [maxFiles]: [string]) => {
  if (!value) return true;
  const isValid = value.length <= parseInt(maxFiles);
  return isValid || `You can upload up to ${maxFiles} files.`;
});
defineRule("uploadFileSize", (value: FileList | null, [size]: [string]) => {
  if (!value || value.length === 0) return true;
  const maxBytes = parseInt(size) * 1024 * 1024;
  const isValid = Array.from(value).every((file) => file.size <= maxBytes);
  return isValid || `Files must not exceed ${size}MB.`;
});

// ─────────────────────────────────────────────
// App Creation & Component Registration
// ─────────────────────────────────────────────
const app = createApp(App);
const pinia = createPinia();
app.use(pinia);

app.component("VeeForm", VeeForm);
app.component("Field", Field);
app.component("VeeTextbox", VeeTextbox);
app.component("VeeTextarea", VeeTextarea);
app.component("VeeCheckbox", VeeCheckbox);
app.component("VeeRadio", VeeRadio);
app.component("VeePhone", VeePhone);
app.component("VeeDropdown", VeeDropdown);
app.component("VeeDatePicker", VeeDatePicker);

// ─────────────────────────────────────────────
// Plugin Usage & Mount
// ─────────────────────────────────────────────
app.use(router);
app.use(i18n);

// ─────────────────────────────────────────────
// Google Analytics (vue-gtag)
// Only initialize when a GA4 measurement ID is configured, so dev/local
// builds without an ID don't load gtag. Passing `router` enables automatic
// page-view tracking on route changes.
// ─────────────────────────────────────────────
const gaMeasurementId = import.meta.env.VITE_APP_GA_MEASUREMENT_ID;
if (gaMeasurementId) {
  app.use(
    VueGtag,
    {
      config: { id: gaMeasurementId },
    },
    router,
  );
}

app.mount("#app");
