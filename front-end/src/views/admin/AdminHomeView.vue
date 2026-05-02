<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { NGrid, NGi, NCard, NStatistic, NSpin, NFlex, NText } from 'naive-ui'
import VChart from 'vue-echarts'
import { use } from 'echarts/core'
import { CanvasRenderer } from 'echarts/renderers'
import { LineChart, BarChart, PieChart } from 'echarts/charts'
import {
  GridComponent, TooltipComponent, LegendComponent,
  TitleComponent,
} from 'echarts/components'
import { getAdminDashboard } from '../../api/admin/dashboard'
import type { AdminDashboardDto } from '../../api/admin/dashboard'

use([
  CanvasRenderer,
  LineChart, BarChart, PieChart,
  GridComponent, TooltipComponent, LegendComponent, TitleComponent,
])

const dashboard = ref<AdminDashboardDto | null>(null)
const loading = ref(false)

async function fetchDashboard() {
  loading.value = true
  try {
    dashboard.value = await getAdminDashboard()
  } catch {
    // silent
  } finally {
    loading.value = false
  }
}
onMounted(fetchDashboard)

const cur = computed(() => dashboard.value?.kpi.currency ?? 'USD')

// ── 折线图：月度收益 vs 分红趋势 ─────────────────────────────
const trendOption = computed(() => {
  const months = dashboard.value?.revenueByMonth ?? []
  return {
    tooltip: { trigger: 'axis', axisPointer: { type: 'cross' } },
    legend: { data: ['收益', '分红'], bottom: 0 },
    grid: { top: 16, bottom: 48, left: 56, right: 16 },
    xAxis: { type: 'category', data: months.map(m => m.month), boundaryGap: false },
    yAxis: { type: 'value', axisLabel: { formatter: (v: number) => v.toLocaleString() } },
    series: [
      {
        name: '收益',
        type: 'line',
        smooth: true,
        data: months.map(m => m.revenue),
        itemStyle: { color: '#3b82f6' },
        areaStyle: { color: 'rgba(59,130,246,0.08)' },
      },
      {
        name: '分红',
        type: 'line',
        smooth: true,
        data: months.map(m => m.dividend),
        itemStyle: { color: '#f59e0b' },
        areaStyle: { color: 'rgba(245,158,11,0.08)' },
      },
    ],
  }
})

// ── 饼图：收益按平台 ──────────────────────────────────────────
const platformOption = computed(() => ({
  tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
  legend: { orient: 'vertical', right: 8, top: 'center', type: 'scroll' },
  series: [{
    name: '各平台收益',
    type: 'pie',
    radius: ['40%', '68%'],
    center: ['38%', '50%'],
    data: dashboard.value?.revenueByPlatform ?? [],
    label: { show: true, formatter: '{b}\n{d}%' },
    emphasis: { itemStyle: { shadowBlur: 10, shadowColor: 'rgba(0,0,0,.2)' } },
  }],
}))

// ── 横向柱状图：各项目分红 ────────────────────────────────────
const dividendProjectOption = computed(() => {
  const list = [...(dashboard.value?.dividendByProject ?? [])].reverse()
  return {
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
    grid: { top: 8, bottom: 8, left: 100, right: 60, containLabel: false },
    xAxis: { type: 'value', axisLabel: { formatter: (v: number) => v.toLocaleString() } },
    yAxis: { type: 'category', data: list.map(x => x.name), axisLabel: { width: 90, overflow: 'truncate' } },
    series: [{
      name: '各项目分红',
      type: 'bar',
      barMaxWidth: 28,
      data: list.map(x => x.value),
      itemStyle: {
        color: (params: any) => {
          const colors = ['#18a058','#2080f0','#f0a020','#d03050','#7fe7c4','#a855f7','#f97316','#06b6d4']
          return colors[params.dataIndex % colors.length]
        },
        borderRadius: [0, 4, 4, 0],
      },
      label: {
        show: true, position: 'right',
        formatter: (p: any) => `${cur.value} ${Number(p.value).toLocaleString()}`,
      },
    }],
  }
})

