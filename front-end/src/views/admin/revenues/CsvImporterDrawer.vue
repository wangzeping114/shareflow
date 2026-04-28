<script setup lang="ts">
import { ref, watch } from 'vue'
import {
  NDrawer, NDrawerContent, NForm, NFormItem, NSelect, NButton,
  NFlex, NUpload, NAlert, NList, NListItem, NText,
  useMessage, type UploadFileInfo,
} from 'naive-ui'
import { importRevenueCsv } from '../../../api/admin/revenue'
import { useProjectSelect, getProjectPlatformOptions, getProjectPlatformValues } from '../../../composables/use-project-select'

const props = defineProps<{ show: boolean }>()
const emit = defineEmits<{
  'update:show': [value: boolean]
  imported: []
}>()

const message = useMessage()
const loading = ref(false)
const selectedProjectId = ref<string | null>(null)
const platforms = ref<string[]>([])
const fileList = ref<UploadFileInfo[]>([])
const importResult = ref<{ imported: number; failed: number; errors: string[] } | null>(null)

const { projectOptions, projectsLoading } = useProjectSelect()
const csvPlatformOptions = ref<{ label: string; value: string }[]>([])

// 选中项目后仅展示该项目的平台并自动全选
watch(selectedProjectId, (id) => {
  if (!id) { csvPlatformOptions.value = []; platforms.value = []; return }
  const opt = projectOptions.value.find((p) => p.value === id)
  if (opt) {
    csvPlatformOptions.value = getProjectPlatformOptions(opt.platformName)
    platforms.value = getProjectPlatformValues(opt.platformName)
  }
})

function handleClose() {
  selectedProjectId.value = null
  platforms.value = []
  csvPlatformOptions.value = []
  fileList.value = []
  importResult.value = null
  emit('update:show', false)
}

function handleFileChange(data: { fileList: UploadFileInfo[] }) {
  fileList.value = data.fileList.slice(-1)
}

async function handleImport() {
  if (!selectedProjectId.value) {
    message.warning('请选择项目')
    return
  }
  if (!platforms.value.length) {
    message.warning('请选择平台')
    return
  }
  const file = fileList.value[0]?.file
  if (!file) {
    message.warning('请选择 CSV 文件')
    return
  }

  loading.value = true
  importResult.value = null
  try {
    const results = await Promise.all(
      platforms.value.map((p) => importRevenueCsv(selectedProjectId.value!, p, file))
    )
    const total = { imported: 0, failed: 0, errors: [] as string[] }
    for (const r of results) {
      if (r) {
        total.imported += r.imported
        total.failed += r.failed
        total.errors.push(...r.errors)
      }
    }
    importResult.value = total
    if (total.imported > 0) {
      message.success(`成功导入 ${total.imported} 条`)
      emit('imported')
    }
  } catch {
    message.error('导入失败，请检查文件格式')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <n-drawer :show="props.show" width="440" placement="right" @update:show="emit('update:show', $event)">
    <n-drawer-content title="CSV 批量导入" closable>
      <n-form label-placement="top">
        <n-form-item label="项目" required>
          <n-select
            v-model:value="selectedProjectId"
            :options="projectOptions"
            :loading="projectsLoading"
            filterable
            placeholder="搜索并选择项目"
          />
        </n-form-item>
        <n-form-item label="平台" required>
          <n-select
            v-model:value="platforms"
            :options="csvPlatformOptions"
            multiple
            :disabled="!selectedProjectId"
            placeholder="请先选择项目"
          />
        </n-form-item>
        <n-form-item label="CSV 文件" required>
          <n-upload
            accept=".csv,text/csv"
            :max="1"
            :file-list="fileList"
            :show-download-button="false"
            @change="handleFileChange"
          >
            <n-button dashed style="width:100%">点击或拖拽上传 CSV</n-button>
          </n-upload>
        </n-form-item>
      </n-form>

      <n-alert type="info" style="margin-bottom: 16px">
        <n-text>CSV 格式示例：<br />
          <code>Date,Revenue,Currency<br />2024-01-15,1234.56,USD</code>
        </n-text>
      </n-alert>

      <div v-if="importResult" style="margin-bottom: 16px">
        <n-alert :type="importResult.failed > 0 ? 'warning' : 'success'">
          导入 {{ importResult.imported }} 条成功，{{ importResult.failed }} 条失败
        </n-alert>
        <n-list v-if="importResult.errors.length" size="small" style="margin-top: 8px">
          <n-list-item v-for="(err, i) in importResult.errors" :key="i">
            <n-text type="error">{{ err }}</n-text>
          </n-list-item>
        </n-list>
      </div>

      <n-flex justify="end" gap="12">
        <n-button @click="handleClose">关闭</n-button>
        <n-button type="primary" :loading="loading" @click="handleImport">开始导入</n-button>
      </n-flex>
    </n-drawer-content>
  </n-drawer>
</template>
