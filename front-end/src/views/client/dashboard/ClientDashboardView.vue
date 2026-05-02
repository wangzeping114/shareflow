<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import {
  NGrid, NGi, NCard, NStatistic, NSpin,
  NDataTable, NFlex, NEmpty,
  type DataTableColumns,
} from 'naive-ui'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { BarChart, PieChart } from 'echarts/charts'
import { GridComponent, TooltipComponent, LegendComponent } from 'echarts/components'
import { getClientDashboard } from '../../../api/client/dashboard'
import { useRegion } from '../../../composables/use-region'
import type { ClientDashboardDto, ClientProjectSummaryDto, ClientRecentDividendDto } from '../../../types/client'

use([CanvasRenderer, BarChart, PieChart, GridComponent, TooltipComponent, LegendComponent])

const { t } = useI18n()
const { clientLocale } = useRegion()
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

function fmtDate(iso: string) {
  return new Date(iso).toLocaleDateString(clientLocale, { year: 'numeric', month: 'short', day: 'numeric' })
}

const projectColumns = computed<DataTableColumns<ClientProjectSummaryDto>>(() => [
  { title: t('client.col.project'), key: 'projectTitle', ellipsis: { tooltip: true } },
  { title: t('client.col.platform'), key: 'platformName', width: 160 },
  {
    title: t('client.col.equityPct'),
    key: 'sharePermille',
    width: 110,
    render: row => `${(row.sharePermille / 10).toFixed(2)}%`,
  },
  {
    title: t('client.col.dividendsEarned'),
    key: 'totalDividendReceived',
    width: 160,
    render: row => `${row.totalDividendReceived.toFixed(2)} ${row.currency}`,
  },
  { title: t('client.col.contractStatus'), key: 'contractStatus', width: 130 },
])

const dividendColumns = computed<DataTableColumns<ClientRecentDividendDto>>(() => [
  { title: t('client.col.project'), key: 'projectTitle', ellipsis: { tooltip: true } },
  {
    title: t('client.col.dividendAmount'),
    key: 'dividendAmount',
    width: 160,
    render: row => `${row.dividendAmount.toFixed(2)} ${row.currency}`,
  },
  { title: t('client.col.status'), key: 'statusLabel', width: 100 },
  {
    title: t('client.col.date'),
    key: 'calculatedAt',
    width: 160,
    render: row => fmtDate(row.calculatedAt),
  },
])

// ── 饼图：持股比例分布 ────────────────────────────────────────
const portfolioOption = computed(() => ({
  tooltip: { trigger: 'item', formatter: '{b}: {d}%' },
  legend: { orient: 'vertical', right: 8, top: 'center', type: 'scroll' },
  series: [{
    name: 'Portfolio',
    type: 'pie',
    radius: ['38%', '65%'],
    center: ['38%', '50%'],
    data: (dashboard.value?.projects ?? []).map(p => ({
      name: p.projectTitle,
      value: +(p.sharePermille / 10).toFixed(2),
    })),
    label: { show: true, formatter: '{b}\n{d}%' },
    emphasis: { itemStyle: { shadowBlur: 10, shadowColor: 'rgba(0,0,0,.15)' } },
  }],
}))

// ── 柱状图：各项目累计分红 ────────────────────────────────────
const dividendChartOption = computed(() => {
  const projects = dashboard.value?.projects ?? []
  const earning_colors = ['#18a058','#2080f0','#f59e0b','#d03050','#8b5cf6','#06b6d4']
  return {
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
    grid: { top: 8, bottom: 32, left: 8, right: 8, containLabel: true },
    xAxis: { type: 'category', data: projects.map(p => p.projectTitle), axisLabel: { overflow: 'truncate', width: 80 } },
    yAxis: { type: 'value', axisLabel: { formatter: (v: number) => v.toLocaleString() } },
    series: [{
      type: 'bar',
      barMaxWidth: 40,
      data: projects.map((p, i) => ({
        value: p.totalDividendReceived,
        itemStyle: { color: earning_colors[i % earning_colors.length], borderRadius: [4, 4, 0, 0] },
      })),
      label: { show: true, position: 'top', formatter: (p: any) => p.value > 0 ? p.value.toFixed(2) : '' },
    }],
  }
})
</script>

<template>
  <n-spin :show="loading">
    <div style="display: flex; flex-direction: column; gap: 24px;">

      <!-- KPI 卡片 -->
      <n-grid :x-gap="16" :y-gap="16" :cols="3">
        <n-gi>
          <n-card>
            <n-statistic :label="t('client.availableBalance')">
              <template #default>
                <span style="color: #18a058; font-size: 28px; font-weight: 700">
                  {{ (dashboard?.walletBalance ?? 0).toFixed(2) }}
                </span>
                <span style="font-size: 14px; color: #666; margin-left: 4px">{{ cur }}</span>
              </template>
            </n-statistic>
            <div v-if="(dashboard?.frozenAmount ?? 0) > 0" style="margin-top: 8px; font-size: 12px; color: #f0a020">
              Frozen: {{ dashboard!.frozenAmount.toFixed(2) }} {{ cur }}
            </div>
          </n-card>
        </n-gi>
        <n-gi>
          <n-card>
            <n-statistic :label="t('client.totalDividends')">
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
            <n-statistic :label="t('client.projectCount')">
              <template #default>
                <span style="font-size: 28px; font-weight: 700">
                  {{ dashboard?.projects.length ?? 0 }}
                </span>
                <span v-if="t('client.unitProjects')" style="font-size: 14px; color: #666; margin-left: 4px">
                  {{ t('client.unitProjects') }}
                </span>
              </template>
            </n-statistic>
          </n-card>
        </n-gi>
      </n-grid>

      <!-- 图表行：持股占比 + 各项目分红 -->
      <n-grid v-if="(dashboard?.projects.length ?? 0) > 0" :x-gap="16" :cols="2">
        <n-gi>
          <n-card title="Portfolio Breakdown">
            <v-chart :option="portfolioOption" style="height:240px" autoresize />
          </n-card>
        </n-gi>
        <n-gi>
          <n-card title="Dividends Earned by Project">
            <v-chart :option="dividendChartOption" style="height:240px" autoresize />
          </n-card>
        </n-gi>
      </n-grid>

      <!-- 参投项目 -->
      <n-card :title="t('client.myPortfolio')">
        <n-data-table
          v-if="(dashboard?.projects.length ?? 0) > 0"
          :columns="projectColumns"
          :data="dashboard?.projects ?? []"
          size="small"
          striped
        />
        <n-empty v-else :description="t('client.noProjects')" />
      </n-card>

      <!-- 最近分红 -->
      <n-card :title="t('client.recentDividends')">
        <n-data-table
          v-if="(dashboard?.recentDividends.length ?? 0) > 0"
          :columns="dividendColumns"
          :data="dashboard?.recentDividends ?? []"
          size="small"
          striped
        />
        <n-empty v-else :description="t('client.noDividends')" />
        <n-flex justify="end" style="margin-top: 12px;">
          <span
            style="font-size: 13px; color: #2080f0; cursor: pointer"
            @click="router.push('/client/dividends')"
          >{{ t('client.viewAll') }}</span>
        </n-flex>
      </n-card>

    </div>
  </n-spin>
</template>
