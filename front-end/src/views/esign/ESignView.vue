<template>
  <div class="esign-page">
    <div v-if="loading" class="esign-loading">
      <n-spin size="large" />
    </div>

    <template v-else-if="error">
      <n-result status="error" :title="$t('esign.invalidToken')" />
    </template>

    <template v-else-if="signSuccess">
      <n-result
        status="success"
        :title="$t('contract.signSuccess')"
        :description="$t('contract.signSuccessDesc')"
      />
    </template>

    <template v-else-if="preview">
      <div class="esign-card">
        <n-card :title="$t('esign.title')">
          <n-descriptions bordered :column="1">
            <n-descriptions-item :label="$t('contract.projectTitle')">
              {{ preview.projectTitle }}
            </n-descriptions-item>
            <n-descriptions-item :label="$t('contract.investorName')">
              {{ preview.investorName }}
            </n-descriptions-item>
            <n-descriptions-item :label="$t('contract.sharePct')">
              {{ preview.sharePct }}‰
            </n-descriptions-item>
            <n-descriptions-item v-if="preview.contractSnapshot" :label="$t('contract.contractSnapshot')">
              <pre class="contract-snapshot">{{ preview.contractSnapshot }}</pre>
            </n-descriptions-item>
          </n-descriptions>

          <n-divider />

          <p class="signature-label">{{ $t('contract.signaturePad') }}</p>
          <SignaturePad v-model="signatureDataUrl" :width="500" :height="200" />

          <template #footer>
            <n-space justify="end">
              <n-button
                type="primary"
                :loading="submitting"
                :disabled="!signatureDataUrl"
                @click="handleSubmit"
              >
                {{ $t('contract.submitSignature') }}
              </n-button>
            </n-space>
          </template>
        </n-card>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useMessage, NSpin, NResult, NCard, NDescriptions, NDescriptionsItem, NDivider, NSpace, NButton } from 'naive-ui'
import { useI18n } from 'vue-i18n'
import SignaturePad from '../../components/business/SignaturePad.vue'
import { getESignPreview, submitSignature } from '../../api/public/esign'
import type { ContractPreviewDto } from '../../types/contract'

const route = useRoute()
const message = useMessage()
const { t } = useI18n()

const loading = ref(true)
const error = ref(false)
const submitting = ref(false)
const signSuccess = ref(false)
const preview = ref<ContractPreviewDto | null>(null)
const signatureDataUrl = ref('')

onMounted(async () => {
  try {
    const token = route.params.token as string
    preview.value = await getESignPreview(token)
  } catch {
    error.value = true
  } finally {
    loading.value = false
  }
})

async function handleSubmit() {
  if (!signatureDataUrl.value) return
  submitting.value = true
  try {
    const token = route.params.token as string
    await submitSignature(token, signatureDataUrl.value)
    signSuccess.value = true
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? t('esign.invalidToken'))
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.esign-page {
  display: flex;
  justify-content: center;
  align-items: flex-start;
  min-height: 100vh;
  padding: 40px 16px;
  background: #f5f5f5;
}

.esign-loading {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 300px;
}

.esign-card {
  width: 100%;
  max-width: 600px;
}

.signature-label {
  margin-bottom: 8px;
  font-size: 14px;
  color: #666;
}

.contract-snapshot {
  white-space: pre-wrap;
  font-size: 13px;
  color: #333;
}
</style>
