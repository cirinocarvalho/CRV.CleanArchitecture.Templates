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

defineOptions({ inheritAttrs: false });

const props = withDefaults(
  defineProps<
    BaseFieldProps & {
      modelValue?: OptionValue;
      options: FieldOption[];
      // Text of the empty first option; it holds the "nothing chosen" state
      // that the `required` rule checks for.
      placeholder?: string;
    }
  >(),
  { modelValue: undefined, rules: "", placeholder: "Select…" },
);
defineEmits(["update:modelValue"]);

const ids = computed(() => fieldIds(props));

const { value, errorMessage, handleBlur } = useField<OptionValue | undefined>(
  () => ids.value.name,
  toRef(props, "rules"),
  { syncVModel: true, label: toRef(props, "label") },
);
</script>

<template>
  <div class="field">
    <label v-if="label" class="label" :for="ids.id">
      {{ label }}
    </label>
    <div class="control">
      <div class="select is-fullwidth" :class="{ 'is-danger': errorMessage }">
        <select
          :id="ids.id"
          v-model="value"
          :name="ids.name"
          :required="required"
          :disabled="disabled"
          :aria-invalid="errorMessage ? 'true' : undefined"
          :aria-describedby="describedBy(ids, desc, errorMessage)"
          v-bind="$attrs"
          @blur="handleBlur($event, true)"
        >
          <option :value="undefined">{{ placeholder }}</option>
          <option
            v-for="option in options"
            :key="String(optionValue(option))"
            :value="optionValue(option)"
            :disabled="optionDisabled(option)"
          >
            {{ optionText(option) }}
          </option>
        </select>
      </div>
    </div>
    <p v-if="desc" :id="ids.descId" class="help">{{ desc }}</p>
    <p v-if="errorMessage" :id="ids.errorId" class="help is-danger">
      {{ errorMessage }}
    </p>
  </div>
</template>
