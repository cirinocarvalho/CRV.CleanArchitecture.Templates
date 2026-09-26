<script setup lang="ts">
import { computed, toRef } from "vue";
import { useField } from "vee-validate";
import {
  type BaseFieldProps,
  type FieldOption,
  type OptionValue,
  describedBy,
  fieldIds,
  optionDisabled,
  optionText,
  optionValue,
} from "./fields";

// A group of checkboxes whose value is the array of checked option values.
// For a single yes/no box, pass one option.
const props = withDefaults(
  defineProps<
    BaseFieldProps & { modelValue?: OptionValue[]; options: FieldOption[] }
  >(),
  { modelValue: undefined, rules: "" },
);
defineEmits(["update:modelValue"]);

const ids = computed(() => fieldIds(props));

const { value, errorMessage } = useField<OptionValue[] | undefined>(
  () => ids.value.name,
  toRef(props, "rules"),
  { syncVModel: true, label: toRef(props, "label") },
);

// The `required` rule treats an empty array as a value, so nothing checked is
// stored as undefined for it to fail on.
const checked = computed<OptionValue[]>({
  get: () => value.value ?? [],
  set: (next) => {
    value.value = next.length ? next : undefined;
  },
});
</script>

<template>
  <fieldset
    class="field"
    :aria-describedby="describedBy(ids, desc, errorMessage)"
  >
    <legend v-if="label" class="label">{{ label }}</legend>
    <div class="control">
      <div
        v-for="(option, index) in options"
        :key="String(optionValue(option))"
        class="choice"
      >
        <input
          :id="`${ids.id}-${index}`"
          v-model="checked"
          class="is-checkradio"
          type="checkbox"
          :name="ids.name"
          :value="optionValue(option)"
          :disabled="disabled || optionDisabled(option)"
        />
        <label :for="`${ids.id}-${index}`">{{ optionText(option) }}</label>
      </div>
    </div>
    <p v-if="desc" :id="ids.descId" class="help">{{ desc }}</p>
    <p v-if="errorMessage" :id="ids.errorId" class="help is-danger">
      {{ errorMessage }}
    </p>
  </fieldset>
</template>
