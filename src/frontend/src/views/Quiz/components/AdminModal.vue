<template>
  <div v-if="show" class="modal-backdrop" @click.self="$emit('close')">
    <div class="modal-card glass-panel fade-in">
      <div class="modal-header">
        <div class="header-title">
          <span class="icon">👑</span>
          <h3>Quản Lý Admin (Mã Mở Khóa & Học Viên)</h3>
        </div>
        <button class="close-btn" @click="$emit('close')">&times;</button>
      </div>

      <!-- Password Auth Section Replaced by Google Admin Role Gate -->
      <div v-if="!user || !user.isAdmin" class="modal-body auth-notice-body">
        <div class="lock-shield-icon">🛡️</div>
        <h4 class="auth-notice-title">Khu Vực Dành Riêng Cho Admin</h4>
        <p v-if="!user" class="modal-desc">
          Vui lòng đăng nhập bằng tài khoản Google Quản trị viên (Admin) để truy cập hệ thống quản lý mã mở khóa và học viên.
        </p>
        <p v-else class="modal-desc error-text">
          Tài khoản <strong>{{ user.email }}</strong> không có quyền Quản trị viên. Chỉ tài khoản Admin hệ thống mới có thể truy cập khu vực này.
        </p>

        <div class="auth-actions">
          <button type="button" class="google-login-btn" @click="$emit('open-login')">
            <i class="fa-brands fa-google me-2"></i>
            {{ !user ? 'Đăng nhập Google Admin' : 'Đổi tài khoản Google' }}
          </button>
        </div>
      </div>

      <!-- Main Admin Panel (When Authenticated as Admin) -->
      <div v-else class="modal-body admin-panel">
        <!-- Navigation Tabs -->
        <div class="admin-tabs">
          <button
            class="tab-btn"
            :class="{ 'active': activeTab === 'keys' }"
            @click="switchTab('keys')"
          >
            🔑 Quản lý Mã mở khóa ({{ keys.length }})
          </button>
          <button
            class="tab-btn"
            :class="{ 'active': activeTab === 'users' }"
            @click="switchTab('users')"
          >
            👥 Danh sách Học viên ({{ users.length }})
          </button>
        </div>

        <!-- TAB 1: KEY MANAGEMENT -->
        <div v-if="activeTab === 'keys'" class="tab-content fade-in">
          <!-- 16-Character Key Generator Box -->
          <div class="generator-card">
            <h4 class="section-title"><i class="fa-solid fa-wand-magic-sparkles me-1 text-primary"></i> Tạo Mã 16 Ký Tự Mới</h4>
            <div class="gen-form">
              <div class="form-row">
                <input
                  v-model="customCode"
                  type="text"
                  placeholder="Mã tùy chọn (để trống để tự sinh 16 ký tự)..."
                  class="code-input"
                  maxlength="32"
                />
                <div class="select-wrapper">
                  <label class="field-label">Môn mở khóa:</label>
                  <select v-model="targetOption" class="select-input">
                    <option value="restricted">🔒 JFE301 & JIT401 (Mặc định)</option>
                    <option value="all">🌐 Tất cả 4 môn (MLN122, PRM393, JFE301, JIT401)</option>
                    <option value="jfe301">🟢 Chỉ môn JFE301</option>
                    <option value="jit401">🔴 Chỉ môn JIT401</option>
                    <option value="prm393">🟣 Chỉ môn PRM393</option>
                    <option value="mln122">📘 Chỉ môn MLN122</option>
                  </select>
                </div>
              </div>

              <div class="form-row action-row">
                <div class="batch-controls">
                  <label class="field-label">Số lượng tạo:</label>
                  <input v-model.number="batchCount" type="number" min="1" max="50" class="count-input" />
                </div>
                <button class="submit-btn" :disabled="loading" @click="onGenerateKeys">
                  <span v-if="loading" class="spinner-border spinner-border-sm me-1"></span>
                  <i v-else class="fa-solid fa-bolt me-1"></i>
                  Generate Mã (16 ký tự)
                </button>
              </div>
            </div>
            <p v-if="genStatus" class="success-msg">{{ genStatus }}</p>
          </div>

          <!-- Keys Table -->
          <div class="keys-list-container">
            <div class="table-header">
              <h4 class="section-title">Danh sách Mã mở khóa (Mỗi mã dùng 1 lần)</h4>
              <button class="refresh-btn" @click="fetchKeys"><i class="fa-solid fa-rotate me-1"></i> Làm mới</button>
            </div>

            <div class="table-wrapper">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Mã 16 Ký Tự</th>
                    <th>Môn Mở Khóa</th>
                    <th>Trạng Thái</th>
                    <th>Người Đã Sử Dụng</th>
                    <th>Thời Gian Dùng</th>
                    <th>Thao Tác</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="k in keys" :key="k.id || k._id || k.code">
                    <td>
                      <code class="key-code">{{ k.code }}</code>
                      <button class="copy-btn" title="Copy mã" @click="copyToClipboard(k.code)">
                        <i class="fa-regular fa-copy"></i>
                      </button>
                    </td>
                    <td>
                      <span v-for="sub in k.targetSubjects" :key="sub" class="badge sub-badge">
                        {{ sub.toUpperCase() }}
                      </span>
                    </td>
                    <td>
                      <span v-if="k.isUsed" class="badge used-badge">❌ Đã dùng</span>
                      <span v-else class="badge active-badge">✅ Khả dụng</span>
                    </td>
                    <td class="email-col">{{ k.usedByEmail || '—' }}</td>
                    <td class="date-col">{{ k.usedAt ? formatDate(k.usedAt) : '—' }}</td>
                    <td>
                      <button class="del-btn" title="Xóa mã này" @click="onDeleteKey(k.code || k.id)">
                        <i class="fa-solid fa-trash"></i>
                      </button>
                    </td>
                  </tr>
                  <tr v-if="keys.length === 0">
                    <td colspan="6" class="empty-col">Chưa có mã mở khóa nào trong database.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- TAB 2: USER MANAGEMENT -->
        <div v-else-if="activeTab === 'users'" class="tab-content fade-in">
          <div class="table-header">
            <h4 class="section-title">Danh sách Học viên đã đăng nhập ({{ users.length }})</h4>
            <button class="refresh-btn" @click="fetchUsers"><i class="fa-solid fa-rotate me-1"></i> Làm mới</button>
          </div>

          <div class="table-wrapper">
            <table class="data-table">
              <thead>
                <tr>
                  <th>Tài khoản</th>
                  <th>Email</th>
                  <th>Môn Đã Mở Khóa</th>
                  <th>Thao Tác Mở Môn</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="u in users" :key="u.id || u.email">
                  <td class="user-cell">
                    <img v-if="u.picture || u.avatarUrl" :src="u.picture || u.avatarUrl" :alt="u.name" class="avatar-img" />
                    <span v-else class="avatar-placeholder">{{ (u.name || u.email).charAt(0).toUpperCase() }}</span>
                    <span class="user-name-text">{{ u.name || 'Học viên' }}</span>
                    <span v-if="u.isAdmin" class="admin-crown-badge">👑 Admin</span>
                  </td>
                  <td><code class="email-code">{{ u.email }}</code></td>
                  <td>
                    <template v-if="u.unlockedSubjects && u.unlockedSubjects.length > 0">
                      <span v-for="sub in u.unlockedSubjects" :key="sub" class="badge unlocked-sub-badge">
                        🔓 {{ sub.toUpperCase() }}
                      </span>
                    </template>
                    <span v-else class="text-muted">Chưa mở môn khóa nào</span>
                  </td>
                  <td>
                    <button
                      type="button"
                      class="btn-grant"
                      title="Cấp quyền truy cập môn JFE301 & JIT401"
                      @click="onGrantUser(u.email, ['jfe301', 'jit401'])"
                    >
                      <i class="fa-solid fa-key me-1"></i> Cấp quyền JFE & JIT
                    </button>
                  </td>
                </tr>
                <tr v-if="users.length === 0">
                  <td colspan="4" class="empty-col">Chưa có học viên nào trong danh sách.</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { QuizAPI } from '@/apis/WordsNote/QuizAPI'
