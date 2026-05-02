<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { NGrid, NGi, NStatistic, NCard, NDataTable, NTag, NSpin, type DataTableColumns } from 'naive-ui'
import { getSalesPerformance, getLeads } from '../../../api/sales'
import type { SalesPerformanceDto, LeadDto } from '../../../types/sales'
import { LeadStatus } from '../../../types/sales'

const performance = ref<SalesPerformanceDto | null>(null)
const recentLeads = ref<LeadDto[]>([])
const loading = ref(false)

const statusTagType: Record<LeadStatus, 'default' | 'info' | 'success' | 'warning' | 'error'> = {
  [LeadStatus.New]: 'info',
  [LeadStatus.Contacted]: 'default',
  [LeadStatus.Interested]: 'warning',
  [LeadStatus.Converted]: 'success',
  [LeadStatus.Lost]: 'error',
}

const statusLabel: Record<LeadStatus, string> = {
  [LeadStatus.New]: '新线索',
  [LeadStatus.Contacted]: '已联系',
  [LeadStatus.Interested]: '感兴趣',
  [LeadStatus.Converted]: '已转化',
  [LeadStatus.Lost]: '已流失',
}

const leadColumns: DataTableColumns<LeadDto> = [
  { title: '姓名', key: 'name', width: 120 },
  { title: '联系方式', key: 'contactInfo', width: 160 },
  { title: '邮箱', key: 'email', width: 180, render: (row) => row.email ?? '-' },
  {
    title: '状态', key: 'status', width: 100,
    render: (row) => h(NTag, { type: statusTagType[row.status], size: 'small' }, { default: () => statusLabel[row.status] }),
  },
  { title: '创建时间', key: 'createdAt', width: 160, render: (row) => new Date(row.createdAt).toLocaleDateString('zh-CN') },
]

import { h } from 'vue'

async function load() {
  loading.value = true
  try {
    const [perf, leadsResult] = await Promise.all([
      getSalesPerformance(),
      getLeads({ page: 1, pageSize: 5 }),
    ])
    performance.value = perf
    recentLeads.value = leadsResult.items
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div>
    <h2 style="margin: 0 0 20px; font-size: 20px; font-weight: 600">销售工作台</h2>

    <n-spin :show="loading">
      <!-- 业绩指标 -->
      <n-grid :cols="5" :x-gap="16" style="margin-bottom: 24px">
        <n-gi>
          <n-card>
            <n-statistic label="总线索" :value="performance?.totalLeads ?? 0" />
          </n-card>
        </n-gi>
        <n-gi>
          <n-card>
            <n-statistic label="已转化" :value="performance?.convertedLeads ?? 0" />
          </n-card>
        </n-gi>
        <n-gi>
          <n-card>
            <n-statistic label="转化率" :value="`${performance?.conversionRate ?? 0}%`" />
          </n-card>
        </n-gi>
        <n-gi>
          <n-card>
            <n-statistic label="已发合同" :value="performance?.contractsSent ?? 0" />
          </n-card>
        </n-gi>
        <n-gi>
          <n-card>
            <n-statistic label="已签合同" :value="performance?.contractsSigned ?? 0" />
          </n-card>
        </n-gi>
      </n-grid>

      <!-- 最近客户 -->
      <n-card title="最近客户">
        <n-data-table :columns="leadColumns" :data="recentLeads" :bordered="false" size="small" />
      </n-card>
    </n-spin>
  </div>
</template>
