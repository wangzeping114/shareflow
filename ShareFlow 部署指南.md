# ShareFlow 部署指南

香港云服务器 × Cloudflare CDN & DNS × Docker Compose

| **文档版本**   | v2.0                              |
| -------------- | --------------------------------- |
| **适用环境**   | 生产环境（Production）            |
| **服务器地区** | 香港（腾讯云 / 阿里云，推荐）     |
| **操作系统**   | Ubuntu 22.04 LTS                  |
| **运行时**     | Docker 26 + Docker Compose v2     |
| **编写日期**   | 2026 年 5 月                      |

> **💡 阅读须知：** 本文档面向具备基础 Linux 操作经验的开发者。全程约需 60-90 分钟完成首次部署。ShareFlow 采用 Docker Compose 管理所有服务，无需手动安装 .NET / PostgreSQL / Redis。

---

# 一、服务商注册与选型

## 1.1 为什么选香港服务器

ShareFlow 的 Admin 后台和 Sales 销售端均为**国内用户每日高频使用**。香港节点对大陆延迟仅 5-20ms，远优于日本东京（~60ms），且无需备案。

| 角色 | 用户位置 | 延迟（香港节点） |
|------|---------|----------------|
| Admin 后台 | 国内 | 5-20ms ✅ |
| Sales 销售端 | 国内 | 5-20ms ✅ |
| 境内 Client 客户端 | 国内 | 5-20ms ✅ |
| 境外 Client 客户端 | 海外 | 50-80ms ✅ |

## 1.2 推荐服务商（二选一）

### 方案 A：腾讯云香港（推荐，性价比最高）

**注册地址：** https://cloud.tencent.com

1. 注册账号（支持微信/QQ 登录，人民币付款）
2. 进入「轻量应用服务器」→「新建」
3. 选择配置：

| **地域** | 香港 |
|----------|------|
| **镜像** | Ubuntu 22.04 LTS |
| **套餐** | 4 核 8GB · 100GB SSD · 10Mbps |
| **月费** | ¥220-280/月 |

### 方案 B：阿里云香港

**注册地址：** https://www.aliyun.com

1. 注册账号（支付宝实名认证）
2. 进入「云服务器 ECS」→「立即购买」
3. 选择地域「中国香港」，规格 `ecs.c7.xlarge`（4核8GB）

> ⚠️ **最低配置要求：** ShareFlow 运行 .NET 8 API × 2 + PostgreSQL × 2 + Redis × 2 + MinIO，**至少需要 4GB RAM**，推荐 8GB。

购买后在控制台记录服务器**公网 IP**（格式如 `43.xxx.xxx.xxx`）。

## 1.3 域名注册

**推荐：Cloudflare Registrar（成本价，无溢价）**

**注册地址：** https://www.cloudflare.com/products/registrar/

- 注册后直接在 Cloudflare 购买域名，`.com` 约 $10/年（约 ¥72）
- 省去迁移 NS 的步骤，DNS 管理一体化

**备选：Namecheap**

**注册地址：** https://www.namecheap.com

- `.com` 首年约 $6-8，支持信用卡/PayPal

> 域名规划建议（将 `shareflow.com` 替换为你的实际域名）：
> - `shareflow.com` → 前端（Vue 3）
> - `api.shareflow.com` → 境外 API（overseas 实例）
> - `api-cn.shareflow.com` → 境内 API（domestic 实例，Admin + Sales + 境内 Client）

---

# 二、服务器初始化

## 2.1 SSH 连接服务器

```bash
# Windows 使用 PowerShell，Mac/Linux 直接用终端
ssh root@43.xxx.xxx.xxx

# 首次登录后立即修改 root 密码
passwd
```

## 2.2 系统更新与基础软件

```bash
apt update && apt upgrade -y
apt install -y curl git wget unzip ufw
```

## 2.3 配置防火墙（UFW）

```bash
ufw allow OpenSSH
ufw allow 80    # HTTP
ufw allow 443   # HTTPS
ufw enable
ufw status
```

> 数据库（PostgreSQL）、Redis、MinIO 端口**不对外开放**，仅 Docker 内网互通。

## 2.4 安装 Docker

