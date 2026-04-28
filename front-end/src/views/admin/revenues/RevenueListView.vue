<script setup lang="ts">
import { h, ref, watch } from 'vue'
import {
  NButton, NDataTable, NFlex, NSelect, NTag, NText, NModal, NForm,
  NFormItem, NInput, NInputNumber, NDatePicker, useMessage,
  type DataTableColumns, type DataTableRowKey,
} from 'naive-ui'
import { getRevenueList, approveRevenue, rejectRevenue, addManualRevenue } from '../../../api/admin/revenue'
import { usePagedList } from '../../../composables/use-paged-list'
import type { RevenueDto, RevenueStatus } from '../../../types/revenue'
import CsvImporterDrawer from './CsvImporterDrawer.vue'
import AiScreenshotUploader from './AiScreenshotUploader.vue'
import { useProjectSelect, getProjectPlatformOptions, getProjectPlatformValues } from '../../../composables/use-project-select'

const message = useMessage()

const showManualAdd = ref(false)
const showReject = ref(false)
const rejectId = ref('')
const rejectReason = ref('')
const rejectLoading = ref(false)
const manualLoading = ref(false)
const showCsvDrawer = ref(false)
const showAiUploader = ref(false)

// 批量操作
const checkedRowKeys = ref<DataTableRowKey[]>([])
const batchLoading = ref(false)

// 手工录入 — 项目下拉
const { projectOptions, projectsLoading } = useProjectSelect()
const manualPlatformOptions = ref<{ label: string; value: string }[]>([])
const manualForm = ref({
  projectId: null as string | null,
  platformNames: [] as string[],
  amount: null as number | null,
  currency: 'USD',
  revenueDate: null as number | null,
})

// 选中项目后仅展示该项目的平台并自动全选
watch(() => manualForm.value.projectId, (id) => {
  if (!id) { manualPlatformOptions.value = []; return }
  const opt = projectOptions.value.find((p) => p.value === id)
  if (opt) {
    manualPlatformOptions.value = getProjectPlatformOptions(opt.platformName)
    manualForm.value.platformNames = getProjectPlatformValues(opt.platformName)
  }
})

const platformKeyword = ref('')
let platformTimer: ReturnType<typeof setTimeout> | null = null

const { list, loading, filter, search, reload, paginationProps } = usePagedList<
  RevenueDto, { status?: RevenueStatus; platformName?: string }
>({
  fetcher: (params) =>
    getRevenueList(params).catch(() => {
      message.error('加载收益列表失败')
      return { items: [], total: 0, page: params.page, pageSize: params.pageSize }
    }),
  initialFilter: { status: undefined, platformName: undefined },
})

const statusOptions = [
  { label: '全部', value: undefined },
  { label: '待审核', value: 'Pending' as RevenueStatus },
  { label: '需核实', value: 'NeedsVerification' as RevenueStatus },
  { label: '已通过', value: 'Approved' as RevenueStatus },
  { label: '已拒绝', value: 'Rejected' as RevenueStatus },
]

const statusTagType: Record<RevenueStatus, 'default' | 'info' | 'success' | 'warning' | 'error'> = {
  Pending: 'info',
  NeedsVerification: 'warning',
  Approved: 'success',
  Rejected: 'error',
}

const statusLabel: Record<RevenueStatus, string> = {
  Pending: '待审核',
  NeedsVerification: '需核实',
  Approved: '已通过',
  Rejected: '已拒绝',
}

const sourceLabel: Record<string, string> = {
  Manual: '手工',
  Csv: 'CSV',
  AiScreenshot: 'AI识别',
}

const columns: DataTableColumns<RevenueDto> = [
  { type: 'selection', disabled: (row) => row.status !== 'Pending' && row.status !== 'NeedsVerification' },
  { title: '项目名称', key: 'projectTitle', ellipsis: { tooltip: true } },
  { title: '平台', key: 'platformName', width: 110 },
  {
    title: '金额', key: 'amount', width: 120,
    render: (row) => h(NText, null, () => `${row.amount.toFixed(2)} ${row.currency}`),
  },
  {
    title: '收益日期', key: 'revenueDate', width: 120,
    render: (row) => h(NText, null, () => row.revenueDate.slice(0, 10)),
  },
  {
    title: '来源', key: 'importSource', width: 90,
    render: (row) => h(NTag, { size: 'small' }, () => sourceLabel[row.importSource] ?? row.importSource),
  },
  {
    title: '状态', key: 'status', width: 100,
    render: (row) =>
      h(NTag, { type: statusTagType[row.status], size: 'small', round: true }, () => statusLabel[row.status]),
  },
  {
    title: 'AI置信度', key: 'aiConfidence', width: 100,
    render: (row) =>
      row.importSource === 'AiScreenshot'
        ? h(NText, { type: row.aiConfidence < 0.7 ? 'warning' : 'default' },
            () => `${(row.aiConfidence * 100).toFixed(0)}%`)
        : h(NText, { depth: 3 }, () => '—'),
  },
  {
    title: '操作', key: 'actions', width: 160,
    render: (row) =>
      h(NFlex, { gap: 6 }, () => [
        row.status === 'Pending' || row.status === 'NeedsVerification'
          ? h(NButton, {
              size: 'small', type: 'primary',
              onClick: () => handleApprove(row.id),
            }, () => '通过')
          : null,
        row.status === 'Pending' || row.status === 'NeedsVerification'
          ? h(NButton, {
              size: 'small', type: 'error',
              onClick: () => openReject(row.id),
            }, () => '拒绝')
          : null,
      ]),
  },
]

