<script setup lang="ts">
import { h, ref, onMounted } from 'vue'
import {
  NDataTable, NFlex, NSpin, NTag, NButton, useMessage,
  type DataTableColumns,
} from 'naive-ui'
import { getClientContracts, getContractPdfPath } from '../../../api/client/dashboard'
import type { ClientContractDto } from '../../../types/client'

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
  if (!row.hasPdf) { message.warning('该合同暂无 PDF'); return }
  downloadingIds.value.add(row.id)
  try {
    const result = await getContractPdfPath(row.id)
    if (result?.path) {
      window.open(result.path, '_blank')
    } else {
      message.warning('PDF 暂不可用')
    }
  } catch {
    message.error('获取下载链接失败')
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

const columns: DataTableColumns<ClientContractDto> = [
  { title: '项目名称', key: 'projectTitle', ellipsis: { tooltip: true } },
  { title: '平台', key: 'platformName', width: 120 },
  {
    title: '持股比例',
    key: 'sharePermille',
    width: 100,
    render: row => `${(row.sharePermille / 10).toFixed(2)}%`,
  },
  {
    title: '状态',
    key: 'status',
    width: 110,
    render: row => h(NTag, { type: statusTagType[row.status] ?? 'default', size: 'small' },
      () => row.statusLabel),
  },
  {
    title: '签署时间',
    key: 'signedAt',
    width: 160,
    render: row => row.signedAt ? new Date(row.signedAt).toLocaleDateString('zh-CN') : '—',
  },
  {
    title: '操作',
    key: 'actions',
    width: 120,
    render: row => h(NButton, {
      size: 'small',
      disabled: !row.hasPdf || downloadingIds.value.has(row.id),
      loading: downloadingIds.value.has(row.id),
      onClick: () => downloadPdf(row),
    }, () => '下载 PDF'),
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
