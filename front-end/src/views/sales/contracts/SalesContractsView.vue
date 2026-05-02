<script setup lang="ts">
import { ref, onMounted, computed, h } from 'vue'
import {
  NCard,
  NForm,
  NFormItem,
  NInput,
  NSelect,
  NButton,
  NFlex,
  NAlert,
  NText,
  NDataTable,
  NTag,
  NProgress,
  NModal,
  NImage,
  NDivider,
  useMessage,
  type DataTableColumns,
} from 'naive-ui'
import { getSalesProjects, getSalesClients, getSalesContracts, getSalesContractDetail, initiateContract } from '../../../api/sales'
import type {
  SalesProjectDto,
  SalesClientDto,
  SalesContractDto,
  SalesContractDetailDto,
  InitiateContractResult,
  SalesContractStatus,
} from '../../../types/sales'

const message = useMessage()
const projects = ref<SalesProjectDto[]>([])
const clients = ref<SalesClientDto[]>([])
const contracts = ref<SalesContractDto[]>([])
const loading = ref(false)
const submitting = ref(false)
const result = ref<InitiateContractResult | null>(null)
const copied = ref(false)
const showInitiateModal = ref(false)
const filterStatus = ref<SalesContractStatus | undefined>(undefined)
const keyword = ref('')
const showDetailModal = ref(false)
const detailLoading = ref(false)
const contractDetail = ref<SalesContractDetailDto | null>(null)

const form = ref({
  projectId: '',
  slotId: '',
  investorUserId: '',
})

const projectOptions = computed(() =>
  projects.value.map(p => ({
    label: `${p.title} (${p.platformName}) — 可用:${p.availableSlots}`,
    value: p.id,
  })),
)

const selectedProject = computed(() => projects.value.find(p => p.id === form.value.projectId) ?? null)

const slotOptions = computed(() =>
  (selectedProject.value?.slots ?? []).map(s => ({
    label: `槽位 ${s.id.slice(0, 8)}...（${(s.sharePermille / 10).toFixed(2)}%）`,
    value: s.id,
  })),
)

const clientOptions = computed(() =>
  clients.value.map(c => ({
    label: c.email ? `${c.name}（${c.email}）` : c.name,
    value: c.id,
  })),
)

async function load() {
  loading.value = true
  try {
    const [projectResult, clientResult, contractResult] = await Promise.all([
      getSalesProjects(),
      getSalesClients(),
      getSalesContracts(),
    ])
    projects.value = projectResult
    clients.value = clientResult
    contracts.value = contractResult
  } finally {
    loading.value = false
  }
}

function handleProjectChange() {
  form.value.slotId = ''
}

async function handleSubmit() {
  submitting.value = true
  try {
    result.value = await initiateContract({
      projectId: form.value.projectId,
      slotId: form.value.slotId,
      investorUserId: form.value.investorUserId,
    })
    message.success('合同已创建，签约链接已生成')
    await refreshContracts()
  } catch {
    message.error('创建失败，请检查输入')
  } finally {
    submitting.value = false
  }
}

async function refreshContracts() {
  contracts.value = await getSalesContracts()
}

async function openDetail(row: SalesContractDto) {
  showDetailModal.value = true
  detailLoading.value = true
  contractDetail.value = null
  try {
    contractDetail.value = await getSalesContractDetail(row.id)
  } catch {
    message.error('加载合同详情失败')
    showDetailModal.value = false
  } finally {
    detailLoading.value = false
  }
}

function openInitiateModal() {
  result.value = null
  copied.value = false
  form.value = {
    projectId: '',
    slotId: '',
    investorUserId: '',
  }
  showInitiateModal.value = true
}

function statusLabel(status: SalesContractStatus) {
  const map: Record<SalesContractStatus, string> = {
    Draft: '草稿',
    Sent: '已发送',
    Signed: '已签署',
    Executed: '已执行',
    Expired: '已过期',
    PendingRenew: '待续签',
    Renewing: '续签中',
    Superseded: '已替换',
  }
  return map[status]
}

function statusType(status: SalesContractStatus): 'default' | 'success' | 'warning' | 'error' | 'info' {
  if (status === 'Executed' || status === 'Signed') return 'success'
  if (status === 'Sent' || status === 'Draft' || status === 'Renewing' || status === 'PendingRenew') return 'warning'
  if (status === 'Expired') return 'error'
  return 'default'
}

function progressValue(status: SalesContractStatus) {
  const map: Record<SalesContractStatus, number> = {
    Draft: 15,
    Sent: 45,
    Signed: 75,
    Executed: 100,
    Expired: 100,
    PendingRenew: 60,
    Renewing: 80,
    Superseded: 100,
  }
  return map[status]
}

