<script setup lang="ts">
// ─────────────────────────────────────────────
// Attachments
// ─────────────────────────────────────────────
// Sample screen for the file upload slice: uploads a file against a reference
// and lists what has already been stored. Mirrors the FileUpload endpoints in
// the API. Delete this along with the rest of the slice if you don't need it.
import { onMounted, ref } from "vue";
import api from "../services/api";
import UploadInput from "../components/UploadInput.vue";
import { getErrorMessage } from "../utils/errorMessage";

interface Attachment {
  id: number;
  fileName: string;
  originalFileName: string;
  reference: string;
  sizeBytes: number;
  createdDate: string;
}

const uploadRef = ref<{
  getFiles: () => FileList | null;
  reset: () => void;
} | null>(null);

const reference = ref("");
const attachments = ref<Attachment[]>([]);
const isBusy = ref(false);
const errorText = ref("");
const successText = ref("");

async function load() {
  try {
    const { data } = await api.get<Attachment[]>("/api/v1/fileupload");
    attachments.value = data;
  } catch (error) {
    errorText.value = getErrorMessage(error, "Couldn't load the attachments.");
  }
}

async function upload() {
  errorText.value = "";
  successText.value = "";

  const files = uploadRef.value?.getFiles();
  if (!files || files.length === 0) {
    errorText.value = "Choose a file first.";
    return;
  }

  isBusy.value = true;
  try {
    // One request per selected file — the API takes a single file per call.
    for (const file of Array.from(files)) {
      const form = new FormData();
      form.append("file", file);
      form.append("reference", reference.value);
      await api.post("/api/v1/fileupload", form);
    }

    successText.value =
      files.length === 1 ? "File uploaded." : `${files.length} files uploaded.`;
    uploadRef.value?.reset();
    await load();
  } catch (error) {
    errorText.value = getErrorMessage(error, "The upload failed.");
  } finally {
    isBusy.value = false;
  }
}

async function remove(id: number) {
  errorText.value = "";
  successText.value = "";
  try {
    await api.delete(`/api/v1/fileupload/${id}`);
    await load();
  } catch (error) {
    errorText.value = getErrorMessage(error, "Couldn't delete the attachment.");
  }
}

function formatSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

onMounted(load);
</script>

<template>
  <div class="container attachments">
    <h1 class="title is-3 mt-5">Attachments</h1>

    <div class="notification is-danger is-light" v-if="errorText">
      {{ errorText }}
    </div>
    <div class="notification is-success is-light" v-if="successText">
      {{ successText }}
    </div>

    <VeeForm @submit="upload">
      <VeeTextbox
        v-model="reference"
        name="reference"
        label="Reference"
        rules="required"
      />
      <UploadInput ref="uploadRef" label-text="File" :file-size="50" required />
      <button class="button is-primary mt-4" type="submit" :disabled="isBusy">
        {{ isBusy ? "Uploading…" : "Upload" }}
      </button>
    </VeeForm>

    <table class="table is-fullwidth mt-6" v-if="attachments.length">
      <thead>
        <tr>
          <th>File</th>
          <th>Reference</th>
          <th>Size</th>
          <th>Uploaded</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in attachments" :key="item.id">
          <td>{{ item.originalFileName }}</td>
          <td>{{ item.reference }}</td>
          <td>{{ formatSize(item.sizeBytes) }}</td>
          <td>{{ new Date(item.createdDate).toLocaleString() }}</td>
          <td>
            <button class="button is-small is-danger" @click="remove(item.id)">
              Delete
            </button>
          </td>
        </tr>
      </tbody>
    </table>
    <p class="mt-6" v-else>Nothing uploaded yet.</p>
  </div>
</template>

<style lang="scss" scoped>
.attachments {
  max-width: 60rem;
}
</style>
