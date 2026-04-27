<script setup lang="ts">
import { onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRegion } from '../composables/use-region'
import BaseLayout from './BaseLayout.vue'

const { locale } = useI18n()
const { clientLocale } = useRegion()

// 客户端门户：境外显示英文，境内显示中文
onMounted(() => {
  locale.value = clientLocale
})
</script>

<template>
  <BaseLayout title-key="dashboard.client" />
</template>
