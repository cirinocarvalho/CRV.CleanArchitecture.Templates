<script setup lang="ts">
defineProps<{
  loading: boolean;
}>();
</script>
<template>
  <div v-if="loading" class="loader-container">
    <div class="spinner"></div>
  </div>
</template>

<style scoped>
.loader-container {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  height: 100px; /* or 100vh for fullscreen */
}

.spinner {
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  width: 32px;
  height: 32px;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