async function handleApprove(id: string) {
  try {
    await approveRevenue(id)
    message.success('已通过审核')
    reload()
  } catch {
    message.error('操作失败')
  }
}

async function handleBatchApprove() {
  if (!checkedRowKeys.value.length) return
  batchLoading.value = true
  try {
    await Promise.all(checkedRowKeys.value.map((id) => approveRevenue(id as string)))
    message.success(`批量审核通过 ${checkedRowKeys.value.length} 条`)
    checkedRowKeys.value = []
    reload()
  } catch {
    message.error('部分操作失败')
  } finally {
    batchLoading.value = false
  }
}

const showBatchReject = ref(false)
const batchRejectReason = ref('')
const batchRejectLoading = ref(false)

async function handleBatchReject() {
  if (!batchRejectReason.value.trim()) {
    message.warning('请输入拒绝原因')
    return
  }
  batchRejectLoading.value = true
  try {
    await Promise.all(
      checkedRowKeys.value.map((id) =>
        rejectRevenue(id as string, { reason: batchRejectReason.value })
      )
    )
    message.success(`批量拒绝 ${checkedRowKeys.value.length} 条`)
    checkedRowKeys.value = []
    showBatchReject.value = false
    batchRejectReason.value = ''
    reload()
  } catch {
    message.error('部分操作失败')
  } finally {
    batchRejectLoading.value = false
  }
}

function openReject(id: string) {
  rejectId.value = id
  rejectReason.value = ''
  showReject.value = true
}

async function handleReject() {
  if (!rejectReason.value.trim()) {
    message.warning('请输入拒绝原因')
    return
  }
  rejectLoading.value = true
  try {
    await rejectRevenue(rejectId.value, { reason: rejectReason.value })
    message.success('已拒绝')
    showReject.value = false
    reload()
  } catch {
    message.error('操作失败')
  } finally {
    rejectLoading.value = false
  }
}

async function handleManualAdd() {
  if (!manualForm.value.projectId || !manualForm.value.platformNames.length
    || !manualForm.value.amount || !manualForm.value.revenueDate) {
    message.warning('请填写完整信息')
    return
  }
  manualLoading.value = true
  try {
    const date = new Date(manualForm.value.revenueDate)
    await Promise.all(
      manualForm.value.platformNames.map((platform) =>
        addManualRevenue({
          projectId: manualForm.value.projectId!,
          platformName: platform,
          amount: manualForm.value.amount!,
          currency: manualForm.value.currency,
          revenueDate: date.toISOString(),
        })
      )
    )
    const count = manualForm.value.platformNames.length
    message.success(count > 1 ? `成功录入 ${count} 条收益记录` : '手工录入成功')
    showManualAdd.value = false
    manualForm.value = { projectId: null, platformNames: [], amount: null, currency: 'USD', revenueDate: null }
    manualPlatformOptions.value = []
    reload()
  } catch {
    message.error('录入失败')
  } finally {
    manualLoading.value = false
  }
}

function onPlatformInput(val: string) {
  platformKeyword.value = val
  if (platformTimer) clearTimeout(platformTimer)
  platformTimer = setTimeout(() => {
    filter.value.platformName = val.trim() || undefined
    search()
  }, 400)
}
</script>

