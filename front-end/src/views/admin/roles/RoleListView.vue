<template>
  <div class="p-6">
    <n-tabs v-model:value="activeTab" type="line" animated>
      <!-- ===== Tab 1: 角色管理 ===== -->
      <n-tab-pane name="roles" tab="角色管理">
        <div class="flex justify-end mb-4">
          <n-button type="primary" @click="openCreateRole">+ 新建角色</n-button>
        </div>

        <n-spin :show="rolesLoading">
          <n-empty v-if="!rolesLoading && roles.length === 0" description="暂无角色，点击右上角新建" class="py-16" />
          <n-grid v-else :cols="3" :x-gap="16" :y-gap="16" responsive="screen" item-responsive>
            <n-grid-item v-for="role in roles" :key="role.id" span="3 m:1">
              <n-card :title="role.name" hoverable size="small">
                <template #header-extra>
                  <n-space size="small">
                    <n-button size="tiny" @click="openEditRole(role)">编辑权限</n-button>
                    <n-popconfirm @positive-click="deleteRole(role.id)">
                      <template #trigger>
                        <n-button size="tiny" type="error" ghost>删除</n-button>
                      </template>
                      确认删除角色「{{ role.name }}」？
                    </n-popconfirm>
                  </n-space>
                </template>
                <div v-if="role.permissions.length === 0" class="text-sm" style="color:#999">无权限</div>
                <n-space v-else :wrap="true" size="small">
                  <n-tag v-for="p in role.permissions" :key="p" type="info" size="small">
                    {{ permLabel(p) }}
                  </n-tag>
                </n-space>
              </n-card>
            </n-grid-item>
          </n-grid>
        </n-spin>
      </n-tab-pane>

      <!-- ===== Tab 2: 内部账号 ===== -->
      <n-tab-pane name="users" tab="内部账号">
        <div class="flex justify-end mb-4">
          <n-button type="primary" @click="openCreateUser">+ 新建账号</n-button>
        </div>

        <n-spin :show="usersLoading">
          <n-data-table
            :columns="userColumns"
            :data="internalUsers"
            :loading="usersLoading"
            :pagination="false"
            size="small"
          />
        </n-spin>
      </n-tab-pane>
    </n-tabs>

    <!-- ===== 角色弹窗 ===== -->
    <n-modal
      v-model:show="roleModalVisible"
      :title="editingRole ? `编辑权限：${editingRole.name}` : '新建角色'"
      preset="card"
      style="width:580px"
      :mask-closable="false"
    >
      <n-form :model="roleForm" :rules="roleRules" ref="roleFormRef" label-placement="top">
        <n-form-item v-if="!editingRole" label="角色名称" path="name">
          <n-input v-model:value="roleForm.name" placeholder="例：财务审核员" />
        </n-form-item>
        <n-form-item label="菜单 / 操作权限">
          <div class="w-full">
            <div v-for="(perms, group) in groupedPermissions" :key="group" class="mb-3">
              <div class="text-sm font-medium mb-1" style="color:#666">{{ group }}</div>
              <n-space :wrap="true">
                <n-checkbox
                  v-for="p in perms"
                  :key="p.value"
                  :checked="roleForm.permissions.includes(p.value)"
                  @update:checked="(v: boolean) => togglePerm(p.value, v)"
                >{{ p.label }}</n-checkbox>
              </n-space>
            </div>
          </div>
        </n-form-item>
      </n-form>
      <template #footer>
        <n-space justify="end">
          <n-button @click="roleModalVisible = false">取消</n-button>
          <n-button type="primary" :loading="roleSaving" @click="saveRole">保存</n-button>
        </n-space>
      </template>
    </n-modal>

    <!-- ===== 内部账号弹窗 ===== -->
    <n-modal
      v-model:show="userModalVisible"
      title="新建内部账号"
      preset="card"
      style="width:520px"
      :mask-closable="false"
    >
      <n-form :model="userForm" :rules="userRules" ref="userFormRef" label-placement="top">
        <n-grid :cols="2" :x-gap="12">
          <n-form-item-gi label="姓名 / 昵称" path="displayName">
            <n-input v-model:value="userForm.displayName" placeholder="张三" />
          </n-form-item-gi>
          <n-form-item-gi label="用户名" path="username">
            <n-input v-model:value="userForm.username" placeholder="zhangsan" />
          </n-form-item-gi>
          <n-form-item-gi label="邮箱" path="email" span="2">
            <n-input v-model:value="userForm.email" placeholder="zhangsan@company.com" />
          </n-form-item-gi>
          <n-form-item-gi label="密码" path="password" span="2">
            <n-input v-model:value="userForm.password" type="password" show-password-on="click" placeholder="至少 8 位" />
          </n-form-item-gi>
          <n-form-item-gi label="账号类型" path="role" span="2">
            <n-radio-group v-model:value="userForm.role">
              <n-space>
                <n-radio value="BackendCustom">后台自定义角色</n-radio>
                <n-radio value="Sales">销售员</n-radio>
              </n-space>
            </n-radio-group>
          </n-form-item-gi>
          <n-form-item-gi v-if="userForm.role === 'BackendCustom'" label="绑定角色" path="backendRoleId" span="2">
            <n-select
              v-model:value="userForm.backendRoleId"
              :options="roleOptions"
              clearable
              placeholder="选择角色（可选）"
            />
          </n-form-item-gi>
        </n-grid>
      </n-form>
      <template #footer>
        <n-space justify="end">
          <n-button @click="userModalVisible = false">取消</n-button>
          <n-button type="primary" :loading="userSaving" @click="saveUser">创建</n-button>
        </n-space>
      </template>
    </n-modal>

    <!-- ===== 分配角色弹窗 ===== -->
    <n-modal
      v-model:show="assignModalVisible"
      :title="`分配角色：${assigningUser?.displayName}`"
      preset="card"
      style="width:400px"
      :mask-closable="false"
    >
      <n-form-item label="绑定后台角色">
        <n-select
          v-model:value="assignRoleId"
          :options="roleOptions"
          clearable
          placeholder="不绑定角色（仅 Sales 或无权限）"
        />
      </n-form-item>
      <template #footer>
        <n-space justify="end">
          <n-button @click="assignModalVisible = false">取消</n-button>
          <n-button type="primary" :loading="assignSaving" @click="saveAssign">保存</n-button>
        </n-space>
      </template>
    </n-modal>
  </div>
