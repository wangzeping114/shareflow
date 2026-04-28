<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import {
  NCard, NDescriptions, NDescriptionsItem, NTag, NDivider,
  NButton, NFlex, NAlert, NSpin, NModal, useMessage,
} from 'naive-ui'
import { getContractById, generateSignLink } from '../../../api/admin/contract'
import type { ContractDetailDto, ContractStatus, GenerateSignLinkResult } from '../../../types/contract'

const route = useRoute()
const message = useMessage()

const loading = ref(true)
const contract = ref<ContractDetailDto | null>(null)
const signLinkResult = ref<GenerateSignLinkResult | null>(null)
const generating = ref(false)
const windowOrigin = window.location.origin

const statusLabel: Record<ContractStatus, string> = {
  Draft: '草稿',
  Sent: '已发送',
  Signed: '已签署',
  Executed: '已执行',
  Expired: '已过期',
  PendingRenew: '等待续签',
  Renewing: '续签中',
  Superseded: '已替代',
}

const statusTagType: Record<ContractStatus, 'default' | 'info' | 'success' | 'warning' | 'error'> = {
  Draft: 'default',
  Sent: 'info',
  Signed: 'warning',
  Executed: 'success',
  Expired: 'error',
  PendingRenew: 'warning',
  Renewing: 'info',
  Superseded: 'default',
}

onMounted(async () => {
  try {
    const id = route.params.id as string
    contract.value = await getContractById(id)
  } catch {
    message.error('加载合同失败')
  } finally {
    loading.value = false
  }
})

async function handleGenerateSignLink() {
  if (!contract.value) return
  generating.value = true
  try {
    signLinkResult.value = await generateSignLink(contract.value.id)
    message.success('签约链接已生成')
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? '生成链接失败')
  } finally {
    generating.value = false
  }
}

function copySignLink() {
  if (!signLinkResult.value) return
  const url = `${windowOrigin}${signLinkResult.value.signUrl}`
  navigator.clipboard.writeText(url).then(() => message.success('已复制到剪贴板'))
}

// ── 合同正文预览 ──────────────────────────
const showSnapshotModal = ref(false)
</script>

<template>
  <div class="view-container">
    <div v-if="loading" class="loading-wrap">
      <n-spin size="large" />
    </div>

    <template v-else-if="contract">
      <n-card :title="`合同详情 — ${contract.projectTitle}`">
        <n-descriptions bordered :column="2">
          <n-descriptions-item label="投资人">{{ contract.investorName }}</n-descriptions-item>
          <n-descriptions-item label="状态">
            <n-tag :type="statusTagType[contract.status]" size="small" round>
              {{ statusLabel[contract.status] }}
            </n-tag>
          </n-descriptions-item>
          <n-descriptions-item label="模板类型">{{ contract.templateType }}</n-descriptions-item>
          <n-descriptions-item label="签署时间">
            {{ contract.signedAt ? new Date(contract.signedAt).toLocaleString('zh-CN') : '—（生效时间以实际签约日期为准）' }}
          </n-descriptions-item>
        </n-descriptions>

        <n-divider />

        <n-flex gap="12" align="center">
          <n-button
            v-if="contract.contractSnapshot"
            secondary
            @click="showSnapshotModal = true"
          >
            📄 查看合同正文
          </n-button>
          <n-button
            v-if="['Draft', 'Sent', 'Expired'].includes(contract.status)"
            type="primary"
            :loading="generating"
            @click="handleGenerateSignLink"
          >
            生成签约链接
          </n-button>
        </n-flex>

        <template v-if="signLinkResult">
          <n-divider />
          <n-alert type="success" title="签约链接已生成">
            <template #default>
              <p>链接：<code>{{ `${windowOrigin}${signLinkResult.signUrl}` }}</code></p>
              <p>有效期至：{{ new Date(signLinkResult.expiresAt).toLocaleString('zh-CN') }}</p>
              <n-button size="small" @click="copySignLink">复制链接</n-button>
            </template>
          </n-alert>
        </template>
      </n-card>
    </template>

    <!-- 合同正文预览 Modal -->
    <n-modal v-model:show="showSnapshotModal" :mask-closable="true">
      <n-card
        :title="`合同正文 — ${contract?.projectTitle} / ${contract?.investorName}`"
        style="width: 760px; max-height: 90vh; display: flex; flex-direction: column"
        :bordered="false"
      >
        <div class="snapshot-body">
          <pre class="snapshot-text">{{ contract?.contractSnapshot }}</pre>
        </div>
        <template #footer>
          <n-flex justify="end">
            <n-button @click="showSnapshotModal = false">关闭</n-button>
          </n-flex>
        </template>
      </n-card>
    </n-modal>
  </div>
</template>

<style scoped>
.view-container {
  padding: 16px;
}
.loading-wrap {
  display: flex;
  justify-content: center;
  padding: 60px;
}
.snapshot-body {
  overflow-y: auto;
  max-height: 65vh;
  background: #f8f8f8;
  border-radius: 6px;
  padding: 20px;
}
.snapshot-text {
  white-space: pre-wrap;
  font-family: 'Courier New', Courier, monospace;
  font-size: 13px;
  line-height: 1.8;
  color: #333;
  margin: 0;
}
</style>