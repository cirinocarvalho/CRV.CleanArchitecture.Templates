<script setup lang="ts">
import UsersPanel from "./admin/UsersPanel.vue";
</script>

<template>
  <div class="container content admin-page">
    <div class="columns is-multiline">
      <div class="column is-12">
        <div class="section-panel">
          <div class="panel admin-panel">
            <p class="panel-heading">Admin</p>
            <div class="panel-block">
              <div class="admin-content">
                <UsersPanel />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-page {
  padding: 5px;
}

/* Section panel */
.section-panel {
  border: 1px solid #dbdbdb;
  padding: 10px;
  background: #fff;
}

/* Blue title bar + shadow card, matching the other pages' panels */
.panel-heading {
  background-color: #2176d2;
  color: #fff;
  font-weight: 600;
}
.admin-panel .panel-block {
  display: block;
}
.admin-content {
  width: 100%;
  padding: 8px 4px;
}
</style>
