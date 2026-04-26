import axios, { type InternalAxiosRequestConfig } from 'axios'
import { pinia } from '../stores'
import { useAuthStore } from '../stores/auth'

type RetryableConfig = InternalAxiosRequestConfig & {
  _retry?: boolean
}

const http = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5001',
  timeout: 15000,
})

let isRefreshing = false
let pendingQueue: Array<(token: string | null) => void> = []

function flushQueue(token: string | null) {
  pendingQueue.forEach((resolve) => resolve(token))
  pendingQueue = []
}

http.interceptors.request.use((config) => {
  const authStore = useAuthStore(pinia)
  if (authStore.accessToken) {
    config.headers.Authorization = `Bearer ${authStore.accessToken}`
  }

  return config
})

http.interceptors.response.use(
  (response) => response,
  async (error) => {
    const authStore = useAuthStore(pinia)
    const originalRequest = (error.config ?? {}) as RetryableConfig

    if (error.response?.status === 401 && !originalRequest._retry && authStore.refreshTokenValue) {
      originalRequest.headers = originalRequest.headers ?? {}

      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          pendingQueue.push((token) => {
            if (!token) {
              reject(error)
              return
            }

            originalRequest.headers.Authorization = `Bearer ${token}`
            resolve(http(originalRequest))
          })
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const payload = await authStore.refreshSession()
        flushQueue(payload.accessToken)
        originalRequest.headers.Authorization = `Bearer ${payload.accessToken}`
        return http(originalRequest)
      } catch (refreshError) {
        flushQueue(null)
        authStore.clearSession()
        window.location.href = '/auth/login'
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)

export default http
