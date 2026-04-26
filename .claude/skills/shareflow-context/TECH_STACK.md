# ShareFlow — 技术栈速查

## 后端 NuGet 包

| 包 | 版本 | 用途 |
|----|------|------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 8.x | JWT 认证 |
| `Autofac.Extensions.DependencyInjection` | 9.x | IoC 容器 |
| `Autofac.Extras.DynamicProxy` | 7.x | AOP 拦截器 |
| `MediatR` | 12.x | Domain Events / CQRS |
| `FluentValidation.AspNetCore` | 11.x | 请求验证 |
| `Mapster` | 7.x | DTO 映射 |
| `Mapster.DependencyInjection` | 1.x | IMapper 注入 |
| `Microsoft.EntityFrameworkCore` | 8.x | ORM |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 8.x | PG 驱动 |
| `StackExchange.Redis` | 2.x | Redis 客户端 |
| `Quartz.Extensions.Hosting` | 3.x | 定时任务 |
| `Serilog.AspNetCore` | 8.x | 结构化日志 |
| `Serilog.Sinks.File` | 5.x | 文件日志 |
| `BCrypt.Net-Next` | 4.x | 密码哈希 |
| `QuestPDF` | 2024.x | PDF 生成 |
| `ClosedXML` | 0.102.x | Excel 导出 |
| `xunit` | 2.x | 单元测试 |
| `Moq` | 4.x | Mock |
| `FluentAssertions` | 6.x | 断言 |
| `Testcontainers.PostgreSql` | 3.x | 集成测试 |

## 前端 NPM 包

| 包 | 版本 | 用途 |
|----|------|------|
| `vue` | 3.5.x | 框架 |
| `typescript` | 5.6.x | 类型系统 |
| `vite` | 6.x | 构建工具 |
| `naive-ui` | 2.x | UI 组件库 |
| `pinia` | 2.x | 状态管理 |
| `pinia-plugin-persistedstate` | 3.x | Store 持久化 |
| `axios` | 1.x | HTTP 客户端 |
| `vue-i18n` | 9.x | 国际化 |
| `echarts` | 5.x | 图表 |
| `vue-echarts` | 6.x | ECharts Vue 封装 |
| `vee-validate` | 4.x | 表单验证 |
| `zod` | 3.x | Schema 验证 |
| `@vueuse/core` | 10.x | 工具 Composables |
| `vitest` | 2.x | 单元测试 |
| `@vue/test-utils` | 2.x | 组件测试 |

## 后端项目间依赖关系

```
Api → Application → Domain
Api → Infrastructure → Domain
Infrastructure → Application (Interfaces)
Migrator → Infrastructure (DbContext)
```

## 核心架构模式

### Autofac 注册模式
```csharp
// 每层有对应 Module：
// - ApplicationModule.cs (Application 层 Services)
// - InfrastructureModule.cs (Repositories, External Services)
// - AOP Interceptors 在 Module 中通过 EnableInterfaceInterceptors() 注册
```

### Repository 模式
```csharp
// 接口定义在 Domain/Interfaces/
// 实现在 Infrastructure/Persistence/Repositories/
// 复杂查询强制: AsNoTracking().AsSplitQuery()
```

### Mapster 使用规范
```csharp
// 1. 每个业务模块创建 XxxMappingConfig : IRegister
// 2. 在 DependencyInjection.cs 中调用:
//    services.AddMapster(); // 自动扫描所有 IRegister
// 3. 注入 IMapper 而非直接使用 TypeAdapter.Adapt<>()
```

### FluentValidation 使用规范
```csharp
// 每个 Request DTO 对应一个 Validator：
public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator() {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TotalSlots).GreaterThan(0).When(x => x.SlotMode == ProjectSlotMode.Fixed);
    }
}
// 通过 FluentValidation.AspNetCore 自动注册，返回 ValidationProblemDetails
```

## 数据库规范

### 命名规范
- 表名：`snake_case` 复数（`video_projects`, `project_slots`）
- 列名：`snake_case`（`created_at`, `investor_user_id`）
- 索引：`ix_{table}_{columns}`
- 外键：`fk_{table}_{ref_table}`

### EF Core 配置原则
```csharp
// 不使用 Data Annotations，所有配置通过 Fluent API
// 每个实体对应一个 IEntityTypeConfiguration<T> 实现
// decimal 精度必须显式配置:
builder.Property(x => x.Amount).HasPrecision(15, 2);
builder.Property(x => x.SharePermille).HasColumnType("decimal(8,4)");
```

## Docker 环境变量

### 境外实例（docker-compose.overseas.yml）
```yaml
ASPNETCORE_ENVIRONMENT: Production
ConnectionStrings__Default: "Host=postgres;..."
Redis__ConnectionString: "redis:6379"
Jwt__Secret: "${JWT_SECRET_OVERSEAS}"
Region__Name: "overseas"
Region__Currency: "USD"
Region__Language: "en-US"
InternalApi__DomesticBaseUrl: "http://domestic-api:8080"
InternalApi__Token: "${INTERNAL_API_TOKEN}"
Claude__ApiKey: "${CLAUDE_API_KEY}"
```

### 境内实例（docker-compose.domestic.yml）
```yaml
ASPNETCORE_ENVIRONMENT: Production
Region__Name: "domestic"
Region__Currency: "CNY"
Region__Language: "zh-CN"
InternalApi__Token: "${INTERNAL_API_TOKEN}"
InternalApi__AllowedIps__0: "${OVERSEAS_API_IP}"
```
