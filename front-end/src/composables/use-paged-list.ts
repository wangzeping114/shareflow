import { ref, computed, onMounted, type Ref, type ComputedRef } from 'vue'
import type { PagedResult } from '../types/common'

export interface PagedListOptions<T, F extends Record<string, unknown> = Record<string, unknown>> {
  /** 数据获取函数，接收 filter + page + pageSize，返回分页结果 */
  fetcher: (params: F & { page: number; pageSize: number }) => Promise<PagedResult<T>>
  /** 初始过滤条件 */
  initialFilter?: F
  /** 初始每页条数，默认 20 */
  defaultPageSize?: number
  /** 是否在 onMounted 时自动加载，默认 true */
  immediate?: boolean
}

export interface PagedListReturn<T, F extends Record<string, unknown> = Record<string, unknown>> {
  /** 当前页数据 */
  list: Ref<T[]>
  /** 总条数 */
  total: Ref<number>
  /** 当前页码 */
  page: Ref<number>
  /** 每页条数 */
  pageSize: Ref<number>
  /** 加载状态 */
  loading: Ref<boolean>
  /** 过滤条件（响应式，修改后调用 search() 生效） */
  filter: Ref<F>
  /**
   * 手动触发加载（保持当前页码）
   * 一般用于外部事件触发刷新（如创建/删除后）
   */
  reload: () => Promise<void>
  /**
   * 重置到第 1 页并加载
   * 一般用于过滤条件变化时
   */
  search: () => Promise<void>
  /**
   * NDataTable pagination 绑定对象（computed自动解析 Ref，直接传给 :pagination）
   */
  paginationProps: ComputedRef<{
    page: number
    itemCount: number
    pageSize: number
    pageSizes: number[]
    showSizePicker: boolean
    onChange: (p: number) => void
    onUpdatePageSize: (ps: number) => void
  }>
}

/**
 * 通用分页列表 Composable
 *
 * @example
 * ```ts
 * const { list, loading, filter, search, paginationProps } = usePagedList({
 *   fetcher: (params) => projectApi.list(params),
 *   initialFilter: { status: undefined } as { status?: ProjectStatus },
 * })
 *
 * // 过滤条件变更时重置到第1页
 * watch(() => filter.value.status, search)
 * ```
 */
export function usePagedList<T, F extends Record<string, unknown> = Record<string, unknown>>(
  options: PagedListOptions<T, F>,
): PagedListReturn<T, F> {
  const {
    fetcher,
    initialFilter = {} as F,
    defaultPageSize = 20,
    immediate = true,
  } = options

  const list = ref<T[]>([]) as Ref<T[]>
  const total = ref(0)
  const page = ref(1)
  const pageSize = ref(defaultPageSize)
  const loading = ref(false)
  const filter = ref<F>({ ...initialFilter }) as Ref<F>

  async function fetchData() {
    loading.value = true
    try {
      const result = await fetcher({
        ...filter.value,
        page: page.value,
        pageSize: pageSize.value,
      })
      list.value = result.items
      total.value = result.total
    } finally {
      loading.value = false
    }
  }

  async function reload() {
    await fetchData()
  }

  async function search() {
    page.value = 1
    await fetchData()
  }

  function onPageChange(p: number) {
    page.value = p
    fetchData()
  }

  function onPageSizeChange(ps: number) {
    pageSize.value = ps
    page.value = 1
    fetchData()
  }

  const paginationProps = computed(() => ({
    page: page.value,
    itemCount: total.value,
    pageSize: pageSize.value,
    pageSizes: [10, 20, 50, 100],
    showSizePicker: true,
    onChange: onPageChange,
    onUpdatePageSize: onPageSizeChange,
  }))

  if (immediate) {
    onMounted(fetchData)
  }

  return {
    list,
    total,
    page,
    pageSize,
    loading,
    filter,
    reload,
    search,
    paginationProps,
  }
}
