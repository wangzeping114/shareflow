<script setup lang="ts">
import { computed } from 'vue'
import { NCard, NGrid, NGi, NStatistic, NSpin } from 'naive-ui'
import type { DividendProjectSummaryDto } from '../../../types/dividend'

interface Props {
  summary: DividendProjectSummaryDto | null | undefined
  loading?: boolean
}

const props = withDefaults(defineProps<Props>(), { loading: false })

const currency = computed(() => props.summary?.currency ?? '')
</script>

<template>
  <NSpin :show="loading">
    <NCard title="项目分红汇总" size="small">
      <NGrid :cols="3" :x-gap="16">
        <NGi>
          <NStatistic label="已计算（待确认）">
            <template #default>
              <span style="font-size: 20px; font-weight: 600;">
                {{ summary ? summary.totalCalculated.toFixed(2) : '-' }}
              </span>
              <span v-if="currency" style="font-size: 13px; color: #999; margin-left: 4px;">{{ currency }}</span>
            </template>
          </NStatistic>
        </NGi>
        <NGi>
          <NStatistic label="已确认（待分发）">
            <template #default>
              <span style="font-size: 20px; font-weight: 600; color: #2080f0;">
                {{ summary ? summary.totalConfirmed.toFixed(2) : '-' }}
              </span>
              <span v-if="currency" style="font-size: 13px; color: #999; margin-left: 4px;">{{ currency }}</span>
            </template>
          </NStatistic>
        </NGi>
        <NGi>
          <NStatistic label="已分发（到账）">
            <template #default>
              <span style="font-size: 20px; font-weight: 600; color: #18a058;">
                {{ summary ? summary.totalDistributed.toFixed(2) : '-' }}
              </span>
              <span v-if="currency" style="font-size: 13px; color: #999; margin-left: 4px;">{{ currency }}</span>
            </template>
          </NStatistic>
        </NGi>
      </NGrid>
    </NCard>
  </NSpin>
</template>
