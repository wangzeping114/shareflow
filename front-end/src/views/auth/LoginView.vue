<script setup lang="ts">
import { reactive, ref } from 'vue'
import { NAlert, NButton, NCard, NForm, NFormItem, NInput } from 'naive-ui'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { z } from 'zod'
import { useAuthStore } from '../../stores/auth'

const { t } = useI18n()
const router = useRouter()
const authStore = useAuthStore()

const form = reactive({
  username: '',
  password: '',
})

const loading = ref(false)
const errorMessage = ref('')

const schema = z.object({
  username: z.string().min(3),
  password: z.string().min(6),
})

async function handleSubmit() {
  errorMessage.value = ''
  const parsed = schema.safeParse(form)
  if (!parsed.success) {
    errorMessage.value = parsed.error.issues[0]?.message ?? t('auth.loginFailed')
    return
  }

  loading.value = true

  try {
    await authStore.login(form.username, form.password)
    await router.replace(authStore.homePath)
  } catch {
    errorMessage.value = t('auth.loginFailed')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <div class="hero">
        <span class="badge">{{ t('app.brand') }}</span>
        <h1>{{ t('auth.title') }}</h1>
        <p>{{ t('auth.subtitle') }}</p>
      </div>

      <n-card :bordered="false">
        <n-form @submit.prevent="handleSubmit">
          <n-form-item :label="t('auth.username')">
            <n-input v-model:value="form.username" :placeholder="t('auth.usernamePlaceholder')" />
          </n-form-item>

          <n-form-item :label="t('auth.password')">
            <n-input
              v-model:value="form.password"
              type="password"
              show-password-on="click"
              :placeholder="t('auth.passwordPlaceholder')"
            />
          </n-form-item>

          <n-alert v-if="errorMessage" type="error" :show-icon="false" style="margin-bottom: 16px;">
            {{ errorMessage }}
          </n-alert>

          <n-button block type="primary" attr-type="submit" :loading="loading">
            {{ t('app.login') }}
          </n-button>
        </n-form>
      </n-card>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 24px;
  background: radial-gradient(circle at top, #dbeafe 0%, #eff6ff 45%, #f8fafc 100%);
}

.login-card {
  width: min(960px, 100%);
  display: grid;
  grid-template-columns: 1.1fr 0.9fr;
  gap: 24px;
  padding: 24px;
  border-radius: 28px;
  background: rgba(255, 255, 255, 0.88);
  backdrop-filter: blur(14px);
  box-shadow: 0 24px 60px rgba(37, 99, 235, 0.12);
}

.hero {
  padding: 32px;
}

.badge {
  display: inline-flex;
  padding: 8px 14px;
  border-radius: 999px;
  background: #dbeafe;
  color: #1d4ed8;
  font-weight: 700;
}

h1 {
  margin: 24px 0 12px;
  font-size: 40px;
}

p {
  margin: 0;
  color: #475569;
  line-height: 1.8;
}

@media (max-width: 900px) {
  .login-card {
    grid-template-columns: 1fr;
  }
}
</style>
