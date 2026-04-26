<script setup lang="ts">
import { NCard, NH2, NSpace, NText } from 'naive-ui'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
</script>

<template>
  <n-card>
    <n-space vertical>
      <n-h2>{{ t('dashboard.client') }}</n-h2>
      <n-text depth="3">{{ t('dashboard.clientDesc') }}</n-text>
    </n-space>
  </n-card>
</template>