</template>

<script setup lang="ts">
import { h, ref, computed, onMounted } from 'vue'
import {
  NTabs, NTabPane, NButton, NCard, NGrid, NGridItem, NFormItemGi,
  NSpace, NTag, NEmpty, NSpin, NModal, NForm, NFormItem,
  NInput, NCheckbox, NRadioGroup, NRadio, NSelect, NDataTable,
  NPopconfirm, useMessage,
  type FormRules, type FormInst, type DataTableColumns,
} from 'naive-ui'
import {
  roleApi,
  ALL_PERMISSIONS,
  type BackendRoleDto,
  type InternalUserDto,
} from '../../../api/admin/role'

const message = useMessage()
const activeTab = ref('roles')

// ── 角色数据 ───────────────────────────────────────────────
const roles = ref<BackendRoleDto[]>([])
const rolesLoading = ref(false)
const roleModalVisible = ref(false)
const roleSaving = ref(false)
const editingRole = ref<BackendRoleDto | null>(null)
const roleFormRef = ref<FormInst | null>(null)
const roleForm = ref<{ name: string; permissions: string[] }>({ name: '', permissions: [] })
const roleRules: FormRules = {
  name: [{ required: true, message: '请输入角色名称', trigger: 'blur' }],
}

const groupedPermissions = computed(() => {
  const map: Record<string, typeof ALL_PERMISSIONS> = {}
  for (const p of ALL_PERMISSIONS) {
    if (!map[p.group]) map[p.group] = []
    map[p.group].push(p)
  }
  return map
})
const permLabel = (v: string) => ALL_PERMISSIONS.find(p => p.value === v)?.label ?? v

function togglePerm(value: string, checked: boolean) {
  if (checked) {
    if (!roleForm.value.permissions.includes(value)) roleForm.value.permissions.push(value)
  } else {
    roleForm.value.permissions = roleForm.value.permissions.filter(p => p !== value)
  }
}

async function loadRoles() {
  rolesLoading.value = true
  try {
    const res = await roleApi.getAll()
    roles.value = res.data.data ?? []
  } finally {
    rolesLoading.value = false
  }
}

function openCreateRole() {
  editingRole.value = null
  roleForm.value = { name: '', permissions: [] }
  roleModalVisible.value = true
}
function openEditRole(role: BackendRoleDto) {
  editingRole.value = role
  roleForm.value = { name: role.name, permissions: [...role.permissions] }
  roleModalVisible.value = true
}

