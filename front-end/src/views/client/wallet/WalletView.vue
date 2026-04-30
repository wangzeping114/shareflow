<script setup lang="ts">
import { h, ref, onMounted, computed } from 'vue'
import {
  NCard, NFlex, NText, NStatistic, NDivider, NTimeline, NTimelineItem,
  NButton, NModal, NForm, NFormItem, NInputNumber, NInput, NTag,
  NPagination, useMessage,
} from 'naive-ui'
import { getMyWallet, getMyTransactions, requestWithdrawal, getMyWithdrawals } from '../../api/public/wallet'
import type { WalletDto, WalletTransactionDto, WithdrawalRequestDto, WithdrawalStatus } from '../../types/wallet'

const message = useMessage()

// ── 钱包余额 ─────────────────────────────────────────────────
const wallet = ref<WalletDto | null>(null)
const walletLoading = ref(false)

async function fetchWallet() {
  walletLoading.value = true
  try {
    wallet.value = await getMyWallet()
  } catch {
    // 钱包不存在时为空
  } finally {
    walletLoading.value = false
  }
}

// ── 流水列表 ─────────────────────────────────────────────────
const txList = ref<WalletTransactionDto[]>([])
const txTotal = ref(0)
const txPage = ref(1)
const txLoading = ref(false)

async function fetchTx() {
  txLoading.value = true
  try {
    const result = await getMyTransactions(txPage.value, 20)
    txList.value = result?.items ?? []
    txTotal.value = result?.total ?? 0
  } finally {
    txLoading.value = false
  }
}

// ── 提现历史 ─────────────────────────────────────────────────
const withdrawals = ref<WithdrawalRequestDto[]>([])

async function fetchWithdrawals() {
  try {
    const result = await getMyWithdrawals(1, 10)
    withdrawals.value = result?.items ?? []
  } catch {}
}

onMounted(() => {
  fetchWallet()
  fetchTx()
  fetchWithdrawals()
})

// ── 申请提现弹窗 ─────────────────────────────────────────────
const showWithdraw = ref(false)
const withdrawAmount = ref<number | null>(null)
const withdrawPayment = ref('')
const withdrawLoading = ref(false)

async function handleWithdraw() {
  if (!withdrawAmount.value || withdrawAmount.value <= 0) { message.warning('请输入有效金额'); return }
  if (!wallet.value || withdrawAmount.value > wallet.value.balance) { message.warning('余额不足'); return }
  withdrawLoading.value = true
  try {
    await requestWithdrawal(withdrawAmount.value, withdrawPayment.value || undefined)
    message.success('提现申请已提交，等待管理员审核')
    showWithdraw.value = false
    fetchWallet()
    fetchWithdrawals()
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? '提申失败')
  } finally {
    withdrawLoading.value = false
  }
}

const statusTagMap: Record<WithdrawalStatus, { type: 'default' | 'info' | 'warning' | 'success' | 'error'; label: string }> = {
  Pending: { type: 'warning', label: '待审核' },
  Approved: { type: 'info', label: '已批准' },
  Rejected: { type: 'error', label: '已拒绝' },
  Completed: { type: 'success', label: '已完成' },
}
</script>

<template>
  <div style="padding: 24px; max-width: 800px; margin: 0 auto; display: flex; flex-direction: column; gap: 24px;">
    <!-- 余额卡片 -->
    <NCard title="我的钱包" :loading="walletLoading">
      <template v-if="wallet">
        <NFlex :gap="32" align="flex-end">
          <NStatistic label="可用余额">
            <template #default>
              <NText type="success" style="font-size: 32px; font-weight: 700;">
                {{ wallet.balance.toFixed(2) }} {{ wallet.currency }}
              </NText>
            </template>
          </NStatistic>
          <NStatistic v-if="wallet.frozenAmount > 0" label="冻结中（提现审核）">
            <NText type="warning" style="font-size: 20px;">
              {{ wallet.frozenAmount.toFixed(2) }} {{ wallet.currency }}
            </NText>
          </NStatistic>
          <div style="flex: 1;" />
          <NButton
            type="primary"
            :disabled="!wallet || wallet.balance <= 0"
            @click="showWithdraw = true"
          >
            申请提现
          </NButton>
        </NFlex>
      </template>
      <NText v-else depth="3">钱包尚未初始化，分红到账后将自动创建</NText>
    </NCard>

    <!-- 最近提现记录 -->
    <NCard v-if="withdrawals.length > 0" title="提现记录">
      <div v-for="w in withdrawals" :key="w.id" style="display:flex;justify-content:space-between;padding:8px 0;border-bottom:1px solid #eee;">
        <NText>{{ w.amount.toFixed(2) }} {{ w.currency }}</NText>
        <NText depth="3">{{ new Date(w.createdAt).toLocaleDateString('zh-CN') }}</NText>
        <NTag :type="statusTagMap[w.status].type" size="small">{{ statusTagMap[w.status].label }}</NTag>
      </div>
    </NCard>

    <!-- 流水 Timeline -->
    <NCard title="资金流水">
      <NTimeline v-if="txList.length > 0">
        <NTimelineItem
          v-for="tx in txList"
          :key="tx.id"
          :type="tx.direction === 'In' ? 'success' : 'error'"
          :title="`${tx.typeLabel}  ${tx.directionLabel}${tx.amount.toFixed(2)} ${tx.currency}`"
          :content="tx.remark"
          :time="new Date(tx.createdAt).toLocaleString('zh-CN')"
        />
      </NTimeline>
      <NText v-else depth="3">暂无流水记录</NText>
      <NFlex justify="end" style="margin-top: 16px;">
        <NPagination
          v-model:page="txPage"
          :item-count="txTotal"
          :page-size="20"
          @update:page="fetchTx"
        />
      </NFlex>
    </NCard>

    <!-- 提现申请弹窗 -->
    <NModal v-model:show="showWithdraw" title="申请提现" preset="card" style="width: 440px;">
      <NForm label-placement="left" label-width="90">
        <NFormItem label="提现金额">
          <NInputNumber
            v-model:value="withdrawAmount"
            placeholder="请输入金额"
            :min="0.01"
            :max="wallet?.balance ?? undefined"
            :precision="2"
            style="width: 100%;"
          />
        </NFormItem>
        <NFormItem label="收款方式">
          <NInput v-model:value="withdrawPayment" type="textarea" placeholder="银行账号 / 支付宝 / 收款人等（线下入账信息）" :rows="3" />
        </NFormItem>
      </NForm>
      <template #footer>
        <NFlex justify="end" gap="8">
          <NButton @click="showWithdraw = false">取消</NButton>
          <NButton type="primary" :loading="withdrawLoading" @click="handleWithdraw">提交申请</NButton>
        </NFlex>
      </template>
    </NModal>
  </div>
</template>
