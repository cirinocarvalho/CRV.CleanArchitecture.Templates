<script setup lang="ts">
import { computed, toRef } from "vue";
import { useField } from "vee-validate";
import { type BaseFieldProps, describedBy, fieldIds } from "./fields";

// Attributes such as autocomplete, placeholder and inputmode belong on the
// <input>, not on the wrapping .field.
defineOptions({ inheritAttrs: false });

const props = withDefaults(
  defineProps<
    BaseFieldProps & {
      modelValue?: string | number;
      type?: string;
      // Font Awesome classes for an icon inside the control, e.g. "fas fa-phone".
      icon?: string;
    }
  >(),
  { modelValue: "", type: "text", rules: "" },
);
defineEmits(["update:modelValue"]);

const ids = computed(() => fieldIds(props));

// Rules go in as a ref, not a getter: vee-validate calls a function it is given
// as the validator itself, so a getter would return the rule string as the error.
// Validation runs when the field loses focus, then on every keystroke once an
// error is showing, so the message clears as soon as the input is fixed.
const { value, errorMessage, handleBlur, validate } = useField<
  string | number
>(() => ids.value.name, toRef(props, "rules"), {
  syncVModel: true,
  validateOnValueUpdate: false,
  // Messages name the field by its visible label, not its id.
  label: toRef(props, "label"),
});

function onInput() {
  if (errorMessage.value) validate();
}
</script>

<template>
  <div class="field">
    <label v-if="label" class="label" :for="ids.id">
      {{ label }}
    </label>
    <div class="control" :class="{ 'has-icons-left': icon }">
      <input
        :id="ids.id"
        v-model="value"
        class="input"
        :class="{ 'is-danger': errorMessage }"
        :name="ids.name"
        :type="type"
        :required="required"
        :disabled="disabled"
        :aria-invalid="errorMessage ? 'true' : undefined"
        :aria-describedby="describedBy(ids, desc, errorMessage)"
        v-bind="$attrs"
        @input="onInput"
        @blur="handleBlur($event, true)"
      />
      <span v-if="icon" class="icon is-small is-left" aria-hidden="true">
        <i :class="icon"></i>
      </span>
    </div>
    <p v-if="desc" :id="ids.descId" class="help">{{ desc }}</p>
    <p v-if="errorMessage" :id="ids.errorId" class="help is-danger">
      {{ errorMessage }}
    </p>
  </div>
</template>