async function saveRole() {
  try { await roleFormRef.value?.validate() } catch { return }
  roleSaving.value = true
  try {
    if (editingRole.value) {
      await roleApi.updatePermissions(editingRole.value.id, { permissions: roleForm.value.permissions })
      message.success('权限已更新')
    } else {
      await roleApi.create({ name: roleForm.value.name, permissions: roleForm.value.permissions })
      message.success('角色已创建')
    }
    roleModalVisible.value = false
    await loadRoles()
  } catch {
    message.error('保存失败，请重试')
  } finally {
    roleSaving.value = false
  }
}

async function deleteRole(id: string) {
  try {
    await roleApi.delete(id)
    message.success('角色已删除')
    await loadRoles()
  } catch {
    message.error('删除失败')
  }
}

// ── 内部账号 ──────────────────────────────────────────────
const internalUsers = ref<InternalUserDto[]>([])
const usersLoading = ref(false)
const userModalVisible = ref(false)
const userSaving = ref(false)
const userFormRef = ref<FormInst | null>(null)
const userForm = ref<{
  username: string; email: string; displayName: string; password: string; role: string; backendRoleId?: string
}>({ username: '', email: '', displayName: '', password: '', role: 'BackendCustom', backendRoleId: undefined })
const userRules: FormRules = {
  displayName: [{ required: true, message: '请输入姓名', trigger: 'blur' }],
  username:    [{ required: true, message: '请输入用户名', trigger: 'blur' }],
  email:       [{ required: true, type: 'email', message: '请输入有效邮箱', trigger: 'blur' }],
  password:    [{ required: true, min: 8, message: '密码至少 8 位', trigger: 'blur' }],
}

const roleOptions = computed(() => roles.value.map(r => ({ label: r.name, value: r.id })))

const userColumns: DataTableColumns<InternalUserDto> = [
  { title: '姓名', key: 'displayName' },
  { title: '用户名', key: 'username' },
  { title: '邮箱', key: 'email' },
  {
    title: '类型',
    key: 'role',
    render: (row) => row.role === 'Sales' ? '销售员' : '后台账号',
  },
  {
    title: '绑定角色',
    key: 'backendRoleName',
    render: (row) => row.backendRoleName
      ? h(NTag, { type: 'info', size: 'small' }, { default: () => row.backendRoleName })
      : h('span', { style: 'color:#999' }, '未绑定'),
  },
  {
    title: '状态',
    key: 'status',
    render: (row) => h(NTag, { type: row.status === 'Active' ? 'success' : 'default', size: 'small' }, {
      default: () => row.status === 'Active' ? '正常' : '禁用',
    }),
  },
  {
    title: '操作',
    key: 'actions',
    render: (row) => h(NButton, { size: 'tiny', onClick: () => openAssign(row) }, { default: () => '分配角色' }),
  },
]

async function loadUsers() {
  usersLoading.value = true
  try {
    const res = await roleApi.getInternalUsers()
    internalUsers.value = res.data.data ?? []
  } finally {
    usersLoading.value = false
  }
}

function openCreateUser() {
  userForm.value = { username: '', email: '', displayName: '', password: '', role: 'BackendCustom', backendRoleId: undefined }
  userModalVisible.value = true
}

async function saveUser() {
  try { await userFormRef.value?.validate() } catch { return }
  userSaving.value = true
  try {
    await roleApi.createInternalUser({
      ...userForm.value,
      backendRoleId: userForm.value.backendRoleId || undefined,
    })
    message.success('账号已创建')
    userModalVisible.value = false
    await loadUsers()
  } catch (e: any) {
    message.error(e?.response?.data?.message ?? '创建失败')
  } finally {
    userSaving.value = false
  }
}

// ── 分配角色 ──────────────────────────────────────────────
const assignModalVisible = ref(false)
const assignSaving = ref(false)
const assigningUser = ref<InternalUserDto | null>(null)
const assignRoleId = ref<string | null>(null)

function openAssign(user: InternalUserDto) {
  assigningUser.value = user
  assignRoleId.value = user.backendRoleId
  assignModalVisible.value = true
}

async function saveAssign() {
  if (!assigningUser.value) return
  assignSaving.value = true
  try {
    await roleApi.assignRole(assigningUser.value.id, { backendRoleId: assignRoleId.value })
    message.success('角色已更新')
    assignModalVisible.value = false
    await loadUsers()
  } catch {
    message.error('操作失败')
  } finally {
    assignSaving.value = false
  }
}

onMounted(async () => {
  await loadRoles()
  await loadUsers()
})
</script>