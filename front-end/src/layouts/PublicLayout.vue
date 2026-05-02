<template>
  <n-config-provider>
    <n-message-provider>
      <div class="public-layout">
        <router-view />
      </div>
    </n-message-provider>
  </n-config-provider>
</template>

<script setup lang="ts">
import { NConfigProvider, NMessageProvider } from 'naive-ui'
</script>

<style scoped>
.public-layout {
  min-height: 100vh;
}
</style>
