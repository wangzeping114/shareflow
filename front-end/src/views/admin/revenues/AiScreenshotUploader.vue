<script setup lang="ts">
import { ref, watch } from 'vue'
import {
  NModal, NForm, NFormItem, NSelect, NButton, NFlex, NUpload,
  NAlert, NText, NInputNumber, NTag, useMessage, type UploadFileInfo,
} from 'naive-ui'
import { previewRevenueScreenshot, confirmAiImport } from '../../../api/admin/revenue'
import type { AiImportPreviewDto } from '../../../types/revenue'
import { useProjectSelect, getProjectPlatformOptions, getProjectPlatformValues } from '../../../composables/use-project-select'

const props = defineProps<{ show: boolean }>()
const emit = defineEmits<{
  'update:show': [value: boolean]
  imported: []
}>()

const message = useMessage()
const step = ref<'upload' | 'preview'>('upload')
const loading = ref(false)
const confirmLoading = ref(false)
const selectedProjectId = ref<string | null>(null)
const platform = ref('tiktok')
const fileList = ref<UploadFileInfo[]>([])
const preview = ref<AiImportPreviewDto | null>(null)
const overrideAmount = ref<number | null>(null)

const { projectOptions, projectsLoading } = useProjectSelect()
const aiPlatformOptions = ref<{ label: string; value: string }[]>([])

// 选中项目后仅展示该项目的平台（AI 识别单选，取第一个）
watch(selectedProjectId, (id) => {
  if (!id) { aiPlatformOptions.value = []; return }
  const opt = projectOptions.value.find((p) => p.value === id)
  if (opt) {
    aiPlatformOptions.value = getProjectPlatformOptions(opt.platformName)
    platform.value = getProjectPlatformValues(opt.platformName)[0] ?? 'tiktok'
  }
})

function handleClose() {
  step.value = 'upload'
  preview.value = null
  fileList.value = []
  overrideAmount.value = null
  selectedProjectId.value = null
  platform.value = 'tiktok'
  aiPlatformOptions.value = []
  emit('update:show', false)
}

function handleFileChange(data: { fileList: UploadFileInfo[] }) {
  fileList.value = data.fileList.slice(-1)
}

async function handlePreview() {
  if (!selectedProjectId.value) {
    message.warning('请选择项目')
    return
  }
  const file = fileList.value[0]?.file
  if (!file) {
    message.warning('请选择截图文件')
    return
  }
  loading.value = true
  try {
    const result = await previewRevenueScreenshot(selectedProjectId.value!, platform.value, file)
    preview.value = result!
    overrideAmount.value = result!.amount
    step.value = 'preview'
  } catch {
    message.error('AI 识别失败，请重试')
  } finally {
    loading.value = false
  }
}

async function handleConfirm() {
  if (!preview.value) return
  confirmLoading.value = true
  try {
    await confirmAiImport(preview.value.revenueId, {
      overrideAmount: overrideAmount.value !== preview.value.amount ? overrideAmount.value ?? undefined : undefined,
    })
    message.success('已成功录入收益记录')
    emit('imported')
    handleClose()
  } catch {
    message.error('确认失败')
  } finally {
    confirmLoading.value = false
  }
}
</script>

<template>
  <n-modal
    :show="props.show"
    title="AI 截图识别"
    preset="card"
    style="width: 520px"
    @update:show="emit('update:show', $event)"
    @mask-click="handleClose"
  >
    <!-- Step 1: 上传 -->
    <template v-if="step === 'upload'">
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
            v-model:value="platform"
            :options="aiPlatformOptions"
            :disabled="!selectedProjectId"
            placeholder="请先选择项目"
          />
        </n-form-item>
        <n-form-item label="收益截图" required>
          <n-upload
            accept="image/*"
            :max="1"
            :file-list="fileList"
            list-type="image-card"
            @change="handleFileChange"
          />
        </n-form-item>
      </n-form>
      <n-alert type="info" style="margin-bottom: 16px">
        支持 PNG / JPG 格式截图。AI 将自动识别金额、货币和日期。
      </n-alert>
      <n-flex justify="end" gap="12">
        <n-button @click="handleClose">取消</n-button>
        <n-button type="primary" :loading="loading" @click="handlePreview">开始识别</n-button>
      </n-flex>
    </template>

    <!-- Step 2: 预览确认 -->
    <template v-else-if="step === 'preview' && preview">
      <div style="margin-bottom: 16px">
        <n-alert v-if="preview.needsVerification" type="warning" style="margin-bottom: 12px">
          AI 置信度较低（{{ (preview.confidence * 100).toFixed(0) }}%），建议人工核实后确认。
        </n-alert>
        <n-alert v-else type="success" style="margin-bottom: 12px">
          AI 识别置信度：
          <n-tag :type="preview.confidence >= 0.9 ? 'success' : 'info'" size="small">
            {{ (preview.confidence * 100).toFixed(0) }}%
          </n-tag>
        </n-alert>
      </div>

      <n-form label-placement="top">
        <n-flex gap="12">
          <n-form-item label="识别金额（可修改）" required style="flex:1">
            <n-input-number
              v-model:value="overrideAmount"
              :min="0.01"
              :precision="2"
              style="width:100%"
            />
          </n-form-item>
          <n-form-item label="货币" style="width: 100px">
            <n-text>{{ preview.currency }}</n-text>
          </n-form-item>
        </n-flex>
        <n-form-item label="收益日期">
          <n-text>{{ preview.revenueDate.slice(0, 10) }}</n-text>
        </n-form-item>
      </n-form>

      <n-flex justify="end" gap="12" style="margin-top: 16px">
        <n-button @click="step = 'upload'">重新上传</n-button>
        <n-button type="primary" :loading="confirmLoading" @click="handleConfirm">确认录入</n-button>
      </n-flex>
    </template>
  </n-modal>
</template>
