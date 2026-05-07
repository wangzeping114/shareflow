<script setup lang="ts">
import { h, onMounted, ref, computed } from 'vue'
import {
  NButton,
  NDataTable,
  NDescriptions,
  NDescriptionsItem,
  NFlex,
  NInput,
  NInputNumber,
  NModal,
  NCard,
  NForm,
  NFormItem,
  NSelect,
  NTag,
  NText,
  NSpin,
  useMessage,
  type DataTableColumns,
} from 'naive-ui'
import { useRoute, useRouter } from 'vue-router'
import { projectApi } from '../../../api/admin/project'
import { getTemplatePreview } from '../../../api/admin/contract'
import { useRegion } from '../../../composables/use-region'
import SlotProgressBar from '../../../components/business/SlotProgressBar.vue'
import type { ProjectDetail, ProjectSlot, ProjectStatus, SlotStatus } from '../../../types/project'

const route = useRoute()
const router = useRouter()
const message = useMessage()
const { currency } = useRegion()

const projectId = route.params.id as string
const loading = ref(true)
const project = ref<ProjectDetail | null>(null)

async function fetchDetail() {
  loading.value = true
  try {
    project.value = await projectApi.getById(projectId)
  } catch {
    message.error('加载项目详情失败')
  } finally {
    loading.value = false
  }
}

onMounted(fetchDetail)

// ── 状态操作 ──────────────────────────────
const statusLoading = ref(false)

async function changeStatus(newStatus: ProjectStatus) {
  statusLoading.value = true
  try {
    await projectApi.changeStatus(projectId, newStatus)
    message.success('状态更新成功')
    fetchDetail()
  } catch {
    message.error('状态更新失败')
  } finally {
    statusLoading.value = false
  }
}

function statusLabel(status: ProjectStatus) {
  const map: Record<ProjectStatus, string> = {
    Draft: '草稿', Active: '运营中', Paused: '已暂停', Closed: '已关闭',
  }
  return map[status] ?? status
}

function statusTagType(status: ProjectStatus) {
  const map: Record<ProjectStatus, 'default' | 'info' | 'success' | 'warning' | 'error'> = {
    Draft: 'default', Active: 'success', Paused: 'warning', Closed: 'error',
  }
  return map[status] ?? 'default'
}

function slotStatusLabel(status: SlotStatus) {
  const map: Record<SlotStatus, string> = {
    Available: '空闲', Reserved: '预留中', Occupied: '已占用', Released: '已释放',
  }
  return map[status] ?? status
}

function slotTagType(status: SlotStatus) {
  const map: Record<SlotStatus, 'default' | 'success' | 'warning' | 'error'> = {
    Available: 'default', Reserved: 'warning', Occupied: 'success', Released: 'error',
  }
  return map[status] ?? 'default'
}

// ── 槽位表格 ──────────────────────────────
const checkedRowKeys = ref<string[]>([])

const templateLabel: Record<string, string> = {
  OverseasEnglish: '境外英文',
  DomesticChinese: '境内中文',
}

const slotColumns: DataTableColumns<ProjectSlot> = [
  { type: 'selection' },
  { title: '编号', key: 'slotNumber', width: 60, render: (row) => h(NText, {}, () => `#${row.slotNumber}`) },
  {
    title: '别名',
    key: 'alias',
    width: 120,
    render: (row) => h(NText, { style: row.alias ? '' : 'opacity:0.4' }, () => row.alias ?? '未设置'),
  },
  {
    title: '持股比例',
    key: 'sharePct',
    width: 110,
    render: (row) => h(NText, {}, () => `${row.sharePct}%`),
  },
  {
    title: '状态',
    key: 'status',
    width: 90,
    render: (row) =>
      h(NTag, { type: slotTagType(row.status), size: 'small' }, () => slotStatusLabel(row.status)),
  },
  {
    title: '当前模板',
    key: 'templateType',
    width: 110,
    render: (row) => h(NTag, { size: 'small', round: true }, () => templateLabel[row.templateType] ?? row.templateType),
  },
  {
    title: '期限',
    key: 'contractMonths',
    width: 90,
    render: (row) => h(NText, {}, () => `${row.contractMonths} 个月`),
  },
  {
    title: '客户ID',
    key: 'clientUserId',
    ellipsis: { tooltip: true },
    render: (row) => h(NText, { depth: 3 }, () => row.clientUserId ?? '—'),
  },
  {
    title: '操作',
    key: 'actions',
    width: 170,
    render: (row) =>
      h(NFlex, { gap: 6 }, () => [
        h(NButton, { size: 'small', secondary: true, onClick: () => openContractPreview(row) }, () => '预览合同'),
        h(NButton, { size: 'small', secondary: true, onClick: () => openEditAlias(row) }, () => '设别名'),
        h(NButton, {
          size: 'small', secondary: true,
          disabled: row.status === 'Occupied',
          onClick: () => openEditSharePct(row)
        }, () => '修改比例'),
      ]),
  },
]

