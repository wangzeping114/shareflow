<script setup lang="ts">
import { h, ref, computed } from 'vue'
import {
  NButton, NDataTable, NFlex, NModal, NForm, NFormItem,
  NInput, NInputNumber, NSelect, NTag, NText, NAlert,
  NRadioGroup, NRadio, NSpace, NDivider,
  useMessage, useDialog, type DataTableColumns,
} from 'naive-ui'
import { useRouter } from 'vue-router'
import { projectApi } from '../../../api/admin/project'
import SlotProgressBar from '../../../components/business/SlotProgressBar.vue'
import type { Project, ProjectStatus, ProjectSlotMode, CreateProjectRequest, UpdateProjectRequest } from '../../../types/project'
import { usePagedList } from '../../../composables/use-paged-list'

const router = useRouter()
const message = useMessage()
const dialog = useDialog()

const { list, loading, filter, search, reload, paginationProps } = usePagedList<
  Project, { status?: ProjectStatus }
>({
  fetcher: (params) =>
    projectApi.list(params).catch(() => {
      message.error('加载项目列表失败')
      return { items: [], total: 0, page: params.page, pageSize: params.pageSize }
    }),
  initialFilter: { status: undefined },
})

const ALL_PLATFORMS = ['YouTube', 'TikTok', 'Instagram', 'Kwai', 'Xiaohongshu', 'Douyin', 'Kuaishou', 'BrandDeal']
const platformOptions = ALL_PLATFORMS.map(p => ({ label: p, value: p }))
function platformTags(platformName: string): string[] {
  return platformName ? platformName.split(',').map(s => s.trim()).filter(Boolean) : []
}

const statusOptions = [
  { label: '全部', value: undefined },
  { label: '草稿', value: 'Draft' as ProjectStatus },
  { label: '运营中', value: 'Active' as ProjectStatus },
  { label: '已暂停', value: 'Paused' as ProjectStatus },
  { label: '已关闭', value: 'Closed' as ProjectStatus },
]
function onStatusChange(val: ProjectStatus | undefined) { filter.value.status = val; search() }

function statusTagType(status: ProjectStatus): 'default' | 'info' | 'success' | 'warning' | 'error' {
  const map: Record<ProjectStatus, 'default' | 'info' | 'success' | 'warning' | 'error'> = {
    Draft: 'default', Active: 'success', Paused: 'warning', Closed: 'error',
  }
  return map[status] ?? 'default'
}
function statusLabel(status: ProjectStatus): string {
  return ({ Draft: '草稿', Active: '运营中', Paused: '已暂停', Closed: '已关闭' } as Record<ProjectStatus, string>)[status] ?? status
}

const columns: DataTableColumns<Project> = [
  { title: '项目名称', key: 'title', ellipsis: { tooltip: true } },
  {
    title: '平台', key: 'platformName', width: 160,
    render: (row) => h(NFlex, { gap: 4, wrap: true }, () =>
      platformTags(row.platformName).map(p => h(NTag, { size: 'small', bordered: false }, () => p))
    ),
  },
  {
    title: '状态', key: 'status', width: 90,
    render: (row) => h(NTag, { type: statusTagType(row.status), size: 'small' }, () => statusLabel(row.status)),
  },
  {
    title: '份数模式', key: 'slotMode', width: 130,
    render: (row) => {
      if (row.slotMode === 'Fixed') {
        return h(NFlex, { vertical: true, gap: 2 }, () => [
          h(NTag, { type: 'info', size: 'small' }, () => '固定份数'),
          h(NText, { depth: 3, style: 'font-size:12px' }, () =>
            row.totalSlots > 0 ? `每份 ${(100 / row.totalSlots).toFixed(2)}%` : '均分'),
        ])
      }
      return h(NFlex, { vertical: true, gap: 2 }, () => [
        h(NTag, { type: 'warning', size: 'small' }, () => '灵活份数'),
        h(NText, { depth: 3, style: 'font-size:12px' }, () => '每份独立定义'),
      ])
    },
  },
  {
    title: '总投资（内部）', key: 'totalInvestment', width: 130,
    render: (row) => row.totalInvestment != null
      ? h(NText, { strong: true, style: 'color:#3b82f6' }, () => `¥${row.totalInvestment!.toLocaleString()}`)
      : h(NText, { depth: 3 }, () => '未填写'),
  },
  {
    title: '名额进度', key: 'slots', width: 200,
    render: (row) => h(SlotProgressBar, { filled: row.filledSlots, total: row.totalSlots, reserved: row.reservedSlots }),
  },
  {
    title: '操作', key: 'actions', width: 210,
    render: (row) => h(NFlex, { gap: 6, style: 'flex-wrap:nowrap' }, () => [
      h(NButton, { size: 'small', onClick: () => openEdit(row) }, () => '编辑'),
      h(NButton, { size: 'small', onClick: () => goDetail(row.id) }, () => '详情'),
      row.status !== 'Closed'
        ? h(NButton, { size: 'small', type: 'error', ghost: true, onClick: () => confirmClose(row) }, () => '关闭')
        : h(NButton, { size: 'small', type: 'warning', ghost: true, onClick: () => confirmReopen(row) }, () => '重启'),
    ]),
  },
]

