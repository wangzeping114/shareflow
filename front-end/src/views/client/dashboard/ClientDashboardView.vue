<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import {
  NGrid, NGi, NCard, NStatistic, NSpin, NTag,
  NDataTable, NFlex, NText, NEmpty,
  type DataTableColumns,
} from 'naive-ui'
import { getClientDashboard } from '../../../api/client/dashboard'
import type { ClientDashboardDto, ClientProjectSummaryDto, ClientRecentDividendDto } from '../../../types/client'

const router = useRouter()
const dashboard = ref<ClientDashboardDto | null>(null)
const loading = ref(false)

async function fetchDashboard() {
  loading.value = true
  try {
    dashboard.value = await getClientDashboard()
  } catch {
    // silent
  } finally {
    loading.value = false
  }
}
onMounted(fetchDashboard)

const cur = computed(() => dashboard.value?.currency ?? 'USD')

const projectColumns: DataTableColumns<ClientProjectSummaryDto> = [
  { title: '项目名称', key: 'projectTitle', ellipsis: { tooltip: true } },
  { title: '平台', key: 'platformName', width: 120 },
  {
    title: '持股比例',
    key: 'sharePermille',
    width: 110,
    render: row => `${(row.sharePermille / 10).toFixed(2)}%`,
  },
  {
    title: '累计分红',
    key: 'totalDividendReceived',
    width: 140,
    render: row => `${row.totalDividendReceived.toFixed(2)} ${row.currency}`,
  },
  { title: '合同状态', key: 'contractStatus', width: 100 },
]

const dividendColumns: DataTableColumns<ClientRecentDividendDto> = [
  { title: '项目', key: 'projectTitle', ellipsis: { tooltip: true } },
  {
    title: '分红金额',
    key: 'dividendAmount',
    width: 150,
    render: row => `${row.dividendAmount.toFixed(2)} ${row.currency}`,
  },
  { title: '状态', key: 'statusLabel', width: 90 },
  {
    title: '时间',
    key: 'calculatedAt',
    width: 160,
    render: row => new Date(row.calculatedAt).toLocaleDateString('zh-CN'),
  },
]
</script>

<template>
  <n-spin :show="loading">
    <div style="display: flex; flex-direction: column; gap: 24px;">

      <!-- KPI 卡片 -->
      <n-grid :x-gap="16" :y-gap="16" :cols="3">
        <n-gi>
          <n-card>
            <n-statistic label="可用余额">
              <template #default>
                <span style="color: #18a058; font-size: 28px; font-weight: 700">
                  {{ (dashboard?.walletBalance ?? 0).toFixed(2) }}
                </span>
                <span style="font-size: 14px; color: #666; margin-left: 4px">{{ cur }}</span>
              </template>
            </n-statistic>
            <div v-if="(dashboard?.frozenAmount ?? 0) > 0" style="margin-top: 8px; font-size: 12px; color: #f0a020">
              冻结: {{ dashboard!.frozenAmount.toFixed(2) }} {{ cur }}
            </div>
          </n-card>
        </n-gi>
        <n-gi>
          <n-card>
            <n-statistic label="累计已收分红">
              <template #default>
                <span style="color: #2080f0; font-size: 28px; font-weight: 700">
                  {{ (dashboard?.totalDividendReceived ?? 0).toFixed(2) }}
                </span>
                <span style="font-size: 14px; color: #666; margin-left: 4px">{{ cur }}</span>
              </template>
            </n-statistic>
          </n-card>
        </n-gi>
        <n-gi>
          <n-card>
            <n-statistic label="参投项目数">
              <template #default>
                <span style="font-size: 28px; font-weight: 700">
                  {{ dashboard?.projects.length ?? 0 }}
                </span>
                <span style="font-size: 14px; color: #666; margin-left: 4px">个</span>
              </template>
            </n-statistic>
          </n-card>
        </n-gi>
      </n-grid>

      <!-- 参投项目 -->
      <n-card title="我的持股项目">
        <n-data-table
          v-if="(dashboard?.projects.length ?? 0) > 0"
          :columns="projectColumns"
          :data="dashboard?.projects ?? []"
          size="small"
          striped
        />
        <n-empty v-else description="暂无持股项目" />
      </n-card>

      <!-- 最近分红 -->
      <n-card title="最近分红记录">
        <n-data-table
          v-if="(dashboard?.recentDividends.length ?? 0) > 0"
          :columns="dividendColumns"
          :data="dashboard?.recentDividends ?? []"
          size="small"
          striped
        />
        <n-empty v-else description="暂无分红记录" />
        <n-flex justify="end" style="margin-top: 12px;">
          <span
            style="font-size: 13px; color: #2080f0; cursor: pointer"
            @click="router.push('/client/dividends')"
          >查看全部 →</span>
        </n-flex>
      </n-card>

    </div>
  </n-spin>
</template>
