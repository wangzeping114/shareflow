<script setup lang="ts">
import { computed } from 'vue'
import { NProgress, NText } from 'naive-ui'

interface Props {
  filled: number
  total: number
  reserved?: number
}

const props = withDefaults(defineProps<Props>(), { reserved: 0 })

const occupiedPct = computed(() =>
  props.total === 0 ? 0 : Math.round((props.filled / props.total) * 100)
)

const statusType = computed(() => {
  if (occupiedPct.value >= 100) return 'error'
  if (occupiedPct.value >= 80) return 'warning'
  return 'success'
})
</script>

<template>
  <div class="slot-progress">
    <n-progress
      type="line"
      :percentage="occupiedPct"
      :status="statusType"
      :show-indicator="false"
      style="margin-bottom: 4px"
    />
    <div class="slot-labels">
      <n-text depth="3" style="font-size: 12px">
        已占 {{ filled }} / {{ total }} 份
      </n-text>
      <n-text v-if="reserved > 0" depth="3" style="font-size: 12px; margin-left: 8px; color: #f0a020">
        预留 {{ reserved }}
      </n-text>
      <n-text depth="3" style="font-size: 12px; margin-left: auto">
        {{ occupiedPct }}%
      </n-text>
    </div>
  </div>
</template>

<style scoped>
.slot-progress {
  min-width: 160px;
}
.slot-labels {
  display: flex;
  align-items: center;
}
</style>