function goDetail(id: string) { router.push(`/admin/projects/${id}`) }

function confirmClose(row: Project) {
  const activeCount = row.filledSlots + row.reservedSlots
  dialog.warning({
    title: '⚠️ 确认关闭项目？',
    content: () => h('div', { style: 'line-height:1.8' }, [
      activeCount > 0
        ? h(NAlert, { type: 'error', style: 'margin-bottom:12px' }, {
            default: () => `当前有 ${row.filledSlots} 位客户正持有名额，${row.reservedSlots} 份处于预留状态。关闭后将停止分红计算，请确认已妥善处理。`
          })
        : h(NAlert, { type: 'info', style: 'margin-bottom:12px' }, {
            default: () => '当前暂无客户参与，可以安全关闭。'
          }),
      h('p', {}, '关闭后项目将停止运营，确认关闭吗？（关闭后可通过「重启」恢复到草稿状态）'),
    ]),
    positiveText: '确认关闭',
    negativeText: '取消',
    positiveButtonProps: { type: 'error' },
    onPositiveClick: async () => {
      try {
        await projectApi.changeStatus(row.id, 'Closed')
        message.success('项目已关闭')
        reload()
      } catch { message.error('操作失败，请重试') }
    },
  })
}

function confirmReopen(row: Project) {
  dialog.warning({
    title: '🔄 重启项目？',
    content: `项目「${row.title}」将重置为「草稿」状态，需重新配置后才能激活运营。确认重启吗？`,
    positiveText: '确认重启',
    negativeText: '取消',
    onPositiveClick: async () => {
      try {
        await projectApi.changeStatus(row.id, 'Draft')
        message.success('项目已重启，当前状态：草稿')
        reload()
      } catch { message.error('操作失败，请重试') }
    },
  })
}

const showEdit = ref(false)
const editLoading = ref(false)
const editingId = ref<string>('')
const editPlatforms = ref<string[]>([])
const editForm = ref<Omit<UpdateProjectRequest, 'platformName'>>({
  title: '', description: '', totalSlots: 10, totalInvestment: undefined,
})

function openEdit(row: Project) {
  editingId.value = row.id
  editPlatforms.value = platformTags(row.platformName)
  editForm.value = {
    title: row.title,
    description: row.description ?? '',
    totalSlots: row.totalSlots,
    totalInvestment: row.totalInvestment,
  }
  showEdit.value = true
}

async function handleEdit() {
  if (!editForm.value.title || editPlatforms.value.length === 0) {
    message.warning('请填写项目名称并选择至少一个平台'); return
  }
  editLoading.value = true
  try {
    await projectApi.update(editingId.value, {
      ...editForm.value, platformName: editPlatforms.value.join(','),
    })
    message.success('项目更新成功')
    showEdit.value = false
    reload()
  } catch { message.error('更新失败，请重试') } finally { editLoading.value = false }
}

const showCreate = ref(false)
const createLoading = ref(false)
const createPlatforms = ref<string[]>([])
const createSlotMode = ref<ProjectSlotMode>('Fixed')
const createForm = ref<Omit<CreateProjectRequest, 'platformName' | 'slotMode'>>({
  title: '', description: '', totalSlots: 10, totalInvestment: undefined,
})

const autoSharePct = computed(() => {
  if (createSlotMode.value !== 'Fixed' || createForm.value.totalSlots <= 0) return null
  return (100 / createForm.value.totalSlots).toFixed(4)
})

function openCreate() {
  createPlatforms.value = []; createSlotMode.value = 'Fixed'
  createForm.value = { title: '', description: '', totalSlots: 10, totalInvestment: undefined }
  showCreate.value = true
}

async function handleCreate() {
  if (!createForm.value.title || createPlatforms.value.length === 0) {
    message.warning('请填写项目名称并选择至少一个平台'); return
  }
  createLoading.value = true
  try {
    await projectApi.create({
      ...createForm.value,
      platformName: createPlatforms.value.join(','),
      slotMode: createSlotMode.value,
    })
    message.success('项目创建成功')
    showCreate.value = false
    reload()
  } catch { message.error('创建失败，请重试') } finally { createLoading.value = false }
}
</script>

