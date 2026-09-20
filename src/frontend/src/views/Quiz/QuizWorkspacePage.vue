<template>
  <div class="quiz-app-container">
    <!-- Top Header Navbar -->
    <HeaderNavbar
      :subjects="quizStore.subjects"
      :active-subject-id="quizStore.activeSubjectId"
      :user="quizStore.user"
      :is-dark="themeStore.isDark"
      @select-subject="onSelectSubject"
      @open-search="showSearchDrawer = true"
      @open-admin="showAdminModal = true"
      @open-login="showLoginModal = true"
      @toggle-theme="toggleTheme"
      @logout="onLogout"
    />

    <!-- Main Workspace 2-Column Layout -->
    <div class="workspace-grid">
      <!-- Main Content Column -->
      <main class="main-col">
        <!-- Controls Toolbar -->
        <QuizControls
          :pos="quizStore.pos"
          :queue-length="quizStore.queue.length"
          :total-questions="quizStore.questions.length"
          :current-mode="quizStore.currentMode"
          :current-source="quizStore.currentSource"
          :stats="quizStore.stats"
          :has-source-filters="hasSourceFilters"
          @jump="onJump"
          @set-mode="quizStore.setMode"
          @set-source="quizStore.setSourceFilter"
        />

        <!-- Loading State -->
        <div v-if="quizStore.loadingData" class="state-card glass-panel fade-in">
          <div class="spinner-border text-primary mb-3" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
          <p class="state-text">Đang tải dữ liệu câu hỏi môn {{ activeSubjectCode }}...</p>
        </div>

        <!-- Locked Subject State -->
        <div v-else-if="quizStore.isLocked" class="state-card glass-panel fade-in lock-card">
          <div class="lock-illustration">🔒</div>
          <h3 class="lock-title">Môn {{ activeSubjectCode }} đang bị khóa</h3>
          <p class="lock-desc">
            Môn học này yêu cầu <strong>Mã mở khóa 1 lần</strong> liên kết với tài khoản của bạn để truy cập bài học và đáp án.
          </p>

          <div class="lock-actions">
            <button v-if="!quizStore.user" type="button" class="btn btn-primary" @click="showLoginModal = true">
              <i class="fa-brands fa-google me-1"></i> Đăng nhập bằng Google
            </button>
            <button v-else type="button" class="btn btn-success" @click="showUnlockModal = true">
              <i class="fa-solid fa-key me-1"></i> Nhập mã mở khóa (16 ký tự)
            </button>
          </div>
        </div>

        <!-- Question Card View -->
        <QuestionCard
          v-else-if="quizStore.currentQuestion"
          :question="quizStore.currentQuestion"
          :pos-index="quizStore.pos"
          :queue-length="quizStore.queue.length"
          :selected-keys="quizStore.selectedKeys"
          :is-checked="quizStore.isChecked"
          :is-revealed="quizStore.isRevealed"
          :is-starred="quizStore.isStarred(quizStore.currentQuestion.id)"
          @select-option="quizStore.selectOption"
          @toggle-star="quizStore.toggleStar"
          @check="quizStore.checkMultiChoice"
          @toggle-reveal="quizStore.toggleReveal"
          @prev="quizStore.prev"
          @next="quizStore.next"
          @save-word="openAddWordModal"
        />

        <!-- Empty Queue State -->
        <div v-else class="state-card glass-panel fade-in">
          <div class="empty-illustration">🎉</div>
          <h3 class="empty-title">Không có câu hỏi nào trong danh sách {{ quizStore.modeLabel }}</h3>
          <p class="empty-desc">Bạn đã hoàn thành tất cả câu hỏi trong chế độ này hoặc chưa có câu hỏi nào thỏa mãn điều kiện lọc.</p>
          <button type="button" class="btn btn-primary" @click="quizStore.setMode('seq')">
            <i class="fa-solid fa-rotate-left me-1"></i> Quay lại Tất cả câu hỏi
          </button>
        </div>
      </main>

      <!-- Right Side Question Map Sidebar -->
      <SideQuestionMap
        :questions="quizStore.questions"
        :current-question-id="quizStore.currentQuestion?.id"
        :progress="quizStore.progress"
        @select-question="onSelectQuestionFromMap"
      />
    </div>

    <!-- Modals -->
    <UnlockModal
      :show="showUnlockModal"
      @close="showUnlockModal = false"
      @redeem-key="onRedeemKey"
    />

    <AdminModal
      :show="showAdminModal"
      :user="quizStore.user"
      @close="showAdminModal = false"
    />

    <SearchDrawer
      :show="showSearchDrawer"
      :questions="quizStore.questions"
      @close="showSearchDrawer = false"
      @select-question="onSelectQuestionFromSearch"
    />

    <GoogleLoginModal
      :show="showLoginModal"
      @close="showLoginModal = false"
      @login-success="onGoogleLoginSuccess"
      @login-error="onGoogleLoginError"
    />

    <!-- Quick Word Save Modal (SRS Flashcards) -->
    <div v-if="showAddWordModal" class="modal-backdrop-custom" @click.self="showAddWordModal = false">
      <div class="custom-dialog glass-panel fade-in">
        <div class="custom-dialog-header">
          <h5><i class="fa-solid fa-bookmark text-primary me-2"></i>Lưu từ vựng vào Deck Flashcard</h5>
          <button type="button" class="btn-close-custom" @click="showAddWordModal = false">&times;</button>
        </div>

        <div class="custom-dialog-body">
          <div class="mb-3 text-start">
            <label class="form-label fw-bold small text-muted">Bộ thẻ (Deck)</label>
            <select v-model="selectedDeckId" class="form-select custom-select">
              <option v-for="d in decks" :key="d.id" :value="d.id">{{ d.title }}</option>
            </select>
          </div>

          <div class="mb-3 text-start">
            <label class="form-label fw-bold small text-muted">Mặt trước (Từ vựng / Thuật ngữ)</label>
            <input v-model="newWordFront" type="text" class="form-control custom-input" placeholder="Ví dụ: Quy luật giá trị" />
          </div>

          <div class="mb-3 text-start">
            <label class="form-label fw-bold small text-muted">Mặt sau (Định nghĩa / Ghi chú)</label>
            <textarea v-model="newWordBack" class="form-control custom-input" rows="3" placeholder="Định nghĩa hoặc ghi nhớ"></textarea>
          </div>
        </div>

        <div class="custom-dialog-footer">
          <button type="button" class="btn btn-sm btn-outline-secondary" @click="showAddWordModal = false">Hủy</button>
          <button
            type="button"
            class="btn btn-sm btn-primary"
            :disabled="!newWordFront.trim() || !selectedDeckId"
            @click="saveWordToDeck"
          >
            Lưu vào Flashcards
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useQuizStore } from '@/stores/WordsNote/QuizStore'
import { useThemeStore } from '@/stores/CFS/ThemeSettingStore'
import { StudyAPI } from '@/apis/WordsNote/StudyAPI'
import type { StudyDeck } from '@/types/WordsNote'