// ── 批量操作 ──────────────────────────────
const batchTemplateType = ref<string>('')
const batchDurationMonths = ref<number | undefined>(undefined)
const batchSharePct = ref<number | null>(null)
const batchApplying = ref(false)

const batchDurationOptions: { label: string; value: number | undefined }[] = [
  { label: '不修改', value: undefined },
  { label: '6 个月', value: 6 },
  { label: '1 年', value: 12 },
  { label: '2 年', value: 24 },
  { label: '3 年', value: 36 },
]

const batchTemplateOptions = [
  { label: '不修改', value: '' },
  { label: '境外英文', value: 'OverseasEnglish' },
  { label: '境内中文', value: 'DomesticChinese' },
]

async function batchApply() {
  if (!checkedRowKeys.value.length) return
  if (!batchTemplateType.value && batchDurationMonths.value === undefined && batchSharePct.value === null) {
    message.warning('请至少选择要修改的项目')
    return
  }
  batchApplying.value = true
  try {
    const payload: { slotIds: string[]; contractMonths?: number; templateType?: string; sharePct?: number } = {
      slotIds: checkedRowKeys.value,
    }
    if (batchDurationMonths.value !== undefined) payload.contractMonths = batchDurationMonths.value
    if (batchTemplateType.value) payload.templateType = batchTemplateType.value
    if (batchSharePct.value !== null && batchSharePct.value > 0) payload.sharePct = batchSharePct.value
    await projectApi.batchUpdateSlots(projectId, payload)
    message.success(`已更新 ${checkedRowKeys.value.length} 个槽位`)
    checkedRowKeys.value = []
    batchSharePct.value = null
    fetchDetail()
  } catch {
    message.error('批量更新失败')
  } finally {
    batchApplying.value = false
  }
}

// ── 编辑别名 ──────────────────────────
const showEditAlias = ref(false)
const editAliasSlot = ref<ProjectSlot | null>(null)
const editAliasValue = ref('')
const editAliasLoading = ref(false)

function openEditAlias(slot: ProjectSlot) {
  editAliasSlot.value = slot
  editAliasValue.value = slot.alias ?? ''
  showEditAlias.value = true
}

async function handleEditAlias() {
  if (!editAliasSlot.value) return
  editAliasLoading.value = true
  try {
    await projectApi.updateSlotAlias(projectId, editAliasSlot.value.id, editAliasValue.value.trim() || null)
    message.success('别名已更新')
    showEditAlias.value = false
    fetchDetail()
  } catch {
    message.error('更新失败')
  } finally {
    editAliasLoading.value = false
  }
}

// ── 单槽位修改比例 ──────────────────────────
const showEditSharePct = ref(false)
const editSharePctSlot = ref<ProjectSlot | null>(null)
const editSharePctValue = ref<number>(3)
const editSharePctLoading = ref(false)

function openEditSharePct(slot: ProjectSlot) {
  editSharePctSlot.value = slot
  editSharePctValue.value = slot.sharePct
  showEditSharePct.value = true
}