import type { UnlockKeyItem, AdminUserItem, QuizUser } from '@/types/WordsNote'

const props = withDefaults(
  defineProps<{
    show?: boolean
    user?: QuizUser | null
  }>(),
  {
    show: false,
    user: null
  }
)

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'open-login'): void
}>()

const activeTab = ref<'keys' | 'users'>('keys')
const keys = ref<UnlockKeyItem[]>([])
const users = ref<AdminUserItem[]>([])
const customCode = ref('')
const batchCount = ref(1)
const targetOption = ref('restricted')

const loading = ref(false)
const genStatus = ref('')

watch(
  () => [props.show, props.user?.isAdmin],
  ([newShow, isAdmin]) => {
    if (newShow && isAdmin) {
      genStatus.value = ''
      fetchKeys()
      fetchUsers()
    }
  },
  { immediate: true }
)

function switchTab(tab: 'keys' | 'users') {
  activeTab.value = tab
  if (tab === 'keys') fetchKeys()
  if (tab === 'users') fetchUsers()
}

async function fetchKeys() {
  if (!props.user?.isAdmin) return
  try {
    const res = await QuizAPI.getAdminKeys()
    keys.value = res.data?.keys || []
  } catch (err) {
    console.error('Failed to fetch keys:', err)
  }
}