import HeaderNavbar from './components/HeaderNavbar.vue'
import QuizControls from './components/QuizControls.vue'
import SideQuestionMap from './components/SideQuestionMap.vue'
import QuestionCard from './components/QuestionCard.vue'
import UnlockModal from './components/UnlockModal.vue'
import AdminModal from './components/AdminModal.vue'
import SearchDrawer from './components/SearchDrawer.vue'
import GoogleLoginModal from './components/GoogleLoginModal.vue'

const route = useRoute()
const router = useRouter()
const quizStore = useQuizStore()
const themeStore = useThemeStore()

// Modals State
const showUnlockModal = ref(false)
const showAdminModal = ref(false)
const showSearchDrawer = ref(false)
const showLoginModal = ref(false)

// Flashcard Add Word Modal State
const showAddWordModal = ref(false)
const desks = ref<StudyDeck[]>([])
const selectedDeckId = ref('')
const newWordFront = ref('')
const newWordBack = ref('')

const activeSubjectCode = computed(() => {
  const current = quizStore.subjects.find((s) => s.id === quizStore.activeSubjectId)
  return current?.code || quizStore.activeSubjectId.toUpperCase()
})

const hasSourceFilters = computed(() => {
  return ['prm393', 'jfe301', 'jit401', 'mln122'].includes(quizStore.activeSubjectId.toLowerCase())
})