async function handleEditSharePct() {
  if (!editSharePctSlot.value || !editSharePctValue.value || editSharePctValue.value <= 0) {
    message.warning('请输入有效的持股比例')
    return
  }
  editSharePctLoading.value = true
  try {
    await projectApi.batchUpdateSlots(projectId, {
      slotIds: [editSharePctSlot.value.id],
      sharePct: editSharePctValue.value,
    })
    message.success('持股比例已更新')
    showEditSharePct.value = false
    fetchDetail()
  } catch {
    message.error('更新失败')
  } finally {
    editSharePctLoading.value = false
  }
}

// ── 合同模板预览 ──────────────────────────
const showPreviewModal = ref(false)
const previewSlot = ref<ProjectSlot | null>(null)
const previewLoading = ref(false)
const rawTemplate = ref('')

// 预览参数（从槽位自身读取）
const templateType = ref<'OverseasEnglish' | 'DomesticChinese'>('OverseasEnglish')
const templateTypeSaved = ref<string>('OverseasEnglish')
const durationMonths = ref<number>(12)
const durationSaved = ref<number>(12)
const savingSettings = ref(false)

const templateTypeOptions = [
  { label: '境外英文 (OverseasEnglish)', value: 'OverseasEnglish' },
  { label: '境内中文 (DomesticChinese)', value: 'DomesticChinese' },
]

const durationOptions = [
  { label: '6 个月', value: 6 },
  { label: '1 年（默认）', value: 12 },
  { label: '2 年', value: 24 },
  { label: '3 年', value: 36 },
]

// 是否未签约槽位（Occupied 不可修改）
const isUnsigned = computed(() => previewSlot.value?.status !== 'Occupied')
// 是否有设置变更
const settingsChanged = computed(
  () => durationMonths.value !== durationSaved.value || templateType.value !== templateTypeSaved.value
)

// 渲染后的合同正文
const renderedContract = computed(() => {
  if (!rawTemplate.value || !project.value || !previewSlot.value) return ''

  const months = durationMonths.value || 12
  const effectiveDateStr = `以实际签约日期为准，合同期限 ${months} 个月`

  return rawTemplate.value
    .replace(/\{ProjectTitle\}/g, project.value.title)
    .replace(/\{PlatformName\}/g, project.value.platformName)
    .replace(/\{InvestorName\}/g, '[投资人姓名]')
    .replace(/\{SharePermille\}/g, String(previewSlot.value.sharePct))
    .replace(/\{Currency\}/g, currency)
    .replace(/\{TotalInvestment\}/g, '[投资总额]')
    .replace(/\{EffectiveDate\}/g, effectiveDateStr)
})

async function openContractPreview(slot: ProjectSlot) {
  previewSlot.value = slot
  templateType.value = (slot.templateType as 'OverseasEnglish' | 'DomesticChinese') || 'OverseasEnglish'
  templateTypeSaved.value = templateType.value
  durationMonths.value = slot.contractMonths || 12
  durationSaved.value = durationMonths.value
  showPreviewModal.value = true
  await loadTemplate()
}

async function loadTemplate() {
  previewLoading.value = true
  rawTemplate.value = ''
  try {
    const result = await getTemplatePreview(templateType.value)
    rawTemplate.value = result?.content ?? ''
  } catch {
    message.error('加载合同模板失败')
  } finally {
    previewLoading.value = false
  }
}

async function saveSettings() {
  if (!previewSlot.value) return
  savingSettings.value = true
  try {
    const updated = await projectApi.batchUpdateSlots(projectId, {
      slotIds: [previewSlot.value.id],
      ...(durationMonths.value !== durationSaved.value ? { contractMonths: durationMonths.value } : {}),
      ...(templateType.value !== templateTypeSaved.value ? { templateType: templateType.value } : {}),
    })
    const updatedSlot = updated[0]
    // 同步更新本地列表
    const slot = project.value?.slots.find(s => s.id === previewSlot.value!.id)
    if (slot && updatedSlot) {
      slot.contractMonths = updatedSlot.contractMonths
      slot.templateType = updatedSlot.templateType
    }
    previewSlot.value = { ...previewSlot.value, contractMonths: updatedSlot.contractMonths, templateType: updatedSlot.templateType }
    durationSaved.value = updatedSlot.contractMonths
    templateTypeSaved.value = updatedSlot.templateType
    message.success('设置已保存')
  } catch {
    message.error('保存失败')
  } finally {
    savingSettings.value = false
  }
}

