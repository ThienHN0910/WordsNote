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
          <button
            class="tab-btn"
            :class="{ 'active': activeTab === 'quizsets' }"
            @click="switchTab('quizsets')"
          >
            📚 Quản lý Bộ môn ({{ quizSets.length }})
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

        <!-- TAB 3: QUIZ SETS MANAGEMENT -->
        <div v-else-if="activeTab === 'quizsets'" class="tab-content fade-in">
          <div class="table-header">
            <h4 class="section-title">Danh sách Bộ câu hỏi / Môn học ({{ quizSets.length }})</h4>
            <div class="header-actions-group">
              <button class="btn-create-set" @click="openCreateSetModal">
                <i class="fa-solid fa-plus me-1"></i> Thêm môn học mới
              </button>
              <button class="refresh-btn" @click="fetchQuizSets"><i class="fa-solid fa-rotate me-1"></i> Làm mới</button>
            </div>
          </div>

          <div class="table-wrapper">
            <table class="data-table">
              <thead>
                <tr>
                  <th>Mã môn</th>
                  <th>Tên môn học</th>
                  <th>Số câu hỏi</th>
                  <th>Trạng thái truy cập</th>
                  <th>Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="s in quizSets" :key="s.id">
                  <td>
                    <span class="badge code-badge" :style="{ backgroundColor: s.color || '#2563eb' }">
                      {{ s.code }}
                    </span>
                  </td>
                  <td>
                    <div class="subject-info">
                      <strong class="subject-title">{{ s.title || s.name }}</strong>
                      <div v-if="s.description" class="subject-desc">{{ s.description }}</div>
                    </div>
                  </td>
                  <td>
                    <span class="badge count-badge">{{ s.totalQuestions || s.total || 0 }} câu</span>
                  </td>
                  <td>
                    <button
                      type="button"
                      class="toggle-lock-btn"
                      :class="s.isRestricted ? 'is-locked' : 'is-open'"
                      :title="s.isRestricted ? 'Bấm để mở tự do cho mọi người' : 'Bấm để khóa VIP'"
                      @click="onToggleRestriction(s)"
                    >
                      <i :class="s.isRestricted ? 'fa-solid fa-lock me-1' : 'fa-solid fa-lock-open me-1'"></i>
                      {{ s.isRestricted ? 'Khóa VIP' : 'Mở tự do' }}
                    </button>
                  </td>
                  <td>
                    <div class="action-btn-group">
                      <button class="edit-btn" title="Chỉnh sửa môn học" @click="openEditSetModal(s)">
                        <i class="fa-solid fa-pen-to-square"></i>
                      </button>
                      <button class="del-btn" title="Xóa môn học này" @click="onDeleteSet(s)">
                        <i class="fa-solid fa-trash"></i>
                      </button>
                    </div>
                  </td>
                </tr>
                <tr v-if="quizSets.length === 0">
                  <td colspan="5" class="empty-col">Chưa có môn học nào trong danh sách.</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Nested Modal: Create / Edit Quiz Set -->
      <div v-if="isSetModalOpen" class="nested-modal-backdrop" @click.self="closeSetModal">
        <div class="nested-modal-card glass-panel fade-in">
          <div class="nested-header">
            <h4>{{ isEditingSet ? '✏️ Chỉnh Sửa Môn Học' : '✨ Thêm Môn Học Mới' }}</h4>
            <button class="close-btn" @click="closeSetModal">&times;</button>
          </div>
          <div class="nested-body">
            <div class="form-group mb-3">
              <label class="field-label">ID Môn (Slug không dấu, vd: prn232, csd201):</label>
              <input
                v-model="setForm.id"
                type="text"
                class="form-control"
                placeholder="vd: prn232"
                :disabled="isEditingSet"
              />
            </div>
            <div class="form-group mb-3">
              <label class="field-label">Mã môn (Code hiển thị, vd: PRN232):</label>
              <input
                v-model="setForm.code"
                type="text"
                class="form-control"
                placeholder="vd: PRN232"
              />
            </div>
            <div class="form-group mb-3">
              <label class="field-label">Tên môn học (Title):</label>
              <input
                v-model="setForm.title"
                type="text"
                class="form-control"
                placeholder="vd: Lập trình .NET & Web API"
              />
            </div>
            <div class="form-group mb-3">
              <label class="field-label">Mô tả ngắn:</label>
              <textarea
                v-model="setForm.description"
                class="form-control text-area"
                rows="2"
                placeholder="Mô tả nội dung môn học..."
              ></textarea>
            </div>
            <div class="form-row mb-3">
              <div class="form-group flex-1">
                <label class="field-label">Màu sắc chủ đạo:</label>
                <div class="color-picker-wrapper">
                  <input v-model="setForm.color" type="color" class="color-picker" />
                  <code class="color-code">{{ setForm.color }}</code>
                </div>
              </div>
              <div class="form-group flex-1 checkbox-wrapper">
                <label class="checkbox-label">
                  <input v-model="setForm.isRestricted" type="checkbox" class="styled-checkbox" />
                  <span>🔒 Khóa VIP (Cần mã mở khóa)</span>
                </label>
              </div>
            </div>
          </div>
          <div class="nested-footer">
            <button class="btn-cancel" @click="closeSetModal">Hủy</button>
            <button class="submit-btn" :disabled="savingSet" @click="onSaveSet">
              <span v-if="savingSet" class="spinner-border spinner-border-sm me-1"></span>
              <i v-else class="fa-solid fa-check me-1"></i>
              {{ isEditingSet ? 'Lưu Thay Đổi' : 'Tạo Môn Học' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { QuizAPI } from '@/apis/WordsNote/QuizAPI'
import type { UnlockKeyItem, AdminUserItem, QuizUser, QuizSet } from '@/types/WordsNote'

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

const activeTab = ref<'keys' | 'users' | 'quizsets'>('keys')
const keys = ref<UnlockKeyItem[]>([])
const users = ref<AdminUserItem[]>([])
const quizSets = ref<QuizSet[]>([])
const customCode = ref('')
const batchCount = ref(1)
const targetOption = ref('restricted')

const loading = ref(false)
const genStatus = ref('')

const isSetModalOpen = ref(false)
const isEditingSet = ref(false)
const savingSet = ref(false)
const setForm = ref({
  id: '',
  code: '',
  title: '',
  description: '',
  color: '#2563eb',
  isRestricted: false
})

watch(
  () => [props.show, props.user?.isAdmin],
  ([newShow, isAdmin]) => {
    if (newShow && isAdmin) {
      genStatus.value = ''
      fetchKeys()
      fetchUsers()
      fetchQuizSets()
    }
  },
  { immediate: true }
)

function switchTab(tab: 'keys' | 'users' | 'quizsets') {
  activeTab.value = tab
  if (tab === 'keys') fetchKeys()
  if (tab === 'users') fetchUsers()
  if (tab === 'quizsets') fetchQuizSets()
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

async function fetchQuizSets() {
  if (!props.user?.isAdmin) return
  try {
    const res = await QuizAPI.getAdminQuizSets()
    quizSets.value = res.data?.sets || []
  } catch (err) {
    console.error('Failed to fetch admin quiz sets:', err)
  }
}

function openCreateSetModal() {
  isEditingSet.value = false
  setForm.value = {
    id: '',
    code: '',
    title: '',
    description: '',
    color: '#2563eb',
    isRestricted: false
  }
  isSetModalOpen.value = true
}

function openEditSetModal(set: QuizSet) {
  isEditingSet.value = true
  setForm.value = {
    id: set.id,
    code: set.code,
    title: set.title || set.name || '',
    description: set.description || '',
    color: set.color || '#2563eb',
    isRestricted: !!set.isRestricted
  }
  isSetModalOpen.value = true
}

function closeSetModal() {
  isSetModalOpen.value = false
}

async function onSaveSet() {
  if (!props.user?.isAdmin) return
  if (!setForm.value.id.trim() || !setForm.value.code.trim() || !setForm.value.title.trim()) {
    alert('Vui lòng nhập đầy đủ ID, Mã môn và Tên môn học.')
    return
  }

  savingSet.value = true
  try {
    if (isEditingSet.value) {
      await QuizAPI.updateQuizSet(setForm.value.id, {
        code: setForm.value.code,
        title: setForm.value.title,
        description: setForm.value.description,
        color: setForm.value.color,
        isRestricted: setForm.value.isRestricted
      })
      alert(`Đã cập nhật môn ${setForm.value.code} thành công!`)
    } else {
      await QuizAPI.createQuizSet({
        id: setForm.value.id,
        code: setForm.value.code,
        title: setForm.value.title,
        description: setForm.value.description,
        color: setForm.value.color,
        isRestricted: setForm.value.isRestricted
      })
      alert(`Đã tạo môn ${setForm.value.code} thành công!`)
    }
    closeSetModal()
    await fetchQuizSets()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Thao tác thất bại.')
  } finally {
    savingSet.value = false
  }
}

async function onToggleRestriction(set: QuizSet) {
  if (!props.user?.isAdmin) return
  const newRestricted = !set.isRestricted
  try {
    await QuizAPI.toggleQuizSetRestriction(set.id, newRestricted)
    set.isRestricted = newRestricted
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Cập nhật trạng thái khóa thất bại.')
  }
}

async function onDeleteSet(set: QuizSet) {
  if (!props.user?.isAdmin) return
  if (!confirm(`Bạn có chắc chắn muốn xóa môn ${set.code} - ${set.title || set.name}? Toàn bộ câu hỏi của môn này cũng sẽ bị xóa!`)) {
    return
  }
  try {
    await QuizAPI.deleteQuizSet(set.id)
    alert(`Đã xóa môn ${set.code} thành công!`)
    await fetchQuizSets()
  } catch (err: any) {
    alert(err?.response?.data?.message || 'Xóa môn học thất bại.')
  }
}

async function onGenerateKeys() {
  if (!props.user?.isAdmin) return
  loading.value = true
  genStatus.value = ''
  try {
    let targets = ['jfe301', 'jit401']
    if (targetOption.value === 'all') {
      targets = ['mln122', 'prm393', 'jfe301', 'jit401', 'prn232', 'ite302c', 'hcm202']
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

.header-actions-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.btn-create-set {
  padding: 5px 12px;
  border-radius: 6px;
  font-size: 0.78rem;
  font-weight: 600;
  border: none;
  background: #2563eb;
  color: #ffffff;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  transition: background 0.2s;
}

.btn-create-set:hover {
  background: #1d4ed8;
}

.code-badge {
  font-family: monospace;
  font-weight: 700;
  color: #ffffff;
  padding: 3px 8px;
  border-radius: 6px;
}

.subject-info {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.subject-title {
  color: var(--wn-ink, #0f172a);
  font-size: 0.88rem;
}

.subject-desc {
  font-size: 0.75rem;
  color: var(--wn-ink-muted, #64748b);
}

.count-badge {
  background: rgba(148, 163, 184, 0.15);
  color: var(--wn-ink, #0f172a);
  font-weight: 600;
}

.toggle-lock-btn {
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 0.76rem;
  font-weight: 600;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  transition: all 0.2s ease;
}

.toggle-lock-btn.is-locked {
  background: rgba(239, 68, 68, 0.15);
  border: 1px solid #ef4444;
  color: #dc2626;
}

.toggle-lock-btn.is-locked:hover {
  background: #ef4444;
  color: #ffffff;
}

.toggle-lock-btn.is-open {
  background: rgba(16, 185, 129, 0.15);
  border: 1px solid #10b981;
  color: #059669;
}

.toggle-lock-btn.is-open:hover {
  background: #10b981;
  color: #ffffff;
}

.action-btn-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.edit-btn {
  background: transparent;
  border: none;
  color: #2563eb;
  cursor: pointer;
  font-size: 0.88rem;
}

.edit-btn:hover {
  color: #1d4ed8;
}

/* Nested Modal for Create/Edit Quiz Set */
.nested-modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1200;
  padding: 16px;
}

.nested-modal-card {
  width: 100%;
  max-width: 480px;
  border-radius: 16px;
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.3);
  padding: 20px;
}

.nested-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  border-bottom: 1px solid var(--wn-card-border, #e2e8f0);
  padding-bottom: 10px;
}

.nested-header h4 {
  margin: 0;
  font-size: 1.05rem;
  font-weight: 700;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.mb-3 {
  margin-bottom: 12px;
}

.flex-1 {
  flex: 1;
}

.form-control {
  width: 100%;
  padding: 8px 12px;
  border-radius: 8px;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: var(--wn-card-bg, #ffffff);
  color: var(--wn-ink, #0f172a);
  font-size: 0.85rem;
}

.form-control:focus {
  outline: none;
  border-color: #2563eb;
}

.text-area {
  resize: vertical;
}

.color-picker-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
}

.color-picker {
  width: 38px;
  height: 38px;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 6px;
  cursor: pointer;
  padding: 2px;
  background: transparent;
}

.color-code {
  font-family: monospace;
  font-size: 0.85rem;
  font-weight: 600;
}

.checkbox-wrapper {
  display: flex;
  align-items: flex-end;
  padding-bottom: 6px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.84rem;
  cursor: pointer;
  user-select: none;
  font-weight: 600;
}

.styled-checkbox {
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.nested-footer {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 16px;
  padding-top: 12px;
  border-top: 1px solid var(--wn-card-border, #e2e8f0);
}

.btn-cancel {
  padding: 6px 14px;
  border-radius: 8px;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: transparent;
  color: var(--wn-ink, #0f172a);
  font-size: 0.84rem;
  cursor: pointer;
}

.btn-cancel:hover {
  background: rgba(148, 163, 184, 0.1);
}
</style>
