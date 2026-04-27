<script setup lang="ts">
import { h, onMounted, ref } from 'vue'
import {
  NButton,
  NDataTable,
  NDescriptions,
  NDescriptionsItem,
  NFlex,
  NInputNumber,
  NModal,
  NForm,
  NFormItem,
  NTag,
  NText,
  NSpin,
  useMessage,
  type DataTableColumns,
} from 'naive-ui'
import { useRoute, useRouter } from 'vue-router'
import { projectApi } from '../../../api/admin/project'
import SlotProgressBar from '../../../components/business/SlotProgressBar.vue'
import type { ProjectDetail, ProjectSlot, ProjectStatus, SlotStatus } from '../../../types/project'

const route = useRoute()
const router = useRouter()
const message = useMessage()

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
const slotColumns: DataTableColumns<ProjectSlot> = [
  { title: '编号', key: 'index', width: 60, render: (_, idx) => h(NText, {}, () => String(idx + 1)) },
  {
    title: '持股比例',
    key: 'sharePct',
    width: 120,
    render: (row) => h(NText, {}, () => `${row.sharePct}%`),
  },
  {
    title: '状态',
    key: 'status',
    width: 100,
    render: (row) =>
      h(NTag, { type: slotTagType(row.status), size: 'small' }, () => slotStatusLabel(row.status)),
  },
  { title: '客户ID', key: 'clientUserId', ellipsis: { tooltip: true },
    render: (row) => h(NText, { depth: 3 }, () => row.clientUserId ?? '—'),
  },
]

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
            :columns="slotColumns"
            :data="project.slots"
            :row-key="(row: ProjectSlot) => row.id"
            size="small"
          />
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
</style>
