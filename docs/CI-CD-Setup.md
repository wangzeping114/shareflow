# GitHub Actions CI/CD 自动部署配置指南

推送代码到 `main` 分支后，GitHub 自动 SSH 进服务器执行 `git pull + docker compose up`，全程无需手动操作。

---

## 前置条件

- 服务器已可 SSH 登录（`ubuntu@43.129.23.249`）
- 服务器上 `/var/www/shareflow/.env` 已配置完成

---

## 第一步：在 GitHub 创建私有仓库

1. 打开 https://github.com/new
2. Repository name 填写 `shareflow`
3. 选择 **Private**（私有）
4. **不要**勾选任何初始化选项（README / .gitignore）
5. 点击 **Create repository**

---

## 第二步：本地推送代码到 GitHub

在本地 PowerShell 执行（替换 `你的GitHub用户名`）：

```powershell
cd e:\shareflow

# 初始化 git（若已初始化跳过）
git init
git add .
git commit -m "initial commit"

# 关联远程仓库并推送
git remote add origin https://github.com/你的GitHub用户名/shareflow.git
git branch -M main
git push -u origin main
```

> 会弹出 GitHub 登录窗口，登录授权即可。

---

## 第三步：服务器上 git clone 代码

SSH 进服务器执行：

```bash
ssh ubuntu@43.129.23.249
sudo -i

# 备份已上传的 .env 文件
cp /var/www/shareflow/.env /tmp/shareflow.env.bak

# 删除旧目录，改用 git clone
rm -rf /var/www/shareflow

# clone 仓库（替换为你的 GitHub 用户名）
cd /var/www
git clone https://github.com/你的GitHub用户名/shareflow.git
cd shareflow

# 恢复 .env 文件
cp /tmp/shareflow.env.bak .env

# 验证 .env 存在
cat .env
```

---

## 第四步：配置 GitHub Secrets

在 GitHub 仓库页面：

1. 点击 **Settings**（仓库设置，不是账号设置）
2. 左侧菜单点 **Secrets and variables** → **Actions**
3. 点击 **New repository secret**，依次添加以下 3 个：

| Secret 名称         | 值                |
| ------------------- | ----------------- |
| `SERVER_HOST`     | `43.129.23.249` |
| `SERVER_USER`     | `ubuntu`        |
| `SERVER_PASSWORD` | 服务器登录密码    |

---

## 第五步：手动触发第一次部署

1. 进入 GitHub 仓库页面
2. 点击顶部 **Actions** 标签
3. 左侧点击 **Deploy to Production**
4. 右侧点击 **Run workflow** → **Run workflow**

等待约 5-10 分钟（首次需构建 Docker 镜像），看到绿色 ✅ 表示部署成功。

---

## 日常使用方式

之后每次开发完成，只需在本地执行：

```powershell
git add .
git commit -m "描述本次修改"
git push origin main
```

GitHub Actions 自动触发，完成部署。可在 Actions 页面查看实时日志。

---

## 验证部署成功

部署完成后，在服务器上执行：

```bash
docker compose ps
# 所有服务应为 Up 状态

curl http://localhost:5001/health
# 返回 {"status":"Healthy"}

curl http://localhost:5002/health
# 返回 {"status":"Healthy"}
```

---

## 常见问题

### Actions 报错 `Host key verification failed`

SSH 首次连接需要信任服务器指纹，`appleboy/ssh-action` 默认跳过此步骤，正常情况不会出现。若出现，在 deploy.yml 中添加：

```yaml
        with:
          ...
          host_key: ""  # 跳过主机密钥检查
```

### 部署失败：`Permission denied`

服务器 `/var/www/shareflow` 目录需要 ubuntu 用户有写权限：

```bash
sudo chown -R ubuntu:ubuntu /var/www/shareflow
```

### 查看部署日志

GitHub 仓库 → Actions → 点击具体的 workflow run → 点击 `deploy` job 展开查看完整日志。