const contractStatusOptions = computed(() => ([
  { label: '草稿', value: 'Draft' },
  { label: '已发送', value: 'Sent' },
  { label: '已签署', value: 'Signed' },
  { label: '已执行', value: 'Executed' },
  { label: '已过期', value: 'Expired' },
  { label: '待续签', value: 'PendingRenew' },
  { label: '续签中', value: 'Renewing' },
  { label: '已替换', value: 'Superseded' },
]))

const filteredContracts = computed(() => {
  const trimmedKeyword = keyword.value.trim().toLowerCase()

  return contracts.value.filter(contract => {
    const statusMatched = !filterStatus.value || contract.status === filterStatus.value
    if (!statusMatched) {
      return false
    }

    if (!trimmedKeyword) {
      return true
    }

    return contract.projectTitle.toLowerCase().includes(trimmedKeyword)
      || contract.slotId.toLowerCase().includes(trimmedKeyword)
      || contract.clientName.toLowerCase().includes(trimmedKeyword)
      || contract.id.toLowerCase().includes(trimmedKeyword)
  })
})

function resetFilter() {
  filterStatus.value = undefined
  keyword.value = ''
}

function filterPendingSign() {
  filterStatus.value = 'Sent'
}

const columns: DataTableColumns<SalesContractDto> = [
  {
    title: '合同编号',
    key: 'id',
    render: row => row.id.slice(0, 8),
  },
  {
    title: '所属项目',
    key: 'projectTitle',
  },
  {
    title: '槽位',
    key: 'slotId',
    render: row => `槽位 ${row.slotId.slice(0, 8)}...`,
  },
  {
    title: '持股比例',
    key: 'sharePermille',
    render: row => `${(row.sharePermille / 10).toFixed(2)}%`,
  },
  {
    title: '客户',
    key: 'clientName',
  },
  {
    title: '当前状态',
    key: 'status',
    render: row => h(NTag, { type: statusType(row.status), size: 'small' }, { default: () => statusLabel(row.status) }),
  },
  {
    title: '合同进度',
    key: 'progress',
    render: row => h(NProgress, { percentage: progressValue(row.status), indicatorPlacement: 'inside' }),
  },
  {
    title: '操作',
    key: 'actions',
    render: row => h(NButton, {
      size: 'small',
      type: 'primary',
      text: true,
      onClick: () => openDetail(row),
    }, { default: () => '查看详情' }),
  },
  {
    title: '创建时间',
    key: 'createdAt',
    render: row => new Date(row.createdAt).toLocaleString('zh-CN'),
  },
]

function copyLink() {
  if (result.value?.signUrl) {
    navigator.clipboard.writeText(result.value.signUrl)
    copied.value = true
    setTimeout(() => { copied.value = false }, 2000)
  }
}

onMounted(load)
</script>