```bash
# 使用 Docker 官方安装脚本
curl -fsSL https://get.docker.com | sh

# 验证安装
docker --version          # 应输出 Docker version 26.x.x
docker compose version    # 应输出 Docker Compose version v2.x.x

# 设置 Docker 开机自启
systemctl enable docker
systemctl start docker
```

---

# 三、配置环境变量（安全关键）

> ⚠️ **生产环境所有密码必须通过 `.env` 文件注入，`.env` 文件禁止提交到 Git 仓库。**

## 3.1 上传项目代码

```bash
cd /var/www
git clone https://github.com/你的账号/shareflow.git
cd shareflow
```

## 3.2 创建 `.env` 生产配置文件

```bash
cp .env.example .env
nano .env
```

按以下格式填写（**所有密码替换为强随机字符串**）：

```dotenv
# 生成强密码命令：openssl rand -base64 32

# PostgreSQL
POSTGRES_USER=shareflow
POSTGRES_PASSWORD=在此填写强密码至少20位
POSTGRES_DB_OVERSEAS=shareflow_overseas
POSTGRES_DB_DOMESTIC=shareflow_domestic

# Redis
REDIS_PASSWORD=在此填写强密码至少20位

# MinIO 对象存储
MINIO_ROOT_USER=shareflow-admin
MINIO_ROOT_PASSWORD=在此填写强密码至少20位

# JWT 签名密钥（至少 32 位）
JWT_SECRET=在此填写64位随机字符串
```

---

# 四、安装 Nginx 反向代理

## 4.1 安装 Nginx

```bash
apt install -y nginx
systemctl enable nginx
```

## 4.2 创建站点配置

```bash
nano /etc/nginx/sites-available/shareflow
```

粘贴以下配置（将 `shareflow.com` 替换为实际域名）：

```nginx
# 前端（Vue 3）
server {
    listen 80;
    server_name shareflow.com www.shareflow.com;

    location / {
        proxy_pass http://localhost:5173;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_cache_bypass $http_upgrade;
    }
}

# 境外 API（overseas 实例，供境外 Client）
server {
    listen 80;
    server_name api.shareflow.com;

    location / {
        proxy_pass http://localhost:5001;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

# 境内 API（domestic 实例，Admin + Sales + 境内 Client）
server {
    listen 80;
    server_name api-cn.shareflow.com;

    location / {
        proxy_pass http://localhost:5002;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
# 启用配置
ln -s /etc/nginx/sites-available/shareflow /etc/nginx/sites-enabled/
nginx -t            # 测试配置语法（应输出 syntax is ok）
systemctl reload nginx
```

---

# 五、启动 ShareFlow 应用

## 5.1 构建并启动所有容器

```bash
cd /var/www/shareflow

# 首次启动（构建镜像 + 启动容器，约 5-10 分钟）
docker compose up -d --build

# 查看所有容器状态（所有服务应为 Up）
docker compose ps
```

## 5.2 执行数据库迁移

```bash
# 境外实例迁移
docker compose exec shareflow-api-overseas dotnet ShareFlow.Migrator.dll

# 境内实例迁移
docker compose exec shareflow-api-domestic dotnet ShareFlow.Migrator.dll
```

## 5.3 查看日志

```bash
docker compose logs -f shareflow-api-overseas shareflow-api-domestic
```

---

# 六、Cloudflare 配置

## 6.1 注册并添加域名

1. 访问 https://dash.cloudflare.com 注册免费账号
2. 点击「Add a Site」输入域名，选择「Free」套餐
3. Cloudflare 自动扫描 DNS 记录，确认后继续
4. 将域名注册商的 NS 记录替换为 Cloudflare 提供的两个地址
5. 等待 5-30 分钟，控制台显示「Active」即生效

## 6.2 配置 DNS 解析（A 记录）

在 Cloudflare DNS 管理页添加以下记录（IP 替换为实际服务器 IP）：

| **类型** | **名称** | **内容** | **代理状态** |
|----------|----------|----------|-------------|
| A | @ | 43.xxx.xxx.xxx | 🟠 已代理（Proxied） |
| A | www | 43.xxx.xxx.xxx | 🟠 已代理（Proxied） |
| A | api | 43.xxx.xxx.xxx | 🟠 已代理（Proxied） |
| A | api-cn | 43.xxx.xxx.xxx | 🟠 已代理（Proxied） |

