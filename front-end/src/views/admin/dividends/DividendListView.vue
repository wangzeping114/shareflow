<script setup lang="ts">
import { h, ref, onMounted } from 'vue'
import {
  NButton, NDataTable, NFlex, NSelect, NTag, NPagination, useMessage,
  type DataTableColumns, type DataTableRowKey,
} from 'naive-ui'
import {
  getDividendList,
  getDividendProjectSummary,
  confirmDividends,
  distributeDividends,
} from '../../../api/admin/dividend'
import type { DividendDto, DividendStatus } from '../../../types/dividend'
import { useProjectSelect } from '../../../composables/use-project-select'
import DividendSummaryCard from './DividendSummaryCard.vue'

const message = useMessage()

// ── 筛选状态 ────────────────────────────────────────────────
const { projectOptions } = useProjectSelect()
const filterProjectIds = ref<string[]>([])
const filterStatuses = ref<DividendStatus[]>([])

// ── 分页与列表 ───────────────────────────────────────────────
const list = ref<DividendDto[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const loading = ref(false)

// ── 操作状态 ─────────────────────────────────────────────────
const checkedRowKeys = ref<DataTableRowKey[]>([])
const batchLoading = ref(false)
const summaryLoading = ref(false)
const summary = ref<Awaited<ReturnType<typeof getDividendProjectSummary>> | null>(null)

// ── 数据加载 ─────────────────────────────────────────────────
async function fetchList() {
  loading.value = true
  try {
    const result = await getDividendList({
      projectIds: filterProjectIds.value.length > 0 ? filterProjectIds.value : undefined,
      statuses: filterStatuses.value.length > 0 ? filterStatuses.value : undefined,
      page: page.value,
      pageSize: pageSize.value,
    })
    list.value = result?.items ?? []
    total.value = result?.total ?? 0
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? '加载失败')
  } finally {
    loading.value = false
  }
}

async function loadSummary() {
  if (filterProjectIds.value.length !== 1) { summary.value = null; return }
  summaryLoading.value = true
  try {
    summary.value = await getDividendProjectSummary(filterProjectIds.value[0])
  } finally {
    summaryLoading.value = false
  }
}

function onFilterChange() {
  page.value = 1
  fetchList()
  loadSummary()
}

onMounted(() => fetchList())

// ── 状态选项（空 = 全部）────────────────────────────────────
const statusOptions = [
  { label: '已计算', value: 'Calculated' as DividendStatus },
  { label: '已确认', value: 'Confirmed' as DividendStatus },
  { label: '已分发', value: 'Distributed' as DividendStatus },
]

function statusTag(status: DividendStatus) {
  const map: Record<DividendStatus, { type: 'default' | 'info' | 'success' | 'warning'; label: string }> = {
    Calculated: { type: 'warning', label: '已计算' },
    Confirmed: { type: 'info', label: '已确认' },
    Distributed: { type: 'success', label: '已分发' },
  }
  const m = map[status]
  return h(NTag, { type: m.type, size: 'small' }, () => m.label)
}

// ── 表格列 ──────────────────────────────────────────────────
const columns: DataTableColumns<DividendDto> = [
  { type: 'selection' },
  { title: '项目', key: 'projectTitle', width: 180, ellipsis: { tooltip: true } },
  { title: '投资人', key: 'investorName', width: 130 },
  {
    title: '收益额',
    key: 'revenueAmount',
    width: 130,
    render: (row) => `${row.revenueAmount.toFixed(2)} ${row.currency}`,
  },
  {
    title: '持股千分比',
    key: 'sharePermille',
    width: 110,
    render: (row) => `${row.sharePermille}‰`,
  },
  {
    title: '分红金额',
    key: 'dividendAmount',
    width: 140,
    render: (row) =>
      h('span', { style: 'font-weight:600;color:#18a058' },
        `${row.dividendAmount.toFixed(2)} ${row.currency}`),
  },
  {
    title: '状态',
    key: 'status',
    width: 100,
    render: (row) => statusTag(row.status as DividendStatus),
  },
  {
    title: '计算时间',
    key: 'calculatedAt',
    width: 160,
    render: (row) => new Date(row.calculatedAt).toLocaleString('zh-CN'),
  },
]

// ── 批量操作 ─────────────────────────────────────────────────
async function handleConfirm() {
  const ids = checkedRowKeys.value as string[]
  if (ids.length === 0) { message.warning('请先选择要确认的记录'); return }
  batchLoading.value = true
  try {
    await confirmDividends(ids)
    message.success(`已确认 ${ids.length} 条分红记录`)
    checkedRowKeys.value = []
    fetchList()
    loadSummary()
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? '操作失败')
  } finally {
    batchLoading.value = false
  }
}

async function handleDistribute() {
  const ids = checkedRowKeys.value as string[]
  if (ids.length === 0) { message.warning('请先选择要分发的记录'); return }
  batchLoading.value = true
  try {
    await distributeDividends(ids)
    message.success(`已分发 ${ids.length} 条分红记录，钱包将到账`)
    checkedRowKeys.value = []
    fetchList()
    loadSummary()
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? '操作失败')
  } finally {
    batchLoading.value = false
  }
}
</script>

<template>
  <div style="padding: 24px; display: flex; flex-direction: column; gap: 16px;">
    <!-- 汇总卡片（仅选中单个项目时展示） -->
    <DividendSummaryCard
      v-if="filterProjectIds.length === 1"
      :summary="summary"
      :loading="summaryLoading"
    />

    <!-- 筛选栏 -->
    <NFlex align="center" :wrap="true" :gap="12">
      <NSelect
        v-model:value="filterProjectIds"
        :options="projectOptions"
        placeholder="筛选项目（支持多选）"
        multiple
        clearable
        style="min-width: 240px; max-width: 380px;"
        @update:value="onFilterChange"
      />
      <NSelect
        v-model:value="filterStatuses"
        :options="statusOptions"
        placeholder="筛选状态（默认全部）"
        multiple
        clearable
        style="width: 230px;"
        @update:value="onFilterChange"
      />
      <NButton @click="fetchList">刷新</NButton>
      <div style="flex: 1;" />
      <NButton
        type="info"
        :loading="batchLoading"
        :disabled="checkedRowKeys.length === 0"
        @click="handleConfirm"
      >
        批量确认（{{ checkedRowKeys.length }}）
      </NButton>
      <NButton
        type="primary"
        :loading="batchLoading"
        :disabled="checkedRowKeys.length === 0"
        @click="handleDistribute"
      >
        批量分发（{{ checkedRowKeys.length }}）
      </NButton>
    </NFlex>

    <!-- 数据表格 -->
    <NDataTable
      v-model:checked-row-keys="checkedRowKeys"
      :columns="columns"
      :data="list"
      :loading="loading"
      :row-key="(row: DividendDto) => row.id"
      size="small"
      striped
    />

    <!-- 分页 -->
    <NFlex justify="end">
      <NPagination
        v-model:page="page"
        v-model:page-size="pageSize"
        :item-count="total"
        :page-sizes="[20, 50, 100]"
        show-size-picker
        @update:page="fetchList"
        @update:page-size="() => { page = 1; fetchList() }"
      />
    </NFlex>
  </div>
</template>
