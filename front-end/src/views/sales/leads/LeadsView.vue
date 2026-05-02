<script setup lang="ts">
import { ref, h, onMounted } from 'vue'
import { NDataTable, NButton, NTag, NModal, NForm, NFormItem, NInput, NSelect, NFlex, NCard, type DataTableColumns } from 'naive-ui'
import { getLeads, createLead, updateLead } from '../../../api/sales'
import { LeadStatus, type LeadDto, type CreateLeadRequest, type UpdateLeadRequest } from '../../../types/sales'
import { useMessage } from 'naive-ui'

const message = useMessage()

const list = ref<LeadDto[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const filterStatus = ref<LeadStatus | undefined>(undefined)
const loading = ref(false)

// Modal
const showModal = ref(false)
const editingId = ref<string | null>(null)
const form = ref<CreateLeadRequest & { status: LeadStatus }>({
  name: '',
  contactInfo: '',
  email: '',
  notes: '',
  status: LeadStatus.New,
})

const statusOptions = [
  { label: '新线索', value: LeadStatus.New },
  { label: '已联系', value: LeadStatus.Contacted },
  { label: '感兴趣', value: LeadStatus.Interested },
  { label: '已转化', value: LeadStatus.Converted },
  { label: '已流失', value: LeadStatus.Lost },
]

const statusTagType: Record<LeadStatus, 'default' | 'info' | 'success' | 'warning' | 'error'> = {
  [LeadStatus.New]: 'info',
  [LeadStatus.Contacted]: 'default',
  [LeadStatus.Interested]: 'warning',
  [LeadStatus.Converted]: 'success',
  [LeadStatus.Lost]: 'error',
}

const statusLabel: Record<LeadStatus, string> = {
  [LeadStatus.New]: '新线索',
  [LeadStatus.Contacted]: '已联系',
  [LeadStatus.Interested]: '感兴趣',
  [LeadStatus.Converted]: '已转化',
  [LeadStatus.Lost]: '已流失',
}

const columns: DataTableColumns<LeadDto> = [
  { title: '姓名', key: 'name', width: 120 },
  { title: '联系方式', key: 'contactInfo', width: 160 },
  { title: '邮箱', key: 'email', width: 200, render: (row) => row.email ?? '-' },
  {
    title: '状态', key: 'status', width: 100,
    render: (row) => h(NTag, { type: statusTagType[row.status], size: 'small' }, { default: () => statusLabel[row.status] }),
  },
  { title: '备注', key: 'notes', ellipsis: true, render: (row) => row.notes ?? '-' },
  { title: '创建时间', key: 'createdAt', width: 130, render: (row) => new Date(row.createdAt).toLocaleDateString('zh-CN') },
  {
    title: '操作', key: '_actions', width: 80,
    render: (row) => h(NButton, { size: 'small', onClick: () => openEdit(row) }, { default: () => '编辑' }),
  },
]

async function fetchList() {
  loading.value = true
  try {
    const result = await getLeads({ status: filterStatus.value, page: page.value, pageSize: pageSize.value })
    list.value = result.items
    total.value = result.total
  } finally {
    loading.value = false
  }
}

function openCreate() {
  editingId.value = null
  form.value = { name: '', contactInfo: '', email: '', notes: '', status: LeadStatus.New }
  showModal.value = true
}

function openEdit(row: LeadDto) {
  editingId.value = row.id
  form.value = { name: row.name, contactInfo: row.contactInfo, email: row.email ?? '', notes: row.notes ?? '', status: row.status }
  showModal.value = true
}

async function handleSave() {
  if (editingId.value) {
    const req: UpdateLeadRequest = { ...form.value }
    await updateLead(editingId.value, req)
    message.success('更新成功')
  } else {
    const req: CreateLeadRequest = { name: form.value.name, contactInfo: form.value.contactInfo, email: form.value.email, notes: form.value.notes }
    await createLead(req)
    message.success('创建成功')
  }
  showModal.value = false
  fetchList()
}

onMounted(fetchList)
</script>

<template>
  <div>
    <h2 style="margin: 0 0 20px; font-size: 20px; font-weight: 600">客户管理</h2>

    <n-card>
      <n-flex style="margin-bottom: 16px" align="center">
        <n-select
          v-model:value="filterStatus"
          :options="statusOptions"
          clearable
          placeholder="状态筛选"
          style="width: 140px"
          @update:value="fetchList"
        />
        <n-button type="primary" @click="openCreate">新建客户</n-button>
      </n-flex>

      <n-data-table
        :columns="columns"
        :data="list"
        :loading="loading"
        :bordered="false"
        :pagination="{ page, pageSize, itemCount: total, onChange: (p) => { page = p; fetchList() } }"
      />
    </n-card>

    <n-modal v-model:show="showModal" preset="card" :title="editingId ? '编辑客户' : '新建客户'" style="width: 480px">
      <n-form :model="form" label-placement="left" label-width="90">
        <n-form-item label="姓名" path="name">
          <n-input v-model:value="form.name" placeholder="请输入姓名" />
        </n-form-item>
        <n-form-item label="联系方式" path="contactInfo">
          <n-input v-model:value="form.contactInfo" placeholder="微信 / 电话 / WhatsApp" />
        </n-form-item>
        <n-form-item label="邮箱" path="email">
          <n-input v-model:value="form.email" placeholder="可选" />
        </n-form-item>
        <n-form-item v-if="editingId" label="状态" path="status">
          <n-select v-model:value="form.status" :options="statusOptions" />
        </n-form-item>
        <n-form-item label="备注" path="notes">
          <n-input v-model:value="form.notes" type="textarea" :rows="3" placeholder="可选" />
        </n-form-item>
      </n-form>
      <template #footer>
        <n-flex justify="end">
          <n-button @click="showModal = false">取消</n-button>
          <n-button type="primary" @click="handleSave">保存</n-button>
        </n-flex>
      </template>
    </n-modal>
  </div>
</template>
