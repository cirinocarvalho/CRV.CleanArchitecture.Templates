<script setup lang="ts">
import VeeTextbox from "./vee-textbox.vue";
import { type BaseFieldProps } from "./fields";

// A text box with the telephone keyboard, autocomplete hint and icon. Format
// checks belong in `rules` — define a phone rule in main.ts for your locale.
defineProps<BaseFieldProps & { modelValue?: string }>();
const emit = defineEmits(["update:modelValue"]);
</script>

<template>
  <VeeTextbox
    v-bind="$props"
    type="tel"
    inputmode="tel"
    autocomplete="tel"
    icon="fas fa-phone"
    @update:model-value="emit('update:modelValue', $event)"
  />
</template>