<template>
  <div class="view-container">
    <n-flex align="center" gap="12" class="toolbar">
      <n-input
        :value="platformKeyword"
        placeholder="按平台名搜索"
        clearable
        style="width: 180px"
        @update:value="onPlatformInput"
        @clear="() => onPlatformInput('')"
      />
      <n-select
        :value="filter.status"
        :options="statusOptions"
        placeholder="筛选状态"
        clearable
        style="width: 140px"
        @update:value="(v) => { filter.status = v; search() }"
      />
      <n-flex style="margin-left: auto" gap="8">
        <n-button @click="showManualAdd = true">手工录入</n-button>
        <n-button @click="showCsvDrawer = true">CSV 导入</n-button>
        <n-button @click="showAiUploader = true">AI 截图识别</n-button>
      </n-flex>
    </n-flex>

    <!-- 批量操作栏 -->
    <n-flex v-if="checkedRowKeys.length > 0" align="center" gap="10" style="margin: 8px 0; padding: 8px 12px; background: var(--n-color); border-radius: 6px; border: 1px solid var(--n-border-color)">
      <n-text>已选 {{ checkedRowKeys.length }} 条</n-text>
      <n-button size="small" type="primary" :loading="batchLoading" @click="handleBatchApprove">批量通过</n-button>
      <n-button size="small" type="error" @click="showBatchReject = true">批量拒绝</n-button>
      <n-button size="small" quaternary @click="checkedRowKeys = []">取消选择</n-button>
    </n-flex>

    <n-data-table
      v-model:checked-row-keys="checkedRowKeys"
      :columns="columns"
      :data="list"
      :loading="loading"
      :pagination="paginationProps"
      remote
      :row-key="(row: RevenueDto) => row.id"
    />

    <!-- 手工录入弹窗 -->
    <n-modal
      v-model:show="showManualAdd"
      title="手工录入收益"
      preset="card"
      style="width: 480px"
    >
      <n-form label-placement="top">
        <n-form-item label="项目" required>
          <n-select
            v-model:value="manualForm.projectId"
            :options="projectOptions"
            :loading="projectsLoading"
            filterable
            placeholder="搜索并选择项目"
          />
        </n-form-item>
        <n-form-item label="平台" required>
          <n-select
            v-model:value="manualForm.platformNames"
            :options="manualPlatformOptions"
            multiple
            :disabled="!manualForm.projectId"
            placeholder="请先选择项目"
          />
        </n-form-item>
        <n-flex gap="12">
          <n-form-item label="金额" required style="flex:1">
            <n-input-number
              v-model:value="manualForm.amount"
              :min="0.01"
              :precision="2"
              style="width:100%"
              placeholder="0.00"
            />
          </n-form-item>
          <n-form-item label="货币" required style="width:110px">
            <n-select
              v-model:value="manualForm.currency"
              :options="[{ label: 'USD', value: 'USD' }, { label: 'CNY', value: 'CNY' }]"
            />
          </n-form-item>
        </n-flex>
        <n-form-item label="收益日期" required>
          <n-date-picker
            v-model:value="manualForm.revenueDate"
            type="date"
            style="width: 100%"
          />
        </n-form-item>
      </n-form>
      <n-flex justify="end" gap="12" style="margin-top: 16px">
        <n-button @click="showManualAdd = false">取消</n-button>
        <n-button type="primary" :loading="manualLoading" @click="handleManualAdd">确认录入</n-button>
      </n-flex>
    </n-modal>

    <!-- 拒绝原因弹窗 -->
    <n-modal
      v-model:show="showReject"
      title="填写拒绝原因"
      preset="card"
      style="width: 400px"
    >
      <n-form label-placement="top">
        <n-form-item label="原因" required>
          <n-input
            v-model:value="rejectReason"
            type="textarea"
            :rows="3"
            placeholder="请说明拒绝原因"
          />
        </n-form-item>
      </n-form>
      <n-flex justify="end" gap="12" style="margin-top: 16px">
        <n-button @click="showReject = false">取消</n-button>
        <n-button type="error" :loading="rejectLoading" @click="handleReject">确认拒绝</n-button>
      </n-flex>
    </n-modal>

    <!-- 批量拒绝原因弹窗 -->
    <n-modal
      v-model:show="showBatchReject"
      title="批量拒绝—输入原因"
      preset="card"
      style="width: 400px"
    >
      <n-form label-placement="top">
        <n-form-item label="拒绝原因" required>
          <n-input
            v-model:value="batchRejectReason"
            type="textarea"
            :rows="3"
            placeholder="该原因将应用到所有已选条目"
          />
        </n-form-item>
      </n-form>
      <n-flex justify="end" gap="12" style="margin-top: 16px">
        <n-button @click="showBatchReject = false">取消</n-button>
        <n-button type="error" :loading="batchRejectLoading" @click="handleBatchReject">确认拒绝</n-button>
      </n-flex>
    </n-modal>

    <!-- CSV 导入抽屉 -->
    <CsvImporterDrawer
      v-model:show="showCsvDrawer"
      @imported="reload"
    />

    <!-- AI 截图识别 -->
    <AiScreenshotUploader
      v-model:show="showAiUploader"
      @imported="reload"
    />
  </div>
</template>
