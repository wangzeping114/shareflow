<script setup lang="ts">
import { h, ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  NDataTable, NSpin, NTag, NButton, useMessage,
  type DataTableColumns,
} from 'naive-ui'
import { getClientContracts, getContractPdfPath } from '../../../api/client/dashboard'
import type { ClientContractDto } from '../../../types/client'

const { t, locale } = useI18n()

function fmtDate(iso: string) {
  return new Date(iso).toLocaleDateString(locale.value, { year: 'numeric', month: 'short', day: 'numeric' })
}

const message = useMessage()
const list = ref<ClientContractDto[]>([])
const loading = ref(false)
const downloadingIds = ref(new Set<string>())

async function fetchList() {
  loading.value = true
  try {
    list.value = (await getClientContracts()) ?? []
  } finally {
    loading.value = false
  }
}
onMounted(fetchList)

async function downloadPdf(row: ClientContractDto) {
  if (!row.hasPdf) { message.warning(t('client.noPdfAvailable')); return }
  downloadingIds.value.add(row.id)
  try {
    const result = await getContractPdfPath(row.id)
    if (result?.path) {
      window.open(result.path, '_blank')
    } else {
      message.warning(t('client.pdfNotReady'))
    }
  } catch {
    message.error(t('client.fetchLinkFailed'))
  } finally {
    downloadingIds.value.delete(row.id)
  }
}

const statusTagType: Record<string, 'default' | 'info' | 'success' | 'warning' | 'error'> = {
  Draft:        'default',
  Sent:         'info',
  Signed:       'success',
  Executed:     'success',
  Expired:      'error',
  PendingRenew: 'warning',
  Renewing:     'warning',
  Superseded:   'default',
}

function contractStatusLabel(row: ClientContractDto) {
  const key = `contract.status.${row.status}`
  const label = t(key)
  return label === key ? row.statusLabel : label
}

const columns: DataTableColumns<ClientContractDto> = [
  { title: t('client.col.project'), key: 'projectTitle', ellipsis: { tooltip: true } },
  { title: t('client.col.platform'), key: 'platformName', width: 160 },
  {
    title: t('client.col.equityPct'),
    key: 'sharePermille',
    width: 100,
    render: row => `${(row.sharePermille / 10).toFixed(2)}%`,
  },
  {
    title: t('client.col.status'),
    key: 'status',
    width: 110,
    render: row => h(NTag, { type: statusTagType[row.status] ?? 'default', size: 'small' },
      () => contractStatusLabel(row)),
  },
  {
    title: t('client.col.signedDate'),
    key: 'signedAt',
    width: 160,
    render: row => row.signedAt ? fmtDate(row.signedAt) : '—',
  },
  {
    title: t('client.col.actions'),
    key: 'actions',
    width: 130,
    render: row => h(NButton, {
      size: 'small',
      disabled: !row.hasPdf || downloadingIds.value.has(row.id),
      loading: downloadingIds.value.has(row.id),
      onClick: () => downloadPdf(row),
    }, () => t('client.downloadPdf')),
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
        :row-key="(row: ClientContractDto) => row.id"
        size="small"
        striped
      />
    </n-spin>
  </div>
</template>