<template>
  <div class="project-list-view">
    <div class="view-header">
      <h2>视频项目管理</h2>
      <n-flex align="center" gap="12">
        <n-select
          :value="filter.status"
          :options="statusOptions"
          placeholder="按状态筛选"
          style="width:140px"
          clearable
          @update:value="onStatusChange"
        />
        <n-button type="primary" @click="openCreate">+ 新建项目</n-button>
      </n-flex>
    </div>

    <n-data-table
      :columns="columns"
      :data="list"
      :loading="loading"
      :pagination="paginationProps"
      :row-key="(row: Project) => row.id"
      striped
    />

    <!-- 新建项目 -->
    <n-modal v-model:show="showCreate" title="新建视频项目" preset="card" style="width:540px" :mask-closable="false">
      <n-form label-placement="top">
        <n-form-item label="项目名称" required>
          <n-input v-model:value="createForm.title" placeholder="例：仙侠系列 - TikTok 01" />
        </n-form-item>
        <n-form-item label="平台（可多选）" required>
          <n-select v-model:value="createPlatforms" :options="platformOptions" placeholder="选择平台" multiple />
        </n-form-item>
        <n-form-item label="描述">
          <n-input v-model:value="createForm.description" type="textarea" :rows="3" placeholder="项目简介（可选）" />
        </n-form-item>
        <n-form-item label="份数模式">
          <n-radio-group v-model:value="createSlotMode">
            <n-space>
              <n-radio value="Fixed">
                固定份数
                <n-text depth="3" style="margin-left:4px;font-size:12px">（每份比例相同，自动均分）</n-text>
              </n-radio>
              <n-radio value="Flexible">
                灵活份数
                <n-text depth="3" style="margin-left:4px;font-size:12px">（每份比例独立设定）</n-text>
              </n-radio>
            </n-space>
          </n-radio-group>
        </n-form-item>
        <n-form-item label="总份数" required>
          <n-flex vertical style="width:100%">
            <n-input-number v-model:value="createForm.totalSlots" :min="1" :max="1000" style="width:100%" />
            <n-text v-if="createSlotMode === 'Fixed' && autoSharePct" depth="3" style="font-size:12px;margin-top:4px">
              固定模式：每份约 {{ autoSharePct }}% 持股比例
            </n-text>
          </n-flex>
        </n-form-item>
        <n-divider style="margin:8px 0" />
        <n-form-item label="项目总投资（内部）">
          <n-input-number
            v-model:value="createForm.totalInvestment"
            :min="0"
            :precision="2"
            placeholder="仅内部记录，不对客户展示（可选）"
            style="width:100%"
          >
            <template #prefix>¥</template>
          </n-input-number>
        </n-form-item>
      </n-form>
      <n-flex justify="end" gap="12" style="margin-top:16px">
        <n-button @click="showCreate = false">取消</n-button>
        <n-button type="primary" :loading="createLoading" @click="handleCreate">确认创建</n-button>
      </n-flex>
    </n-modal>

    <!-- 编辑项目 -->
    <n-modal v-model:show="showEdit" title="编辑视频项目" preset="card" style="width:540px" :mask-closable="false">
      <n-form label-placement="top">
        <n-form-item label="项目名称" required>
          <n-input v-model:value="editForm.title" placeholder="项目名称" />
        </n-form-item>
        <n-form-item label="平台（可多选）" required>
          <n-select v-model:value="editPlatforms" :options="platformOptions" placeholder="选择平台" multiple />
        </n-form-item>
        <n-form-item label="描述">
          <n-input v-model:value="editForm.description" type="textarea" :rows="3" placeholder="项目简介" />
        </n-form-item>
        <n-form-item label="总份数" required>
          <n-input-number v-model:value="editForm.totalSlots" :min="1" :max="1000" style="width:100%" />
        </n-form-item>
        <n-divider style="margin:8px 0" />
        <n-form-item label="项目总投资（内部）">
          <n-input-number
            v-model:value="editForm.totalInvestment"
            :min="0"
            :precision="2"
            placeholder="仅内部记录，不对客户展示（可选）"
            style="width:100%"
          >
            <template #prefix>¥</template>
          </n-input-number>
        </n-form-item>
      </n-form>
      <n-flex justify="end" gap="12" style="margin-top:16px">
        <n-button @click="showEdit = false">取消</n-button>
        <n-button type="primary" :loading="editLoading" @click="handleEdit">保存修改</n-button>
      </n-flex>
    </n-modal>
  </div>
</template>

<style scoped>
.project-list-view { padding: 0; }
.view-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
.view-header h2 { margin: 0; font-size: 18px; font-weight: 600; }
</style>