// ── 新增槽位弹窗 ──────────────────────────
const showAddSlot = ref(false)
const addSlotPct = ref<number>(3)
const addSlotLoading = ref(false)

async function handleAddSlot() {
  if (!addSlotPct.value || addSlotPct.value <= 0) {
    message.warning('请填写有效的持股比例')
    return
  }
  addSlotLoading.value = true
  try {
    await projectApi.addSlot(projectId, { sharePct: addSlotPct.value })
    message.success('槽位添加成功')
    showAddSlot.value = false
    fetchDetail()
  } catch {
    message.error('添加失败，请检查份数是否已满')
  } finally {
    addSlotLoading.value = false
  }
}
</script>

<template>
  <div class="project-detail-view">
    <n-flex align="center" gap="12" style="margin-bottom: 20px">
      <n-button text @click="router.back()">← 返回列表</n-button>
      <h2 style="margin: 0; font-size: 18px; font-weight: 600">项目详情</h2>
    </n-flex>

    <n-spin :show="loading">
      <template v-if="project">
        <!-- 基本信息 -->
        <div class="info-card">
          <n-flex justify="space-between" align="flex-start" style="margin-bottom: 16px">
            <div>
              <h3 style="margin: 0 0 4px">{{ project.title }}</h3>
              <n-text depth="3">{{ project.description }}</n-text>
            </div>
            <n-flex gap="8">
              <n-tag :type="statusTagType(project.status)">{{ statusLabel(project.status) }}</n-tag>
              <n-button
                v-if="project.status === 'Draft'"
                size="small"
                type="primary"
                :loading="statusLoading"
                @click="changeStatus('Active')"
              >启动运营</n-button>
              <n-button
                v-if="project.status === 'Active'"
                size="small"
                :loading="statusLoading"
                @click="changeStatus('Paused')"
              >暂停</n-button>
              <n-button
                v-if="project.status === 'Paused'"
                size="small"
                type="primary"
                :loading="statusLoading"
                @click="changeStatus('Active')"
              >恢复运营</n-button>
              <n-button
                v-if="project.status !== 'Closed'"
                size="small"
                type="error"
                secondary
                :loading="statusLoading"
                @click="changeStatus('Closed')"
              >关闭项目</n-button>
            </n-flex>
          </n-flex>

          <n-descriptions :column="4" bordered>
            <n-descriptions-item label="平台">{{ project.platformName }}</n-descriptions-item>
            <n-descriptions-item label="份数模式">
              {{ project.slotMode === 'Fixed' ? '固定份数' : '灵活份数' }}
            </n-descriptions-item>
            <n-descriptions-item label="总份数">{{ project.totalSlots }}</n-descriptions-item>
            <n-descriptions-item label="已创建时间">{{ project.createdAt?.slice(0, 10) }}</n-descriptions-item>
          </n-descriptions>

          <div style="margin-top: 16px">
            <SlotProgressBar
              :filled="project.filledSlots"
              :total="project.totalSlots"
              :reserved="project.reservedSlots"
            />
          </div>
        </div>

        <!-- 槽位列表 -->
        <div class="slots-card">
          <n-flex justify="space-between" align="center" style="margin-bottom: 12px">
            <h3 style="margin: 0">持股槽位（{{ project.slots.length }} / {{ project.totalSlots }}）</h3>
            <n-button
              size="small"
              type="primary"
              :disabled="project.availableSlots <= 0 || project.status === 'Closed'"
              @click="showAddSlot = true"
            >+ 新增槽位</n-button>
          </n-flex>

          <n-data-table
            v-model:checked-row-keys="checkedRowKeys"
            :columns="slotColumns"
            :data="project.slots"
            :row-key="(row: ProjectSlot) => row.id"
            size="small"
          />

          <!-- 批量操作 Toolbar -->
          <transition name="slide-up">
            <div v-if="checkedRowKeys.length > 0" class="batch-toolbar">
              <n-text style="font-size: 13px; color: #666">已选 {{ checkedRowKeys.length }} 个槽位</n-text>
              <n-select
                v-model:value="batchTemplateType"
                :options="batchTemplateOptions"
                placeholder="批量设置模板"
                style="width: 150px"
              />
              <n-select
                v-model:value="batchDurationMonths"
                :options="batchDurationOptions"
                placeholder="批量设置期限"
                style="width: 140px"
              />
              <n-input-number
                v-model:value="batchSharePct"
                :min="0.01"
                :max="100"
                :precision="2"
                :show-button="false"
                placeholder="批量持股比例%"
                style="width: 140px"
              />
              <n-button
                type="primary"
                size="small"
                :loading="batchApplying"
                @click="batchApply"
              >批量应用</n-button>
              <n-button size="small" @click="checkedRowKeys = []">取消选择</n-button>
            </div>
          </transition>
        </div>
      </template>
    </n-spin>

    <!-- 新增槽位弹窗 -->
    <n-modal
      v-model:show="showAddSlot"
      title="新增持股槽位"
      preset="card"
      style="width: 360px"
    >
      <n-form label-placement="top">
        <n-form-item label="持股比例（%）" required>
          <n-input-number
            v-model:value="addSlotPct"
            :min="0.01"
            :max="100"
            :precision="2"
            style="width: 100%"
            placeholder="例：3.5"
          />
        </n-form-item>
      </n-form>
      <n-flex justify="end" gap="12" style="margin-top: 16px">
        <n-button @click="showAddSlot = false">取消</n-button>
        <n-button type="primary" :loading="addSlotLoading" @click="handleAddSlot">确认添加</n-button>
      </n-flex>
    </n-modal>

    <!-- 设置槽位别名弹窗 -->
    <n-modal
      v-model:show="showEditAlias"
      title="设置槽位别名"
      preset="card"
      style="width: 360px"
    >
      <n-text depth="3" style="display: block; margin-bottom: 12px">
        槽位 #{{ editAliasSlot?.slotNumber }}　当前别名：{{ editAliasSlot?.alias ?? '未设置' }}
      </n-text>
      <n-form label-placement="top">
        <n-form-item label="别名（最长 50 字符，留空则清除）">
          <n-input
            v-model:value="editAliasValue"
            :maxlength="50"
            show-count
            clearable
            placeholder="例：A座 / 贵用-1"
          />
        </n-form-item>
      </n-form>
      <n-flex justify="end" gap="12" style="margin-top: 16px">
        <n-button @click="showEditAlias = false">取消</n-button>
        <n-button type="primary" :loading="editAliasLoading" @click="handleEditAlias">确认修改</n-button>
      </n-flex>
    </n-modal>

    <!-- 修改持股比例弹窗 -->
    <n-modal
      v-model:show="showEditSharePct"
      title="修改持股比例"
      preset="card"
      style="width: 360px"
    >
      <n-text depth="3" style="display: block; margin-bottom: 12px">
        槽位 #{{ editSharePctSlot?.slotNumber }}
        &nbsp;当前：{{ editSharePctSlot?.sharePct }}%
      </n-text>
      <n-form label-placement="top">
        <n-form-item label="新持股比例（%）" required>
          <n-input-number
            v-model:value="editSharePctValue"
            :min="0.01"
            :max="100"
            :precision="2"
            style="width: 100%"
            placeholder="例：5.0"
          />
        </n-form-item>
      </n-form>
      <n-flex justify="end" gap="12" style="margin-top: 16px">
        <n-button @click="showEditSharePct = false">取消</n-button>
        <n-button type="primary" :loading="editSharePctLoading" @click="handleEditSharePct">确认修改</n-button>
      </n-flex>
    </n-modal>

    <!-- 合同模板预览弹窗 -->
    <n-modal v-model:show="showPreviewModal" :mask-closable="false">
      <n-card
        :bordered="false"
        style="width: 800px; max-height: 92vh; display: flex; flex-direction: column"
      >
        <template #header>
          <span>合同模板预览</span>
          <n-text depth="3" style="font-size: 13px; margin-left: 8px">
            — 槽位 #{{ previewSlot?.slotNumber }}
            &nbsp;|&nbsp;{{ previewSlot?.sharePct }}%
            &nbsp;|&nbsp;{{ currency }}
          </n-text>
        </template>

        <!-- 参数配置栏 -->
        <div class="preview-config">
          <n-flex gap="16" align="center" wrap>
            <n-form-item label="模板语言" style="margin: 0">
              <n-select
                v-model:value="templateType"
                :options="templateTypeOptions"
                :disabled="!isUnsigned"
                style="width: 220px"
                @update:value="loadTemplate"
              />
            </n-form-item>
            <n-form-item label="生效日期" style="margin: 0">
              <n-text depth="3" style="font-size: 13px; line-height: 34px">以实际签约日期为准</n-text>
            </n-form-item>
            <n-form-item label="合作期限" style="margin: 0">
              <n-flex align="center" gap="8">
                <n-select
                  v-model:value="durationMonths"
                  :options="durationOptions"
                  :disabled="!isUnsigned"
                  style="width: 140px"
                />
                <n-input-number
                  v-if="!durationOptions.some(o => o.value === durationMonths)"
                  v-model:value="durationMonths"
                  :min="1"
                  :max="120"
                  :disabled="!isUnsigned"
                  style="width: 90px"
                  placeholder="月数"
                />
              </n-flex>
            </n-form-item>
          </n-flex>
          <n-flex v-if="isUnsigned" justify="flex-end" style="margin-top: 10px">
            <n-button
              type="primary"
              size="small"
              :loading="savingSettings"
              :disabled="!settingsChanged"
              @click="saveSettings"
            >保存设置</n-button>
          </n-flex>
          <n-text v-else depth="3" style="font-size: 12px; display: block; margin-top: 6px">已签约，模板与期限不可修改</n-text>
        </div>

        <!-- 合同正文 -->
        <div class="preview-body">
          <div v-if="previewLoading" class="preview-empty">加载中…</div>
          <pre v-else class="preview-text">{{ renderedContract }}</pre>
        </div>

        <template #footer>
          <n-flex justify="end">
            <n-button @click="showPreviewModal = false">关闭</n-button>
          </n-flex>
        </template>
      </n-card>
    </n-modal>
  </div>