async function fetchUsers() {
  if (!props.user?.isAdmin) return
  try {
    const res = await QuizAPI.getAdminUsers()
    users.value = res.data?.users || []
  } catch (err) {
    console.error('Failed to fetch users:', err)
  }
}

async function onGenerateKeys() {
  if (!props.user?.isAdmin) return
  loading.value = true
  genStatus.value = ''
  try {
    let targets = ['jfe301', 'jit401']
    if (targetOption.value === 'all') {
      targets = ['mln122', 'prm393', 'jfe301', 'jit401']
    } else if (targetOption.value !== 'restricted') {
      targets = [targetOption.value]
    }

    const res = await QuizAPI.generateAdminKeys({
      customCode: customCode.value.trim() || undefined,
      count: batchCount.value,
      targetSubjects: targets
    })

    genStatus.value = res.data?.message || 'Đã tạo mã thành công!'
    customCode.value = ''
    await fetchKeys()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Tạo mã thất bại.')
  } finally {
    loading.value = false
  }
}

async function onDeleteKey(codeOrId: string) {
  if (!props.user?.isAdmin) return
  if (!confirm(`Bạn có chắc muốn xóa mã ${codeOrId}?`)) return
  try {
    await QuizAPI.deleteAdminKey(codeOrId)
    await fetchKeys()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Xóa mã thất bại.')
  }
}

async function onGrantUser(email: string, subjects: string[]) {
  if (!props.user?.isAdmin) return
  try {
    await QuizAPI.grantUserAccess({ email, subjects })
    alert(`Đã cấp quyền mở khóa môn cho ${email}!`)
    await fetchUsers()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Cấp quyền thất bại.')
  }
}

function copyToClipboard(text: string) {
  navigator.clipboard.writeText(text)
  alert(`Đã copy mã: ${text}`)
}

function formatDate(isoStr: string) {
  try {
    return new Date(isoStr).toLocaleString('vi-VN')
  } catch {
    return isoStr
  }
}
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.75);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1100;
  padding: 16px;
}

.modal-card {
  width: 100%;
  max-width: 900px;
  max-height: 90vh;
  border-radius: 16px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.25);
}

.modal-header {
  padding: 16px 20px;
  border-bottom: 1px solid var(--wn-card-border, #e2e8f0);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.header-title {
  display: flex;
  align-items: center;
  gap: 10px;
}

.header-title h3 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 700;
}

