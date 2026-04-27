# ShareFlow — Copilot 项目全局指令

## 项目概述

**ShareFlow** 短视频持股分红平台。**同一套代码库，双实例部署**：

- 境外实例（Overseas）：TikTok / YouTube / Instagram，**USD** 结算，客户端英文界面
- 境内实例（Domestic）：抖音 / 快手 / 小红书，**CNY** 结算，客户端中文界面
- **管理后台（Admin + Sales）无论境内外实例，统一使用中文界面**

通过环境变量 `REGION=overseas|domestic` 区分实例，数据库完全独立。

## 技术栈

- **后端**：.NET 8, ASP.NET Core Web API, EF Core 8 + PostgreSQL 16, Redis 7, Autofac, Quartz.NET, Serilog, FluentValidation, Mapster, MediatR, BCrypt, JWT Bearer, QuestPDF, ClosedXML
- **前端**：Vue 3.5, TypeScript 5.6, Vite 6, Naive UI, Pinia, Axios, vue-i18n, ECharts 5, Vee-Validate + Zod
- **部署**：Docker + docker-compose（双套：overseas / domestic）

## 后端架构（Clean Architecture）

```
back-end/src/
├── Api/              → Controllers, Middleware, Program.cs
├── Application/      → Services, DTOs, Validators, Interfaces, MappingConfigs
├── Domain/           → Entities, Enums, Interfaces, Exceptions, Events
├── Infrastructure/   → Repositories, EF Configs, External Services, Jobs
└── Migrator/         → EF Core Migrations, DesignTimeDbContextFactory
```

## 角色与权限

- `SuperAdmin` — 超级管理员，拥有所有权限，绕过 Permission 校验
- `BackendCustom` — 后台自定义角色，权限由 `BackendRole.Permissions[]` 动态配置
- `Sales` — 销售员，管理线索、发起合同
- `Client` — 持股客户端，只读自己的数据
- Partner — 国内合伙人（境内实例）

权限常量在 `Domain/Constants/Permissions.cs`，Controller 方法标注 `[Permission("xxx.yyy")]`。

## EF Core 迁移

```bash
# 在 back-end/ 目录执行
dotnet ef migrations add <迁移名> --project src/Migrator --startup-project src/Migrator
dotnet ef database update --project src/Migrator --startup-project src/Migrator
```

## 前端架构

```
front-end/src/
├── api/          → Axios 封装 + 模块 API（统一通过 http.ts 调用）
├── views/        → 页面组件（按角色分目录：admin / sales / client / public）
├── components/   → 通用组件（base/） + 业务组件
├── stores/       → Pinia Store（auth, notification 等，auth 持久化）
├── router/       → 路由 + 守卫（按角色跳转）
├── i18n/         → 国际化（zh-CN / en-US）
├── types/        → TypeScript 接口
├── composables/  → usePagedList, useForm, useRegion, useExportDownload
└── directives/   → v-permission
```

## 编码规范

### 后端规范

- 所有金额用 `decimal`，**禁止** `float`/`double`
- 持股比例 `decimal(8,4)`，金额 `decimal(15,2)`，EF Fluent API 显式配置精度
- 数据库列名 `snake_case`，C# 代码 `PascalCase`
- **禁止** Controller 直接注入 DbContext，必须通过 Repository
- 复杂查询加 `AsNoTracking()`
- 异常统一用 `throw new BusinessException("domain.errorCode")`
- **Mapster**：每模块建 `XxxMappingConfig : IRegister`，注入 `IMapper`，**禁止手动赋值** DTO
- Entity 属性全部 `private set`，通过工厂方法 `Create()` 和行为方法修改
- **禁止** `if (regionName == "overseas")` 硬判断，必须通过 `IRegionContext` 接口
- 测试命名：`MethodName_StateUnderTest_ExpectedBehavior`

### 前端规范

- 组件文件 `PascalCase.vue`，其他文件 `kebab-case.ts`
- Props 必须用 TypeScript `interface` 定义，通过 `defineProps<Props>()` 声明
- 组件通过 Props 驱动，事件通过 Emits，**禁止**直接修改父组件状态
- 所有 UI 文案用 `$t('module.key')` 国际化，**禁止**硬编码中文
- 权限控制用 `v-permission="'xxx.yyy'"` 指令或 `hasPermission()` 方法
- HTTP 请求**必须**通过 `api/` 模块，**禁止**直接调用 `axios.get()`

## 数据库规范

- PostgreSQL 16，EF Core Code First
- 主键：`Guid`（User、Contract 等业务实体）或 `long`（WalletTransaction、Notification 等高频日志）
- 时间字段：`CreatedAt`, `UpdatedAt`（UTC）
- 不使用软删除，用 `Status` 枚举管理生命周期

## API 规范

- 路径：`/v1/{scope}/{resource}`，RESTful
- 响应格式：`ApiResponse<T>` 统一封装
- 分页：`PagedResult<T>` { Items, Total, Page, PageSize }
- 认证：JWT Bearer（AccessToken 2h + RefreshToken 7d，Redis 黑名单）
- 内部跨实例 API：`/internal/summary`，Header `X-Internal-Token` + IP 白名单

## 核心设计模式

- **IRegionContext**：所有区域差异（货币/语言/平台/模板）通过接口注入，运行时按 `REGION` 环境变量决定实现
- **策略工厂**：`IPlatformRevenueHandlerFactory` — 按平台+来源选择 CSV/AI/手工录入处理器
- **领域事件**：MediatR `INotification`，跨模块解耦（如 `ContractSignedEvent` → 账号初始化，`DividendDistributedEvent` → 钱包到账）
- **AOP 拦截器**：Autofac `EnableInterfaceInterceptors()`，`AuditLogInterceptor` + `PerformanceInterceptor`

## 参考文档

- 完整设计：`ShareFlow 短视频持股分红平台设计文档.md`
- 任务拆解： `开发任务拆解清单.md`
