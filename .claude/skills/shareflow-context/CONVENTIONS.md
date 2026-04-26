# ShareFlow — 编码规范速查

## 后端规范

### 文件组织
```
Application/{Module}/
├── DTOs/           → XxxRequest, XxxDto, XxxResponse
├── Interfaces/     → IXxxService, IXxxRepository（Domain 层接口）
├── Services/       → XxxService.cs（实现）
├── Validators/     → XxxRequestValidator.cs
└── MappingConfig.cs → XxxMappingConfig : IRegister
```

### Entity 编写规范
```csharp
// ✅ 正确：所有属性 private set，通过工厂方法/行为方法修改
public class VideoProject : Entity<Guid>
{
    public string Title { get; private set; } = string.Empty;

    public static VideoProject Create(string title, ...) => new() { Id = Guid.NewGuid(), Title = title };
    public void Rename(string newTitle) { Title = newTitle; SetUpdatedAt(); }
}

// ❌ 错误：公开 setter
public string Title { get; set; }
```

### Service 编写规范
```csharp
// ✅ 正确：通过接口注入，async/await 到底
public class ProjectService(
    IVideoProjectRepository projectRepo,
    IMapper mapper,
    IRegionContext regionContext) : IProjectService
{
    public async Task<PagedResult<ProjectDto>> GetListAsync(ProjectQueryRequest q)
    {
        var projects = await projectRepo.GetPagedAsync(q.Page, q.PageSize);
        return projects.Adapt<PagedResult<ProjectDto>>(mapper.Config);
    }
}

// ❌ 错误：直接注入 DbContext，手动赋值 DTO
```

### 错误处理规范
```csharp
// ✅ 使用 BusinessException
throw new BusinessException("contract.tokenExpired");
throw new BusinessException("wallet.insufficientBalance", new[] { amount.ToString() });

// 错误码命名：{domain}.{reason}（camelCase）
// GlobalExceptionMiddleware 统一转换为 ApiResponse 错误格式
```

### Controller 规范
```csharp
[ApiController]
[Route("v1/admin/[controller]")]
[Authorize]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    // ✅ 正确：Controller 只做参数提取 + 调用 Service
    [HttpGet]
    [Permission(Permissions.ProjectRead)]
    public async Task<ApiResponse<PagedResult<ProjectDto>>> GetList(
        [FromQuery] ProjectQueryRequest query)
        => ApiResponse.Success(await projectService.GetListAsync(query));

    // ✅ 从 JWT 提取当前用户
    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
```

### 测试命名规范
```
MethodName_StateUnderTest_ExpectedBehavior
示例:
  Sign_WhenTokenExpired_ThrowsBusinessException
  Calculate_WhenPermilleIs100_ReturnsTenPercentOfRevenue
  Freeze_WhenAmountExceedsBalance_ThrowsBusinessException
```

## 前端规范

### 组件编写规范
```typescript
// ✅ 正确：<script setup> + defineProps TypeScript 类型
<script setup lang="ts">
interface Props {
  projectId: string
  mode: 'view' | 'edit'
}
const props = defineProps<Props>()
const emit = defineEmits<{ (e: 'save', id: string): void }>()
</script>

// ❌ 错误：不写类型声明，直接修改父组件 ref
```

### API 调用规范
```typescript
// ✅ 正确：统一调用 api/ 模块，不直接用 axios
import { projectApi } from '@/api/project'
const { data } = await projectApi.list({ page: 1, pageSize: 20 })

// ✅ 错误处理：http.ts 拦截器统一处理 401/403/500
// 40x 业务错误：toast message，不 throw
// 500：统一 toast "服务器错误"
```

### Store 规范
```typescript
// ✅ 正确：Composition API 风格 defineStore
export const useProjectStore = defineStore('project', () => {
  const list = ref<Project[]>([])
  const loading = ref(false)

  async function fetchList(params: ProjectQueryRequest) {
    loading.value = true
    try {
      const res = await projectApi.list(params)
      list.value = res.data.items
    } finally {
      loading.value = false
    }
  }
  return { list, loading, fetchList }
})

// ✅ 需持久化的 Store 加 { persist: true }
// ❌ 不要在 Store 中直接操作 DOM
```

### 国际化规范
```typescript
// ✅ 正确：所有 UI 文案通过 $t()
<n-button>{{ $t('common.save') }}</n-button>
<n-form-item :label="$t('project.title')">

// key 命名：{模块}.{字段/操作}（点号分隔）
// zh-CN 和 en-US 文件同步维护
// ❌ 错误：硬编码中文字符串在模板或 JS 中
```

### 权限控制规范
```typescript
// ✅ 模板中用 v-permission 指令
<n-button v-permission="'project.write'">新建项目</n-button>

// ✅ 代码中用 hasPermission
const { hasPermission } = useAuthStore()
if (!hasPermission('dividend.write')) return

// SuperAdmin 自动绕过所有权限检查（在 hasPermission 实现中处理）
```

### 文件命名规范
```
views/admin/projects/ProjectListView.vue   ← PascalCase.vue
views/admin/projects/ProjectDetailView.vue
components/charts/LineChart.vue            ← PascalCase.vue
composables/usePagedList.ts                ← camelCase.ts
types/project.ts                           ← kebab-case.ts
api/project.ts                             ← kebab-case.ts
stores/project.ts                          ← kebab-case.ts
```

## 禁止模式（共同）

### 后端禁止
- ❌ Controller 直接注入 DbContext
- ❌ 手动赋值 DTO（`dto.Name = entity.Name`）→ 必须用 Mapster
- ❌ 使用 `float`/`double` 表示金额
- ❌ `if (regionContext.Name == "overseas")` → 使用 IRegionContext 接口方法
- ❌ 硬编码连接字符串、API Key
- ❌ 业务 Service 直接 `throw new Exception()` → 必须用 BusinessException

### 前端禁止
- ❌ 直接 `axios.get()` 跳过 http.ts 封装
- ❌ 模板中使用硬编码字符串（不走 i18n）
- ❌ 子组件直接 `$parent.xxx = value`
- ❌ 在 mounted 中写大量业务逻辑（抽取到 composable）
- ❌ 权限判断散落在各处（统一用 v-permission 或 hasPermission）