</template>

<style scoped>
.project-detail-view { padding: 0; }
.info-card {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.06);
}
.slots-card {
  background: #fff;
  border-radius: 8px;
  padding: 20px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.06);
}
.preview-config {
  padding: 12px 16px;
  background: #f5f5f5;
  border-radius: 6px;
  margin-bottom: 12px;
}
.preview-body {
  flex: 1;
  overflow-y: auto;
  max-height: 54vh;
  background: #fafafa;
  border: 1px solid #e8e8e8;
  border-radius: 6px;
  padding: 20px 24px;
}
.preview-text {
  white-space: pre-wrap;
  font-family: 'Courier New', Courier, monospace;
  font-size: 13px;
  line-height: 1.8;
  color: #333;
  margin: 0;
}
.preview-empty {
  text-align: center;
  color: #aaa;
  padding: 60px 0;
}
.batch-toolbar {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 12px;
  padding: 10px 14px;
  background: #f0f7ff;
  border: 1px solid #b8d8ff;
  border-radius: 6px;
  flex-wrap: wrap;
}
.slide-up-enter-active, .slide-up-leave-active {
  transition: all 0.2s ease;
}
.slide-up-enter-from, .slide-up-leave-to {
  opacity: 0;
  transform: translateY(8px);
}
</style>