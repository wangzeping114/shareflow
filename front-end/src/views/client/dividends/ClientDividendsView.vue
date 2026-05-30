<script setup lang="ts">
import { h, ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  NDataTable, NFlex, NPagination, NSpin, NTag, NText,
  type DataTableColumns,
} from 'naive-ui'
import { getClientDividends } from '../../../api/client/dashboard'
import type { ClientDividendDto } from '../../../types/client'

const { t, locale } = useI18n()

function fmtDate(iso: string) {
  return new Date(iso).toLocaleDateString(locale.value, { year: 'numeric', month: 'short', day: 'numeric' })
}

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

function dividendStatusLabel(row: ClientDividendDto) {
  const key = `client.dividendStatus.${row.status}`
  const label = t(key)
  return label === key ? row.statusLabel : label
}

const columns: DataTableColumns<ClientDividendDto> = [
  { title: t('client.col.project'), key: 'projectTitle', ellipsis: { tooltip: true } },
  { title: t('client.col.platform'), key: 'platformName', width: 160 },
  {
    title: t('client.col.equityPct'),
    key: 'sharePermille',
    width: 100,
    render: row => `${(row.sharePermille / 10).toFixed(2)}%`,
  },
  {
    title: t('client.col.revenueBase'),
    key: 'revenueAmount',
    width: 150,
    render: row => `${row.revenueAmount.toFixed(2)} ${row.currency}`,
  },
  {
    title: t('client.col.dividendAmount'),
    key: 'dividendAmount',
    width: 160,
    render: row => h(NText, { type: 'success', style: 'font-weight:600' },
      () => `${row.dividendAmount.toFixed(2)} ${row.currency}`),
  },
  {
    title: t('client.col.status'),
    key: 'status',
    width: 100,
    render: row => h(NTag, { type: statusTagType[row.status] ?? 'default', size: 'small' }, () => dividendStatusLabel(row)),
  },
  {
    title: t('client.col.date'),
    key: 'calculatedAt',
    width: 160,
    render: row => fmtDate(row.calculatedAt),
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
