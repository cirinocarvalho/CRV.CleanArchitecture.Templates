<script setup lang="ts">
import { computed, ref } from "vue";

interface Props {
  fileType?: string;
  maxFile?: number;
  fileSize?: number;
  required?: boolean;
  labelText?: string;
}

const props = withDefaults(defineProps<Props>(), {
  fileType: "*/*",
  maxFile: 0,
  fileSize: 10,
  required: false,
  labelText: "",
});

const emit = defineEmits<{ "data-files": [files: FileList] }>();

const fileUploadRef = ref<HTMLInputElement | null>(null);
const fieldRef = ref<{ reset: () => void } | null>(null);

const rules = computed(() => {
  const base = `uploadTypes:${props.fileType}|uploadMaxFiles:${props.maxFile}|uploadFileSize:${props.fileSize}`;
  return props.required ? `required|${base}` : base;
});

function onFileChange(
  event: Event,
  handleChange: (e: Event | unknown) => void,
) {
  const target = event.target as HTMLInputElement;
  const files = target.files;
  if (files) {
    handleChange(files);
    emit("data-files", files);
  }
}

function getFiles(): FileList | null {
  return fileUploadRef.value?.files ?? null;
}

// Clears the selected file: resets the native input (so the filename disappears)
// and the vee-validate field state (value + validation).
function reset() {
  if (fileUploadRef.value) fileUploadRef.value.value = "";
  fieldRef.value?.reset();
}

defineExpose({ getFiles, reset });
</script>
<template>
  <Field
    ref="fieldRef"
    name="FileUpload"
    :rules="rules"
    v-slot="{ errorMessage, handleChange }"
  >
    <div class="field">
      <label for="FileUpload" class="label">{{ labelText }}</label>
      <div class="control">
        <input
          ref="fileUploadRef"
          type="file"
          :accept="fileType"
          id="FileUpload"
          name="FileUpload"
          multiple
          class="input"
          :class="{ 'is-danger': errorMessage }"
          :aria-invalid="errorMessage ? 'true' : undefined"
          :aria-describedby="errorMessage ? 'FileUpload-error' : undefined"
          @change="(e) => onFileChange(e, handleChange)"
        />
      </div>
      <p v-if="errorMessage" id="FileUpload-error" class="help is-danger">
        <span class="icon is-small" aria-hidden="true"
          ><i class="fas fa-exclamation-circle"></i
        ></span>
        <span>{{ errorMessage }}</span>
      </p>
    </div>
  </Field>
</template>
<style scoped>
/* A file input is taller than a text input; let Bulma's .input grow to fit. */
input[type="file"].input {
  height: auto;
  padding-block: 0.5rem;
}
</style>
