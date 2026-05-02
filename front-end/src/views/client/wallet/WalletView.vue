<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import {
  NCard, NFlex, NText, NStatistic, NTimeline, NTimelineItem,
  NButton, NModal, NForm, NFormItem, NInputNumber, NInput, NTag,
  NPagination, useMessage,
} from 'naive-ui'
import { getMyWallet, getMyTransactions, requestWithdrawal, getMyWithdrawals } from '../../../api/public/wallet'
import { useRegion } from '../../../composables/use-region'
import type { WalletDto, WalletTransactionDto, WithdrawalRequestDto, WithdrawalStatus } from '../../../types/wallet'

const { t } = useI18n()
const { clientLocale } = useRegion()

function fmtDate(iso: string) {
  return new Date(iso).toLocaleDateString(clientLocale, { year: 'numeric', month: 'short', day: 'numeric' })
}
function fmtDateTime(iso: string) {
  return new Date(iso).toLocaleString(clientLocale, { year: 'numeric', month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })
}

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
  if (!withdrawAmount.value || withdrawAmount.value <= 0) { message.warning(t('client.withdrawal.invalidAmount')); return }
  if (!wallet.value || withdrawAmount.value > wallet.value.balance) { message.warning(t('client.withdrawal.insufficientBalance')); return }
  withdrawLoading.value = true
  try {
    await requestWithdrawal(withdrawAmount.value, withdrawPayment.value || undefined)
    message.success(t('client.withdrawal.submitted'))
    showWithdraw.value = false
    fetchWallet()
    fetchWithdrawals()
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? t('client.withdrawal.failed'))
  } finally {
    withdrawLoading.value = false
  }
}

const statusTagMap: Record<WithdrawalStatus, { type: 'default' | 'info' | 'warning' | 'success' | 'error'; label: string }> = {
  Pending:   { type: 'warning', label: t('client.withdrawal.statusPending') },
  Approved:  { type: 'info',    label: t('client.withdrawal.statusApproved') },
  Rejected:  { type: 'error',   label: t('client.withdrawal.statusRejected') },
  Completed: { type: 'success', label: t('client.withdrawal.statusCompleted') },
}
</script>

<template>
  <div style="padding: 24px; max-width: 860px; margin: 0 auto; display: flex; flex-direction: column; gap: 24px;">
    <!-- Wallet balance card -->
    <NCard :title="t('client.myWallet')" :loading="walletLoading">
      <template v-if="wallet">
        <NFlex :gap="32" align="flex-end">
          <NStatistic :label="t('client.availableBalance')">
            <template #default>
              <NText type="success" style="font-size: 32px; font-weight: 700;">
                {{ wallet.balance.toFixed(2) }} {{ wallet.currency }}
              </NText>
            </template>
          </NStatistic>
          <NStatistic v-if="wallet.frozenAmount > 0" :label="t('client.frozenBalance')">
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
            {{ t('client.requestWithdrawal') }}
          </NButton>
        </NFlex>
      </template>
      <NText v-else depth="3">{{ t('client.walletEmpty') }}</NText>
    </NCard>

    <!-- Withdrawal history -->
    <NCard v-if="withdrawals.length > 0" :title="t('client.withdrawalHistory')">
      <div v-for="w in withdrawals" :key="w.id" style="display:flex;justify-content:space-between;align-items:center;padding:10px 0;border-bottom:1px solid #f0f0f0;">
        <NText style="font-weight:600">{{ w.amount.toFixed(2) }} {{ w.currency }}</NText>
        <NText depth="3" style="font-size:13px">{{ fmtDate(w.createdAt) }}</NText>
        <NTag :type="statusTagMap[w.status].type" size="small">{{ statusTagMap[w.status].label }}</NTag>
      </div>
    </NCard>

    <!-- Transaction history -->
    <NCard :title="t('client.transactionHistory')">
      <NTimeline v-if="txList.length > 0">
        <NTimelineItem
          v-for="tx in txList"
          :key="tx.id"
          :type="tx.direction === 'In' ? 'success' : 'error'"
          :title="`${tx.typeLabel}  ${tx.directionLabel}${tx.amount.toFixed(2)} ${tx.currency}`"
          :content="tx.remark"
          :time="fmtDateTime(tx.createdAt)"
        />
      </NTimeline>
      <NText v-else depth="3">{{ t('client.noTransactions') }}</NText>
      <NFlex justify="end" style="margin-top: 16px;">
        <NPagination
          v-model:page="txPage"
          :item-count="txTotal"
          :page-size="20"
          @update:page="fetchTx"
        />
      </NFlex>
    </NCard>

    <!-- Withdrawal request modal -->
    <NModal v-model:show="showWithdraw" :title="t('client.withdrawal.title')" preset="card" style="width: 480px;">
      <NForm label-placement="top">
        <NFormItem :label="t('client.withdrawal.amount')">
          <NInputNumber
            v-model:value="withdrawAmount"
            :placeholder="t('client.withdrawal.amountPlaceholder')"
            :min="0.01"
            :max="wallet?.balance ?? undefined"
            :precision="2"
            style="width: 100%;"
          />
        </NFormItem>
        <NFormItem :label="t('client.withdrawal.paymentDetails')">
          <NInput v-model:value="withdrawPayment" type="textarea" :placeholder="t('client.withdrawal.paymentPlaceholder')" :rows="3" />
        </NFormItem>
      </NForm>
      <template #footer>
        <NFlex justify="end" gap="8">
          <NButton @click="showWithdraw = false">{{ t('client.withdrawal.cancel') }}</NButton>
          <NButton type="primary" :loading="withdrawLoading" @click="handleWithdraw">{{ t('client.withdrawal.submit') }}</NButton>
        </NFlex>
      </template>
    </NModal>
  </div>
</template>
