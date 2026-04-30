<script setup lang="ts">
import { h, ref, onMounted } from 'vue'
import {
  NButton, NDataTable, NFlex, NSelect, NTag, NPagination,
  NModal, NInput, NForm, NFormItem, useMessage,
  type DataTableColumns,
} from 'naive-ui'
import {
  getWithdrawalList, approveWithdrawal, completeWithdrawal, rejectWithdrawal,
} from '../../../api/admin/wallet'
import type { WithdrawalRequestDto, WithdrawalStatus } from '../../../types/wallet'

const message = useMessage()

const list = ref<WithdrawalRequestDto[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const loading = ref(false)
const filterStatus = ref<string | undefined>(undefined)
const keyword = ref('')

const processingIds = ref(new Set<string>())

// 拒绝弹窗
const showReject = ref(false)
const rejectId = ref('')
const rejectReason = ref('')
const rejectLoading = ref(false)

async function fetchList() {
  loading.value = true
  try {
    const result = await getWithdrawalList(filterStatus.value, page.value, pageSize.value, keyword.value || undefined)
    list.value = result?.items ?? []
    total.value = result?.total ?? 0
  } catch {
    message.error('加载失败')
  } finally {
    loading.value = false
  }
}
onMounted(fetchList)

const statusOptions = [
  { label: '全部', value: undefined },
  { label: '待审核', value: 'Pending' },
  { label: '已批准', value: 'Approved' },
  { label: '已拒绝', value: 'Rejected' },
  { label: '已完成', value: 'Completed' },
]

const statusTagType: Record<WithdrawalStatus, 'default' | 'info' | 'warning' | 'success' | 'error'> = {
  Pending: 'warning',
  Approved: 'info',
  Rejected: 'error',
  Completed: 'success',
}

async function handleApprove(id: string) {
  processingIds.value.add(id)
  try {
    await approveWithdrawal(id)
    message.success('已批准，金额已冻结')
    fetchList()
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? '操作失败')
  } finally {
    processingIds.value.delete(id)
  }
}

async function handleComplete(id: string) {
  processingIds.value.add(id)
  try {
    await completeWithdrawal(id)
    message.success('已标记完成，线下打款已确认')
    fetchList()
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? '操作失败')
  } finally {
    processingIds.value.delete(id)
  }
}

function openReject(id: string) {
  rejectId.value = id
  rejectReason.value = ''
  showReject.value = true
}

async function handleReject() {
  if (!rejectReason.value.trim()) { message.warning('请填写拒绝原因'); return }
  rejectLoading.value = true
  try {
    await rejectWithdrawal(rejectId.value, rejectReason.value)
    message.success('已拒绝，余额已解冻')
    showReject.value = false
    fetchList()
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? '操作失败')
  } finally {
    rejectLoading.value = false
  }
}

const columns: DataTableColumns<WithdrawalRequestDto> = [
  { title: '投资人', key: 'investorName', width: 140 },
  {
    title: '金额',
    key: 'amount',
    width: 140,
    render: row => h('span', { style: 'font-weight:600' }, `${row.amount.toFixed(2)} ${row.currency}`),
  },
  {
    title: '状态',
    key: 'status',
    width: 100,
    render: row => h(NTag, { type: statusTagType[row.status], size: 'small' }, () => row.statusLabel),
  },
  { title: '收款方式', key: 'paymentMethod', ellipsis: { tooltip: true } },
  { title: '拒绝原因', key: 'rejectReason', ellipsis: { tooltip: true } },
  { title: '申请时间', key: 'createdAt', width: 160, render: row => new Date(row.createdAt).toLocaleString('zh-CN') },
  { title: '处理时间', key: 'processedAt', width: 160, render: row => row.processedAt ? new Date(row.processedAt).toLocaleString('zh-CN') : '—' },
  {
    title: '操作',
    key: 'actions',
    width: 200,
    render: row => h(NFlex, { gap: 6 }, () => [
      row.status === 'Pending'
        ? h(NButton, {
            size: 'small', type: 'primary',
            loading: processingIds.value.has(row.id),
            disabled: processingIds.value.has(row.id),
            onClick: () => handleApprove(row.id),
          }, () => '批准')
        : null,
      row.status === 'Approved'
        ? h(NButton, {
            size: 'small', type: 'success',
            loading: processingIds.value.has(row.id),
            disabled: processingIds.value.has(row.id),
            onClick: () => handleComplete(row.id),
          }, () => '标记完成')
        : null,
      row.status === 'Pending' || row.status === 'Approved'
        ? h(NButton, {
            size: 'small', type: 'error',
            onClick: () => openReject(row.id),
          }, () => '拒绝')
        : null,
    ]),
  },
]
</script>

<template>
  <div style="padding: 24px; display: flex; flex-direction: column; gap: 16px;">
    <NFlex align="center" gap="12">
      <NInput
        v-model:value="keyword"
        placeholder="搜索投资人姓名或邮箱"
        clearable
        style="width: 240px;"
        @keydown.enter="() => { page = 1; fetchList() }"
        @clear="() => { page = 1; fetchList() }"
      />
      <NSelect
        v-model:value="filterStatus"
        :options="statusOptions"
        style="width: 140px;"
        @update:value="() => { page = 1; fetchList() }"
      />
      <NButton type="primary" @click="() => { page = 1; fetchList() }">搜索</NButton>
      <NButton @click="fetchList">刷新</NButton>
    </NFlex>

    <NDataTable
      :columns="columns"
      :data="list"
      :loading="loading"
      :row-key="(row: WithdrawalRequestDto) => row.id"
      size="small"
      striped
    />

    <NFlex justify="end">
      <NPagination
        v-model:page="page"
        v-model:page-size="pageSize"
        :item-count="total"
        :page-sizes="[20, 50]"
        show-size-picker
        @update:page="fetchList"
        @update:page-size="() => { page = 1; fetchList() }"
      />
    </NFlex>

    <!-- 拒绝弹窗 -->
    <NModal v-model:show="showReject" title="拒绝提现申请" preset="card" style="width: 420px;">
      <NForm label-placement="left" label-width="80">
        <NFormItem label="拒绝原因">
          <NInput v-model:value="rejectReason" type="textarea" placeholder="请填写拒绝原因" :rows="3" />
        </NFormItem>
      </NForm>
      <template #footer>
        <NFlex justify="end" gap="8">
          <NButton @click="showReject = false">取消</NButton>
          <NButton type="error" :loading="rejectLoading" @click="handleReject">确认拒绝</NButton>
        </NFlex>
      </template>
    </NModal>
  </div>
</template>