## 6.3 配置 SSL/TLS

1. 进入「SSL/TLS」→「Overview」，选择「**Full**」模式
2. 进入「SSL/TLS」→「Edge Certificates」：
   - 开启「Always Use HTTPS」
   - 开启「Automatic HTTPS Rewrites」

## 6.4 性能与安全优化

| **功能** | **推荐设置** | **说明** |
|----------|------------|---------|
| **Brotli 压缩** | ✅ 开启 | 压缩率优于 gzip |
| **Browser Cache TTL** | 4 小时 | 静态资源缓存 |
| **Security Level** | Medium | 拦截可疑流量 |
| **Bot Fight Mode** | ✅ 开启 | 抵御爬虫攻击 |
| **HTTP/2 & HTTP/3** | ✅ 开启（默认） | 提升并行加载速度 |
| **WebSocket** | ✅ 开启 | 「Network」→「WebSockets」，支持实时通知 |

---

# 七、验证部署

## 7.1 服务器端检查

```bash
# 确认所有容器运行正常
docker compose ps
# 预期：所有服务 Status 为 Up，postgres 服务 Health 为 healthy

# 测试境外 API
curl http://localhost:5001/health
# 预期：{"status":"Healthy"}

# 测试境内 API
curl http://localhost:5002/health
# 预期：{"status":"Healthy"}

# 检查 Nginx 状态
systemctl status nginx
```

## 7.2 浏览器验证

- 访问 `https://shareflow.com` — 前端页面正常加载，地址栏显示 🔒
- DevTools → Network 响应头包含 `cf-ray`（流量已经过 Cloudflare）
- 访问 `https://api.shareflow.com/health` — 境外 API 正常
- 访问 `https://api-cn.shareflow.com/health` — 境内 API 正常

## 7.3 常见问题排查

### ❌ 容器启动失败

```bash
docker compose logs shareflow-api-overseas
# 检查是否有「数据库连接失败」或「环境变量缺失」错误
```

### ❌ 502 Bad Gateway

```bash
docker compose ps          # 确认容器在运行
curl http://localhost:5001  # 直接测试容器是否响应
ufw status                 # 确认防火墙放行 80/443
```

### ❌ 域名无法访问

- 确认 Cloudflare DNS 状态为 Active
- 确认 A 记录 IP 与服务器 IP 一致
- DNS 生效等待 10-30 分钟

### ❌ SSL 证书错误

- Cloudflare SSL 模式选「Full」而非「Full（Strict）」
- 确认「Always Use HTTPS」已开启

---

# 八、日常维护

## 8.1 更新部署

```bash
cd /var/www/shareflow
git pull origin main
docker compose up -d --build
docker compose logs -f --tail=50
```

## 8.2 数据库备份

```bash
# 备份境外数据库
docker compose exec postgres-overseas pg_dump -U shareflow shareflow_overseas \
  > backup_overseas_$(date +%Y%m%d).sql

# 备份境内数据库
docker compose exec postgres-domestic pg_dump -U shareflow shareflow_domestic \
  > backup_domestic_$(date +%Y%m%d).sql
```

建议配置 cron 每日自动备份，上传至 Cloudflare R2 对象存储。

## 8.3 资源监控

```bash
# 查看各容器 CPU/内存占用
docker stats

# 查看磁盘占用
df -h
docker system df
```

---

# 九、费用估算

| **项目** | **月费** | **备注** |
|----------|---------|---------|
| 腾讯云香港轻量 4核8GB | ¥220-280 | 运行全部 Docker 服务 |
| Cloudflare CDN & DNS | 免费 | Free 套餐 |
| Cloudflare SSL 证书 | 免费 | 自动签发续期 |
| 域名（Cloudflare Registrar） | ¥6/月 | 约 ¥72/年 |
| **合计（月均）** | **约 ¥226-286/月** | |

---

✅ **部署完成：** ShareFlow 已成功部署在香港云服务器 + Cloudflare 上。境外 Client 访问 `api.shareflow.com`（overseas 实例），Admin / Sales / 境内 Client 访问 `api-cn.shareflow.com`（domestic 实例），国内外均可流畅访问。