// ── 环形图：合同状态分布 ──────────────────────────────────────
const contractStatusColors: Record<string, string> = {
  Draft: '#94a3b8', Sent: '#3b82f6', Signed: '#f59e0b',
  Executed: '#22c55e', Expired: '#ef4444', PendingRenew: '#a855f7',
  Renewing: '#6366f1', Superseded: '#9ca3af',
}

const contractOption = computed(() => ({
  tooltip: { trigger: 'item', formatter: '{b}: {c} ({d}%)' },
  legend: { orient: 'vertical', right: 8, top: 'center', type: 'scroll' },
  series: [{
    name: '合同状态',
    type: 'pie',
    radius: ['42%', '65%'],
    center: ['38%', '50%'],
    data: (dashboard.value?.contractStatusDist ?? []).map(x => ({
      name: x.name,
      value: x.value,
      itemStyle: { color: contractStatusColors[x.name] ?? '#94a3b8' },
    })),
    label: { show: true, formatter: '{b}\n{c}' },
    emphasis: { itemStyle: { shadowBlur: 10, shadowColor: 'rgba(0,0,0,.2)' } },
  }],
}))
</script>

<template>
  <n-spin :show="loading">
    <div style="display:flex;flex-direction:column;gap:20px;">

      <!-- KPI 卡片 -->
      <n-grid :x-gap="16" :y-gap="16" :cols="5">
        <n-gi>
          <n-card size="small">
            <n-statistic label="总收益">
              <template #default>
                <n-flex align="baseline" :gap="4">
                  <span style="font-size:22px;font-weight:700;color:#3b82f6">
                    {{ (dashboard?.kpi.totalRevenue ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
                  </span>
                  <n-text depth="3" style="font-size:12px">{{ cur }}</n-text>
                </n-flex>
              </template>
            </n-statistic>
          </n-card>
        </n-gi>
        <n-gi>
          <n-card size="small">
            <n-statistic label="已分红总额">
              <template #default>
                <n-flex align="baseline" :gap="4">
                  <span style="font-size:22px;font-weight:700;color:#f59e0b">
                    {{ (dashboard?.kpi.totalDividends ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}
                  </span>
                  <n-text depth="3" style="font-size:12px">{{ cur }}</n-text>
                </n-flex>
              </template>
            </n-statistic>
          </n-card>
        </n-gi>
        <n-gi>
          <n-card size="small">
            <n-statistic label="有效合同数">
              <template #default>
                <span style="font-size:22px;font-weight:700;color:#22c55e">
                  {{ dashboard?.kpi.activeContracts ?? 0 }}
                </span>
              </template>
            </n-statistic>
          </n-card>
        </n-gi>
        <n-gi>
          <n-card size="small">
            <n-statistic label="待处理提现">
              <template #default>
                <span :style="`font-size:22px;font-weight:700;color:${(dashboard?.kpi.pendingWithdrawals ?? 0) > 0 ? '#ef4444' : '#94a3b8'}`">
                  {{ dashboard?.kpi.pendingWithdrawals ?? 0 }}
                </span>
              </template>
            </n-statistic>
          </n-card>
        </n-gi>
        <n-gi>
          <n-card size="small">
            <n-statistic label="持股客户数">
              <template #default>
                <span style="font-size:22px;font-weight:700;color:#8b5cf6">
                  {{ dashboard?.kpi.totalClients ?? 0 }}
                </span>
              </template>
            </n-statistic>
          </n-card>
        </n-gi>
      </n-grid>

      <!-- 月度趋势折线图（全宽） -->
      <n-card title="近 6 个月收益与分红趋势">
        <v-chart :option="trendOption" style="height:260px" autoresize />
      </n-card>

      <!-- 平台饼图 + 合同状态环形图 -->
      <n-grid :x-gap="16" :cols="2">
        <n-gi>
          <n-card title="各平台收益分布">
            <v-chart :option="platformOption" style="height:280px" autoresize />
          </n-card>
        </n-gi>
        <n-gi>
          <n-card title="合同状态分布">
            <v-chart :option="contractOption" style="height:280px" autoresize />
          </n-card>
        </n-gi>
      </n-grid>

      <!-- 各项目分红横向柱状图 -->
      <n-card title="各项目累计分红 Top 10">
        <v-chart :option="dividendProjectOption" style="height:320px" autoresize />
      </n-card>

    </div>
  </n-spin>
</template>
