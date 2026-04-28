import { ref, onMounted } from 'vue'
import { projectApi } from '../api/admin/project'

export interface ProjectOption {
  label: string
  value: string
  platformName: string
}

const PLATFORM_MAP: Array<[RegExp, string]> = [
  [/tiktok/i, 'tiktok'],
  [/youtube/i, 'youtube'],
  [/instagram/i, 'instagram'],
  [/kwai/i, 'kwai'],
  [/douyin|抖音/i, 'douyin'],
  [/kuaishou|快手/i, 'kuaishou'],
  [/xiaohongshu|小红书/i, 'xiaohongshu'],
]

export function normalizePlatform(platformName: string): string {
  for (const [pattern, value] of PLATFORM_MAP) {
    if (pattern.test(platformName)) return value
  }
  return 'tiktok'
}

/** 将项目的 platformName（可能逗号分隔）转为下拉选项列表 */
export function getProjectPlatformOptions(platformName: string) {
  const parts = platformName.split(',').map((s) => s.trim()).filter(Boolean)
  return parts.map((p) => {
    const value = normalizePlatform(p)
    return { label: p, value }
  })
}

/** 返回项目所有平台的 normalized value 列表（用于自动全选） */
export function getProjectPlatformValues(platformName: string): string[] {
  return platformName.split(',').map((s) => normalizePlatform(s.trim())).filter(Boolean)
}

export function useProjectSelect() {
  const projectOptions = ref<ProjectOption[]>([])
  const projectsLoading = ref(false)

  async function loadProjects() {
    projectsLoading.value = true
    try {
      const result = await projectApi.list({ page: 1, pageSize: 200 })
      projectOptions.value = (result?.items ?? []).map((p) => ({
        label: p.title,
        value: p.id,
        platformName: p.platformName,
      }))
    } catch {
      // ignore, just no options
    } finally {
      projectsLoading.value = false
    }
  }

  onMounted(loadProjects)

  return { projectOptions, projectsLoading, loadProjects }
}
