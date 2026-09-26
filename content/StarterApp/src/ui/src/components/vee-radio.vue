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

const props = withDefaults(
  defineProps<
    BaseFieldProps & { modelValue?: OptionValue; options: FieldOption[] }
  >(),
  { modelValue: undefined, rules: "" },
);
defineEmits(["update:modelValue"]);

const ids = computed(() => fieldIds(props));

const { value, errorMessage } = useField<OptionValue | undefined>(
  () => ids.value.name,
  toRef(props, "rules"),
  { syncVModel: true, label: toRef(props, "label") },
);
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
          v-model="value"
          class="is-checkradio"
          type="radio"
          :name="ids.name"
          :value="optionValue(option)"
          :required="required"
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