onMounted(async () => {
  // Load decks for flashcard capture
  loadStudyDecks()

  // Initialize catalog and questions
  await quizStore.fetchCatalog()

  const subFromRoute = (route.params.id as string)?.toLowerCase()
  const initialSub = subFromRoute && ['mln122', 'prm393', 'jfe301', 'jit401'].includes(subFromRoute)
    ? subFromRoute
    : 'mln122'

  await quizStore.fetchQuestions(initialSub)

  // Register Global Keyboard Shortcuts
  window.addEventListener('keydown', handleGlobalKeydown)
})

onUnmounted(() => {
  window.removeEventListener('keydown', handleGlobalKeydown)
})

// Watch route parameter changes
watch(
  () => route.params.id,
  (newId) => {
    if (newId && typeof newId === 'string' && newId.toLowerCase() !== quizStore.activeSubjectId) {
      quizStore.selectSubject(newId.toLowerCase())
    }
  }
)

function onSelectSubject(subId: string) {
  router.push(`/quiz/${subId}`)
  quizStore.selectSubject(subId)
}

function onJump(targetNum: number) {
  quizStore.jump(targetNum)
}

function onSelectQuestionFromMap(qIdOrNum: string | number) {
  if (typeof qIdOrNum === 'number') {
    quizStore.jump(qIdOrNum)
  } else {
    const num = parseInt(String(qIdOrNum).split('-').pop() || '1', 10)
    quizStore.jump(num)
  }
}

function onSelectQuestionFromSearch(qId: string | number) {
  quizStore.setMode('seq')
  onSelectQuestionFromMap(qId)
}

function toggleTheme() {
  themeStore.isDark = !themeStore.isDark
}

function onLogout() {
  quizStore.logout()
}

async function onRedeemKey({ code, onSuccess, onError }: { code: string; onSuccess: (msg?: string) => void; onError: (msg?: string) => void }) {
  try {
    const data = await quizStore.redeemUnlockCode(code)
    onSuccess(data.message)
  } catch (err: any) {
    onError(err?.response?.data?.message || err?.message || 'Mã mở khóa không hợp lệ.')
  }
}

function onGoogleLoginSuccess(credential: string) {
  showLoginModal.value = false
  // Decode JWT payload for basic user info
  try {
    const base64Url = credential.split('.')[1]
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/')
    const jsonPayload = decodeURIComponent(
      window
        .atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    )
    const payload = JSON.parse(jsonPayload)

    const adminEmail = import.meta.env.VITE_GOOGLE_ALLOWED_EMAIL || 'hnt.vn.vn@gmail.com'
    const isAdmin = payload.email?.toLowerCase() === adminEmail.toLowerCase()

    quizStore.setUser(
      {
        email: payload.email,
        name: payload.name || payload.email.split('@')[0],
        picture: payload.picture,
        unlockedSubjects: isAdmin ? ['mln122', 'prm393', 'jfe301', 'jit401'] : [],
        isAdmin
      },
      credential
    )

    quizStore.fetchCatalog()
    quizStore.fetchQuestions(quizStore.activeSubjectId)
  } catch (e) {
    console.error('Failed to parse Google credential:', e)
  }
}

function onGoogleLoginError(msg: string) {
  console.error('Google login error:', msg)
}

// Keyboard shortcuts listener
function handleGlobalKeydown(e: KeyboardEvent) {
  // Ignore when typing inside input / textarea
  const target = e.target as HTMLElement
  if (target && (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.isContentEditable)) {
    return
  }

  // Prev / Next
  if (e.key === 'ArrowLeft' || e.key === 'a' || e.key === 'A') {
    e.preventDefault()
    quizStore.prev()
  } else if (e.key === 'ArrowRight' || e.key === 'd' || e.key === 'D') {
    e.preventDefault()
    quizStore.next()
  }

  // Star
  if (e.key === 's' || e.key === 'S') {
    e.preventDefault()
    quizStore.toggleStar()
  }

  // Option selection
  const keyMap: Record<string, string> = {
    '1': 'A',
    '2': 'B',
    '3': 'C',
    '4': 'D',
    'a': 'A',
    'b': 'B',
    'c': 'C',
    'd': 'D'
  }

  // Check if option select key is pressed without Ctrl/Meta
  if (!e.ctrlKey && !e.metaKey && !e.altKey && e.key.length === 1) {
    const opt = keyMap[e.key.toLowerCase()]
    if (opt && ['A', 'B', 'C', 'D'].includes(opt)) {
      // only trigger if current question has this option
      if (quizStore.currentQuestion?.options && quizStore.currentQuestion.options[opt]) {
        quizStore.selectOption(opt)
      }
    }
  }
}