.close-btn {
  background: transparent;
  border: none;
  font-size: 1.5rem;
  color: var(--wn-ink-muted, #64748b);
  cursor: pointer;
}

.modal-body {
  padding: 20px;
  overflow-y: auto;
  flex: 1;
}

.input-group-custom {
  display: flex;
  gap: 8px;
  margin-top: 12px;
}

.code-input {
  flex: 1;
  height: 40px;
  padding: 0 12px;
  border-radius: 8px;
  border: 1.5px solid var(--wn-card-border, #e2e8f0);
  background: var(--wn-card-bg, #ffffff);
  color: var(--wn-ink, #0f172a);
  font-size: 0.9rem;
  outline: none;
}

.submit-btn {
  padding: 0 18px;
  background: #2563eb;
  border: none;
  border-radius: 8px;
  color: #fff;
  font-weight: 600;
  font-size: 0.88rem;
  cursor: pointer;
}

.auth-notice-body {
  text-align: center;
  padding: 40px 24px;
}

.lock-shield-icon {
  font-size: 3.5rem;
  margin-bottom: 12px;
}

.auth-notice-title {
  font-size: 1.25rem;
  font-weight: 700;
  margin-bottom: 8px;
  color: var(--wn-ink, #0f172a);
}

.error-text {
  color: #ef4444;
}

.auth-actions {
  margin-top: 24px;
  display: flex;
  justify-content: center;
}

.google-login-btn {
  display: inline-flex;
  align-items: center;
  padding: 10px 24px;
  border-radius: 999px;
  border: 1.5px solid var(--wn-card-border, #e2e8f0);
  background: var(--wn-card-bg, #ffffff);
  color: var(--wn-ink, #0f172a);
  font-weight: 600;
  font-size: 0.92rem;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.google-login-btn:hover {
  background: var(--wn-border-subtle, #f8fafc);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
}

.admin-tabs {
  display: flex;
  gap: 8px;
  border-bottom: 1px solid var(--wn-card-border, #e2e8f0);
  padding-bottom: 12px;
  margin-bottom: 18px;
}

.tab-btn {
  padding: 8px 16px;
  font-size: 0.86rem;
  font-weight: 600;
  border-radius: 8px;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: transparent;
  color: var(--wn-ink-muted, #64748b);
  cursor: pointer;
  transition: all 0.15s ease;
}

.tab-btn.active {
  background: #2563eb;
  border-color: #2563eb;
  color: #fff;
}

.generator-card {
  padding: 16px;
  border-radius: 12px;
  background: rgba(148, 163, 184, 0.08);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  margin-bottom: 20px;
}

.section-title {
  font-size: 0.95rem;
  font-weight: 700;
  margin: 0 0 12px;
}

.gen-form {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.form-row {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  align-items: center;
}

.select-wrapper {
  display: flex;
  align-items: center;
  gap: 6px;
}

.field-label {
  font-size: 0.82rem;
  color: var(--wn-ink-muted, #64748b);
  font-weight: 600;
}

.select-input {
  height: 38px;
  padding: 0 10px;
  border-radius: 8px;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: var(--wn-card-bg, #ffffff);
  color: var(--wn-ink, #0f172a);
  font-size: 0.85rem;
}

.action-row {
  justify-content: space-between;
}

.batch-controls {
  display: flex;
  align-items: center;
  gap: 6px;
}

.count-input {
  width: 60px;
  height: 38px;
  border-radius: 8px;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: var(--wn-card-bg, #ffffff);
  color: var(--wn-ink, #0f172a);
  text-align: center;
}

.table-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
}

.refresh-btn {
  padding: 4px 10px;
  border-radius: 6px;
  font-size: 0.78rem;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: transparent;
  color: var(--wn-ink, #0f172a);
  cursor: pointer;
}

.table-wrapper {
  overflow-x: auto;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 8px;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.82rem;
}

.data-table th,
.data-table td {
  padding: 10px 12px;
  border-bottom: 1px solid var(--wn-card-border, #e2e8f0);
  text-align: left;
}

.data-table th {
  background: rgba(148, 163, 184, 0.1);
  font-weight: 700;
  color: var(--wn-ink-muted, #64748b);
}

.key-code {
  font-family: monospace;
  font-weight: 700;
  color: #2563eb;
  padding: 2px 6px;
  border-radius: 4px;
  background: rgba(37, 99, 235, 0.1);
}

.copy-btn {
  background: transparent;
  border: none;
  margin-left: 6px;
  cursor: pointer;
  color: var(--wn-ink-muted, #64748b);
}

.copy-btn:hover {
  color: #2563eb;
}

.del-btn {
  background: transparent;
  border: none;
  color: #ef4444;
  cursor: pointer;
}

.del-btn:hover {
  color: #dc2626;
}

.badge {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 999px;
  font-size: 0.74rem;
  font-weight: 600;
}

.sub-badge {
  background: rgba(148, 163, 184, 0.2);
  color: var(--wn-ink, #0f172a);
  margin-right: 4px;
}

.active-badge {
  background: rgba(16, 185, 129, 0.15);
  color: #059669;
}

.used-badge {
  background: rgba(239, 68, 68, 0.15);
  color: #dc2626;
}

.unlocked-sub-badge {
  background: rgba(16, 185, 129, 0.15);
  color: #059669;
  margin-right: 4px;
}

.user-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}

.avatar-img {
  width: 26px;
  height: 26px;
  border-radius: 50%;
}

.avatar-placeholder {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  background: #2563eb;
  color: #fff;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 0.75rem;
}

.admin-crown-badge {
  font-size: 0.7rem;
  background: rgba(245, 158, 11, 0.2);
  color: #d97706;
  padding: 1px 6px;
  border-radius: 4px;
  font-weight: 700;
}

.btn-grant {
  padding: 4px 8px;
  font-size: 0.78rem;
  border-radius: 6px;
  border: 1px solid #10b981;
  background: rgba(16, 185, 129, 0.1);
  color: #059669;
  cursor: pointer;
}

.btn-grant:hover {
  background: #10b981;
  color: #fff;
}

.empty-col {
  text-align: center;
  padding: 24px;
  color: var(--wn-ink-muted, #64748b);
}

.error-msg {
  color: #dc2626;
  font-size: 0.85rem;
}

.success-msg {
  color: #059669;
  font-size: 0.85rem;
  font-weight: 600;
}
</style>