<template>
  <div>
    <n-flex justify="space-between" align="center" style="margin-bottom: 16px">
      <h2 style="margin: 0; font-size: 20px; font-weight: 600">合同管理</h2>
      <n-button type="primary" @click="openInitiateModal">发起合同</n-button>
    </n-flex>

    <n-card>
      <n-flex style="margin-bottom: 12px" :wrap="false" align="center">
        <n-select
          v-model:value="filterStatus"
          :options="contractStatusOptions"
          clearable
          placeholder="状态筛选"
          style="width: 180px"
        />
        <n-input
          v-model:value="keyword"
          placeholder="搜索项目/槽位/客户/合同编号"
          clearable
        />
        <n-button type="warning" secondary @click="filterPendingSign">仅看待签署</n-button>
        <n-button quaternary @click="resetFilter">重置</n-button>
      </n-flex>

      <n-data-table
        :columns="columns"
        :data="filteredContracts"
        :loading="loading"
        :bordered="false"
      />
    </n-card>

    <n-modal v-model:show="showInitiateModal" preset="card" title="发起合同" style="width: 620px">
      <n-form :model="form" label-placement="left" label-width="110">
        <n-form-item label="选择项目" path="projectId">
          <n-select
            v-model:value="form.projectId"
            :options="projectOptions"
            :loading="loading"
            placeholder="选择募资中的项目"
            @update:value="handleProjectChange"
          />
        </n-form-item>

        <n-form-item label="选择槽位" path="slotId">
          <n-select
            v-model:value="form.slotId"
            :options="slotOptions"
            :disabled="!form.projectId"
            placeholder="请先选择项目"
          />
        </n-form-item>

        <n-form-item label="客户姓名" path="investorUserId">
          <n-select
            v-model:value="form.investorUserId"
            :options="clientOptions"
            filterable
            clearable
            placeholder="请选择客户"
          />
        </n-form-item>
      </n-form>

      <n-button
        type="primary"
        :loading="submitting"
        :disabled="!form.projectId || !form.slotId || !form.investorUserId"
        @click="handleSubmit"
        block
      >
        生成签约链接
      </n-button>

      <n-card v-if="result" title="签约链接" style="margin-top: 16px">
        <n-alert type="success" :show-icon="false" style="margin-bottom: 16px">
          合同已创建，请将下方链接发送给客户（微信/WhatsApp）
        </n-alert>

        <n-input :value="result.signUrl" readonly style="margin-bottom: 12px" />

        <n-flex>
          <n-button type="primary" @click="copyLink">
            {{ copied ? '已复制 ✓' : '复制链接' }}
          </n-button>
          <n-text depth="3" style="font-size: 12px; align-self: center">
            有效期至：{{ new Date(result.expiresAt).toLocaleString('zh-CN') }}
          </n-text>
        </n-flex>

        <template v-if="result.clientUsername && result.tempPassword">
          <n-divider />
          <n-alert type="warning" title="客户账号已创建，请立即告知客户" style="margin-bottom: 12px">
            系统已为该客户自动生成登录账号，此临时密码仅显示一次，请通过安全渠道告知客户。
          </n-alert>
          <n-flex vertical :size="8">
            <n-flex align="center">
              <n-text style="width: 80px; color: #666">用户名</n-text>
              <n-input :value="result.clientUsername" readonly style="flex: 1" />
            </n-flex>
            <n-flex align="center">
              <n-text style="width: 80px; color: #666">临时密码</n-text>
              <n-input :value="result.tempPassword" readonly style="flex: 1" type="text" />
            </n-flex>
          </n-flex>
        </template>
      </n-card>
    </n-modal>

    <!-- 合同详情 Modal -->
    <n-modal v-model:show="showDetailModal" preset="card" title="合同详情" style="width: 700px">
      <div v-if="detailLoading" style="text-align: center; padding: 40px">
        <n-text>加载中...</n-text>
      </div>
      <template v-else-if="contractDetail">
        <n-flex vertical :size="16">
          <n-flex>
            <n-text depth="3">项目：</n-text><n-text>{{ contractDetail.projectTitle }}</n-text>
            <n-text depth="3" style="margin-left: 24px">客户：</n-text><n-text>{{ contractDetail.clientName }}</n-text>
            <n-text depth="3" style="margin-left: 24px">持股比例：</n-text>
            <n-text>{{ (contractDetail.sharePermille / 10).toFixed(2) }}%</n-text>
          </n-flex>

          <div v-if="contractDetail.contractSnapshot">
            <n-text depth="3" style="display: block; margin-bottom: 8px">合同内容快照</n-text>
            <div style="background: #f9f9f9; border: 1px solid #eee; padding: 16px; border-radius: 6px; white-space: pre-wrap; font-size: 13px; max-height: 300px; overflow-y: auto;">
              {{ contractDetail.contractSnapshot }}
            </div>
          </div>
          <div v-else>
            <n-text depth="3">（暂无合同快照）</n-text>
          </div>

          <div v-if="contractDetail.signatureDataUrl">
            <n-text depth="3" style="display: block; margin-bottom: 8px">客户签名</n-text>
            <div style="border: 1px solid #eee; display: inline-block; padding: 8px; border-radius: 6px; background: #fff">
              <n-image
                :src="contractDetail.signatureDataUrl"
                style="max-width: 400px; display: block"
                preview-disabled
              />
            </div>
            <n-text depth="3" style="display: block; margin-top: 4px; font-size: 12px">
              签署时间：{{ contractDetail.signedAt ? new Date(contractDetail.signedAt).toLocaleString('zh-CN') : '-' }}
            </n-text>
          </div>
          <div v-else>
            <n-tag type="warning" size="small">客户尚未签署</n-tag>
          </div>

          <template v-if="contractDetail.clientUsername">
            <n-divider />
            <n-text depth="3" style="display: block; margin-bottom: 8px">客户登录账号</n-text>
            <n-flex vertical :size="8">
              <n-flex align="center">
                <n-text style="width: 80px; color: #666">用户名</n-text>
                <n-input :value="contractDetail.clientUsername" readonly style="flex: 1; font-family: monospace" />
              </n-flex>
              <n-flex align="center">
                <n-text style="width: 80px; color: #666">初始密码</n-text>
                <n-input
                  :value="contractDetail.clientInitialPassword ?? '（客户已修改密码）'"
                  readonly
                  style="flex: 1; font-family: monospace"
                />
              </n-flex>
            </n-flex>
          </template>
        </n-flex>
      </template>
    </n-modal>
  </div>
</template>
