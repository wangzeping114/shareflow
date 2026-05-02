<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { NCard, NDataTable, NTag, NSpin, type DataTableColumns } from 'naive-ui'
import { getSalesProjects } from '../../../api/sales'
import type { SalesProjectDto } from '../../../types/sales'

const list = ref<SalesProjectDto[]>([])
const loading = ref(false)

const columns: DataTableColumns<SalesProjectDto> = [
  { title: '项目名称', key: 'title', ellipsis: true },
  { title: '平台', key: 'platformName', width: 120 },
  {
    title: '可用槽位', key: 'availableSlots', width: 100,
    render: (row) => {
      const type = row.availableSlots > 0 ? 'success' : 'error'
      return h(NTag, { type, size: 'small' }, { default: () => row.availableSlots })
    },
  },
  { title: '创建时间', key: 'createdAt', width: 130, render: (row) => new Date(row.createdAt).toLocaleDateString('zh-CN') },
]

import { h } from 'vue'

async function load() {
  loading.value = true
  try {
    list.value = await getSalesProjects()
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div>
    <h2 style="margin: 0 0 20px; font-size: 20px; font-weight: 600">项目中心</h2>

    <n-card title="募资中项目">
      <n-spin :show="loading">
        <n-data-table :columns="columns" :data="list" :bordered="false" />
      </n-spin>
    </n-card>
  </div>
</template>
