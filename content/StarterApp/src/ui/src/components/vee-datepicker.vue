<script setup lang="ts">
import VeeTextbox from "./vee-textbox.vue";
import { type BaseFieldProps } from "./fields";

// The browser's native date picker. The value is an ISO date string
// (yyyy-mm-dd) whatever format the browser displays, so it can go straight to
// the API. Pass min / max as attributes to bound the range.
defineProps<BaseFieldProps & { modelValue?: string }>();
const emit = defineEmits(["update:modelValue"]);
</script>

<template>
  <VeeTextbox
    v-bind="$props"
    type="date"
    @update:model-value="emit('update:modelValue', $event)"
  />
</template>