// Flashcards Word Capture
async function loadStudyDecks() {
  try {
    const res = await StudyAPI.getDecks()
    desks.value = res.data
    if (desks.value.length > 0) {
      selectedDeckId.value = desks.value[0].id
    }
  } catch {}
}

function openAddWordModal() {
  newWordFront.value = ''
  newWordBack.value = ''
  showAddWordModal.value = true
}

async function saveWordToDesk() {
  if (!newWordFront.value.trim() || !selectedDeckId.value) return
  try {
    await StudyAPI.createCard(selectedDeckId.value, {
      front: newWordFront.value.trim(),
      back: newWordBack.value.trim(),
      tags: [activeSubjectCode.value, 'quiz']
    })
    showAddWordModal.value = false
    alert('Đã lưu từ vựng vào Flashcard!')
  } catch (err: any) {
    alert(err?.response?.data?.error || 'Lỗi khi lưu từ.')
  }
}
</script>

<style scoped>
.quiz-app-container {
  max-width: 1140px;
  margin: 0 auto;
  padding: 14px 16px 32px;
  min-height: 100vh;
}

.workspace-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 210px;
  gap: 14px;
  align-items: start;
}

@media (max-width: 820px) {
  .workspace-grid {
    grid-template-columns: 1fr;
  }
}

.main-col {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.state-card {
  padding: 40px 24px;
  text-align: center;
  border-radius: 14px;
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
}

.lock-card {
  border: 1.5px dashed rgba(220, 38, 38, 0.4);
}

.lock-illustration {
  font-size: 48px;
  margin-bottom: 12px;
}

.lock-title {
  font-size: 1.25rem;
  font-weight: 800;
  color: var(--wn-ink, #0f172a);
  margin-bottom: 8px;
}

.lock-desc {
  font-size: 0.9rem;
  color: var(--wn-ink-muted, #64748b);
  max-width: 440px;
  margin: 0 auto 20px;
  line-height: 1.5;
}

.lock-actions {
  display: flex;
  justify-content: center;
  gap: 10px;
}

.empty-illustration {
  font-size: 40px;
  margin-bottom: 10px;
}

.empty-title {
  font-size: 1.15rem;
  font-weight: 700;
  margin-bottom: 8px;
}

.empty-desc {
  font-size: 0.88rem;
  color: var(--wn-ink-muted, #64748b);
  margin-bottom: 16px;
}

/* Modal Custom styling */
.modal-backdrop-custom {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.7);
  backdrop-filter: blur(6px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1200;
  padding: 16px;
}

.custom-dialog {
  width: 100%;
  max-width: 460px;
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 14px;
  overflow: hidden;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.25);
}

.custom-dialog-header {
  padding: 16px 20px;
  border-bottom: 1px solid var(--wn-card-border, #e2e8f0);
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.custom-dialog-header h5 {
  margin: 0;
  font-size: 1rem;
  font-weight: 700;
}

.btn-close-custom {
  background: transparent;
  border: none;
  font-size: 1.4rem;
  color: var(--wn-ink-muted, #64748b);
  cursor: pointer;
}

.custom-dialog-body {
  padding: 20px;
}

.custom-dialog-footer {
  padding: 12px 20px;
  border-top: 1px solid var(--wn-card-border, #e2e8f0);
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

.custom-select,
.custom-input {
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: var(--wn-card-bg, #ffffff);
  color: var(--wn-ink, #0f172a);
  border-radius: 8px;
}
</style>
