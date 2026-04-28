<script setup lang="ts">
import { h, ref } from 'vue'
import {
  NButton, NDataTable, NFlex, NInput, NSelect, NTag, NText,
  useMessage, type DataTableColumns,
} from 'naive-ui'
import { useRouter } from 'vue-router'
import { getContractList } from '../../../api/admin/contract'
import { usePagedList } from '../../../composables/use-paged-list'
import type { ContractDto, ContractStatus } from '../../../types/contract'

const router = useRouter()
const message = useMessage()

const projectTitleKeyword = ref('')
let searchTimer: ReturnType<typeof setTimeout> | null = null

const { list, loading, filter, search, paginationProps } = usePagedList<
  ContractDto, { status?: ContractStatus; projectTitle?: string }
>({
  fetcher: (params) =>
    getContractList(params).catch(() => {
      message.error('加载合同列表失败')
      return { items: [], total: 0, page: params.page, pageSize: params.pageSize }
    }),
  initialFilter: { status: undefined, projectTitle: undefined },
})

const statusOptions = [
  { label: '全部', value: undefined },
  { label: '已发送', value: 'Sent' as ContractStatus },
  { label: '已签署', value: 'Signed' as ContractStatus },
  { label: '已执行', value: 'Executed' as ContractStatus },
  { label: '已过期', value: 'Expired' as ContractStatus },
  { label: '等待续签', value: 'PendingRenew' as ContractStatus },
  { label: '续签中', value: 'Renewing' as ContractStatus },
]

const statusTagType: Record<ContractStatus, 'default' | 'info' | 'success' | 'warning' | 'error'> = {
  Draft: 'default', Sent: 'info', Signed: 'warning', Executed: 'success',
  Expired: 'error', PendingRenew: 'warning', Renewing: 'info', Superseded: 'default',
}

const statusLabel: Record<ContractStatus, string> = {
  Draft: '草稿', Sent: '已发送', Signed: '已签署', Executed: '已执行',
  Expired: '已过期', PendingRenew: '等待续签', Renewing: '续签中', Superseded: '已替代',
}

const columns: DataTableColumns<ContractDto> = [
  { title: '项目名称', key: 'projectTitle', ellipsis: { tooltip: true } },
  { title: '投资人', key: 'investorName', width: 140 },
  {
    title: '状态', key: 'status', width: 100,
    render: (row) =>
      h(NTag, { type: statusTagType[row.status], size: 'small', round: true }, () => statusLabel[row.status]),
  },
  {
    title: '签署时间', key: 'signedAt', width: 160,
    render: (row) =>
      row.signedAt
        ? h(NText, null, () => new Date(row.signedAt!).toLocaleDateString('zh-CN'))
        : h(NText, { depth: 3 }, () => '—'),
  },
  {
    title: '创建时间', key: 'createdAt', width: 160,
    render: (row) => h(NText, null, () => new Date(row.createdAt).toLocaleDateString('zh-CN')),
  },
  {
    title: '操作', key: 'actions', width: 100,
    render: (row) =>
      h(NButton, { size: 'small', onClick: () => router.push(`/admin/contracts/${row.id}`) }, () => '详情'),
  },
]

function onStatusChange(val: ContractStatus | undefined) {
  filter.value.status = val
  search()
}

function onProjectTitleInput(val: string) {
  projectTitleKeyword.value = val
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    filter.value.projectTitle = val.trim() || undefined
    search()
  }, 400)
}

</script>

<template>
  <div class="view-container">
    <n-flex align="center" gap="12" class="toolbar">
      <n-input
        :value="projectTitleKeyword"
        placeholder="按项目名称搜索"
        clearable
        style="width: 220px"
        @update:value="onProjectTitleInput"
        @clear="() => onProjectTitleInput('')"
      />
      <n-select
        :value="filter.status"
        :options="statusOptions"
        placeholder="筛选状态"
        clearable
        style="width: 150px"
        @update:value="onStatusChange"
      />
    </n-flex>

    <n-data-table
      :columns="columns"
      :data="list"
      :loading="loading"
      :pagination="paginationProps"
      remote
      :row-key="(row: ContractDto) => row.id"
    />


  </div>
</template>

<style scoped>
.view-container {
  padding: 16px;
}
.toolbar {
  margin-bottom: 16px;
}

</style>