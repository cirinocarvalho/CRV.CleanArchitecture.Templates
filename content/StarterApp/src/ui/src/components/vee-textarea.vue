<script setup lang="ts">
import { computed, toRef } from "vue";
import { useField } from "vee-validate";
import { type BaseFieldProps, describedBy, fieldIds } from "./fields";

defineOptions({ inheritAttrs: false });

const props = withDefaults(
  defineProps<BaseFieldProps & { modelValue?: string; rows?: number }>(),
  { modelValue: "", rules: "", rows: 4 },
);
defineEmits(["update:modelValue"]);

const ids = computed(() => fieldIds(props));

const { value, errorMessage, handleBlur, validate } = useField<string>(
  () => ids.value.name,
  toRef(props, "rules"),
  {
    syncVModel: true,
    validateOnValueUpdate: false,
    label: toRef(props, "label"),
  },
);

function onInput() {
  if (errorMessage.value) validate();
}
</script>

<template>
  <div class="field">
    <label v-if="label" class="label" :for="ids.id">
      {{ label }}
    </label>
    <div class="control">
      <textarea
        :id="ids.id"
        v-model="value"
        class="textarea"
        :class="{ 'is-danger': errorMessage }"
        :name="ids.name"
        :rows="rows"
        :required="required"
        :disabled="disabled"
        :aria-invalid="errorMessage ? 'true' : undefined"
        :aria-describedby="describedBy(ids, desc, errorMessage)"
        v-bind="$attrs"
        @input="onInput"
        @blur="handleBlur($event, true)"
      ></textarea>
    </div>
    <p v-if="desc" :id="ids.descId" class="help">{{ desc }}</p>
    <p v-if="errorMessage" :id="ids.errorId" class="help is-danger">
      {{ errorMessage }}
    </p>
  </div>
</template>
