<script setup lang="ts">
import { h, ref, onMounted } from 'vue'
import {
  NDataTable, NFlex, NPagination, NSpin, NTag, NText, NSelect,
  type DataTableColumns,
} from 'naive-ui'
import { getClientDividends } from '../../../api/client/dashboard'
import type { ClientDividendDto } from '../../../types/client'

const list = ref<ClientDividendDto[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const loading = ref(false)

async function fetchList() {
  loading.value = true
  try {
    const result = await getClientDividends(page.value, pageSize.value)
    list.value = result?.items ?? []
    total.value = result?.total ?? 0
  } finally {
    loading.value = false
  }
}
onMounted(fetchList)

const statusTagType: Record<string, 'default' | 'info' | 'warning' | 'success'> = {
  Calculated: 'warning',
  Confirmed:  'info',
  Distributed: 'success',
}

const columns: DataTableColumns<ClientDividendDto> = [
  { title: '项目名称', key: 'projectTitle', ellipsis: { tooltip: true } },
  { title: '平台', key: 'platformName', width: 120 },
  {
    title: '持股比例',
    key: 'sharePermille',
    width: 100,
    render: row => `${(row.sharePermille / 10).toFixed(2)}%`,
  },
  {
    title: '收益基数',
    key: 'revenueAmount',
    width: 130,
    render: row => `${row.revenueAmount.toFixed(2)} ${row.currency}`,
  },
  {
    title: '分红金额',
    key: 'dividendAmount',
    width: 140,
    render: row => h(NText, { type: 'success', style: 'font-weight:600' },
      () => `${row.dividendAmount.toFixed(2)} ${row.currency}`),
  },
  {
    title: '状态',
    key: 'status',
    width: 90,
    render: row => h(NTag, { type: statusTagType[row.status] ?? 'default', size: 'small' }, () => row.statusLabel),
  },
  {
    title: '时间',
    key: 'calculatedAt',
    width: 160,
    render: row => new Date(row.calculatedAt).toLocaleDateString('zh-CN'),
  },
]
</script>

<template>
  <div style="display: flex; flex-direction: column; gap: 16px;">
    <n-spin :show="loading">
      <n-data-table
        :columns="columns"
        :data="list"
        :loading="loading"
        :row-key="(row: ClientDividendDto) => row.id"
        size="small"
        striped
      />
    </n-spin>

    <n-flex justify="end">
      <n-pagination
        v-model:page="page"
        v-model:page-size="pageSize"
        :item-count="total"
        :page-sizes="[20, 50]"
        show-size-picker
        @update:page="fetchList"
        @update:page-size="() => { page = 1; fetchList() }"
      />
    </n-flex>
  </div>
</template>
