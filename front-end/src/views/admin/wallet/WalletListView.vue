<script setup lang="ts">
import { h, ref, onMounted } from 'vue'
import {
  NButton, NDataTable, NFlex, NTag, NText, NModal, NForm, NFormItem,
  NInputNumber, NInput, NPagination, useMessage,
  type DataTableColumns,
} from 'naive-ui'
import {
  getWalletList, adminCredit, getWalletTransactions,
} from '../../../api/admin/wallet'
import type { WalletDto, WalletTransactionDto } from '../../../types/wallet'

const message = useMessage()

// ── 钱包列表 ─────────────────────────────────────────────────
const list = ref<WalletDto[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const loading = ref(false)
const keyword = ref('')

async function fetchList() {
  loading.value = true
  try {
    const result = await getWalletList(page.value, pageSize.value, keyword.value || undefined)
    list.value = result?.items ?? []
    total.value = result?.total ?? 0
  } catch {
    message.error('加载失败')
  } finally {
    loading.value = false
  }
}
onMounted(fetchList)

// ── 充值弹窗 ─────────────────────────────────────────────────
const showCredit = ref(false)
const creditInvestorId = ref('')
const creditInvestorName = ref('')
const creditAmount = ref<number | null>(null)
const creditRemark = ref('')
const creditLoading = ref(false)

function openCredit(row: WalletDto) {
  creditInvestorId.value = row.investorUserId
  creditInvestorName.value = row.investorName
  creditAmount.value = null
  creditRemark.value = ''
  showCredit.value = true
}

async function handleCredit() {
  if (!creditAmount.value || creditAmount.value <= 0) { message.warning('请输入有效金额'); return }
  creditLoading.value = true
  try {
    await adminCredit(creditInvestorId.value, creditAmount.value, creditRemark.value || '管理员充值')
    message.success('充值成功')
    showCredit.value = false
    fetchList()
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? '充值失败')
  } finally {
    creditLoading.value = false
  }
}

// ── 流水弹窗 ─────────────────────────────────────────────────
const showTx = ref(false)
const txList = ref<WalletTransactionDto[]>([])
const txTotal = ref(0)
const txPage = ref(1)
const txLoading = ref(false)
const txInvestorId = ref('')
const txInvestorName = ref('')

async function openTransactions(row: WalletDto) {
  txInvestorId.value = row.investorUserId
  txInvestorName.value = row.investorName
  txPage.value = 1
  showTx.value = true
  await fetchTx()
}

async function fetchTx() {
  txLoading.value = true
  try {
    const result = await getWalletTransactions(txInvestorId.value, txPage.value, 20)
    txList.value = result?.items ?? []
    txTotal.value = result?.total ?? 0
  } finally {
    txLoading.value = false
  }
}

// ── 表格列 ──────────────────────────────────────────────────
const columns: DataTableColumns<WalletDto> = [
  { title: '投资人', key: 'investorName', width: 150 },
  { title: '邮箱', key: 'investorEmail', width: 200, ellipsis: { tooltip: true } },
  {
    title: '可用余额',
    key: 'balance',
    width: 160,
    render: row => h(NText, { type: 'success', style: 'font-weight:600' }, () => `${row.balance.toFixed(2)} ${row.currency}`),
  },
  {
    title: '冻结金额',
    key: 'frozenAmount',
    width: 130,
    render: row => row.frozenAmount > 0
      ? h(NTag, { type: 'warning', size: 'small' }, () => `${row.frozenAmount.toFixed(2)} ${row.currency}`)
      : h(NText, { depth: 3 }, () => '—'),
  },
  {
    title: '操作',
    key: 'actions',
    width: 180,
    render: row => h(NFlex, { gap: 8 }, () => [
      h(NButton, { size: 'small', type: 'primary', onClick: () => openCredit(row) }, () => '充值'),
      h(NButton, { size: 'small', onClick: () => openTransactions(row) }, () => '流水'),
    ]),
  },
]

const txColumns: DataTableColumns<WalletTransactionDto> = [
  {
    title: '方向',
    key: 'directionLabel',
    width: 60,
    render: row => h(NText, { type: row.direction === 'In' ? 'success' : 'error' }, () => row.directionLabel),
  },
  { title: '类型', key: 'typeLabel', width: 100 },
  {
    title: '金额',
    key: 'amount',
    width: 120,
    render: row => h(NText, { type: row.direction === 'In' ? 'success' : 'error', style: 'font-weight:600' },
      () => `${row.directionLabel}${row.amount.toFixed(2)} ${row.currency}`),
  },
  { title: '变动后余额', key: 'balanceAfter', width: 130, render: row => `${row.balanceAfter.toFixed(2)}` },
  { title: '备注', key: 'remark', ellipsis: { tooltip: true } },
  { title: '时间', key: 'createdAt', width: 160, render: row => new Date(row.createdAt).toLocaleString('zh-CN') },
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
      <NButton type="primary" @click="() => { page = 1; fetchList() }">搜索</NButton>
      <NButton @click="fetchList">刷新</NButton>
    </NFlex>

    <NDataTable
      :columns="columns"
      :data="list"
      :loading="loading"
      :row-key="(row: WalletDto) => row.id"
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

    <!-- 充值弹窗 -->
    <NModal v-model:show="showCredit" title="管理员充值" preset="card" style="width: 420px;">
      <NForm label-placement="left" label-width="80">
        <NFormItem label="投资人">
          <NText>{{ creditInvestorName }}</NText>
        </NFormItem>
        <NFormItem label="充值金额">
          <NInputNumber v-model:value="creditAmount" placeholder="请输入金额" :min="0.01" :precision="2" style="width: 100%;" />
        </NFormItem>
        <NFormItem label="备注">
          <NInput v-model:value="creditRemark" placeholder="备注（可选）" />
        </NFormItem>
      </NForm>
      <template #footer>
        <NFlex justify="end" gap="8">
          <NButton @click="showCredit = false">取消</NButton>
          <NButton type="primary" :loading="creditLoading" @click="handleCredit">确认充值</NButton>
        </NFlex>
      </template>
    </NModal>

    <!-- 流水弹窗 -->
    <NModal v-model:show="showTx" :title="`${txInvestorName} 的流水记录`" preset="card" style="width: 700px;">
      <NDataTable
        :columns="txColumns"
        :data="txList"
        :loading="txLoading"
        size="small"
        striped
      />
      <NFlex justify="end" style="margin-top:12px;">
        <NPagination
          v-model:page="txPage"
          :item-count="txTotal"
          :page-size="20"
          @update:page="fetchTx"
        />
      </NFlex>
    </NModal>
  </div>
</template>
