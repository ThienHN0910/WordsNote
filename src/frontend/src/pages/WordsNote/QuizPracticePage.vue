<template>
  <div class="practice-container">
    <div v-if="isLoading" class="loading-state">
      <div class="spinner-border text-primary" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
      <p class="mt-3 text-muted">Loading questions for {{ route.params.id }}...</p>
    </div>

    <div v-else-if="error" class="alert alert-danger" role="alert">
      {{ error }}
    </div>

    <div v-else-if="questions.length === 0" class="empty-state">
      <p class="text-muted">No questions found for this subject.</p>
      <RouterLink to="/quiz" class="btn btn-outline-secondary mt-2">Back to Quiz Banks</RouterLink>
    </div>

    <div v-else class="practice-layout">
      <!-- Top Bar: Progress & Mode Controls -->
      <div class="practice-header">
        <div class="header-left">
          <RouterLink to="/quiz" class="back-link">
            <i class="fa-solid fa-arrow-left me-1"></i> Back
          </RouterLink>
          <span class="subject-pill" :style="{ '--color': currentQuizSet?.color }">
            {{ currentQuizSet?.code }}
          </span>
          <span class="mode-pill">{{ isExamMode ? 'Mock Exam' : 'Practice Mode' }}</span>
        </div>

        <div class="header-right">
          <div v-if="isExamMode" class="timer-badge" :class="{ 'timer-warning': timeLeft < 300 }">
            <i class="fa-regular fa-clock me-1"></i>
            <span>{{ formattedTime }}</span>
          </div>

          <button
            type="button"
            class="btn btn-sm btn-primary"
            :disabled="answeredCount === 0"
            @click="handleSubmit"
          >
            {{ isSubmitted ? 'View Results' : 'Finish & Submit' }}
          </button>
        </div>
      </div>

      <!-- Progress Bar -->
      <div class="progress-wrap">
        <div class="progress-bar-fill" :style="{ width: `${progressPercentage}%` }"></div>
      </div>

      <!-- Main Question Area -->
      <div class="question-main-card">
        <div class="question-meta-row">
          <span class="q-number">
            Question {{ currentQuestionIndex + 1 }} of {{ totalQuestions }}
          </span>

          <div class="q-actions">
            <button
              type="button"
              class="icon-btn"
              :class="{ active: isBookmarked }"
              :title="isBookmarked ? 'Remove Bookmark' : 'Bookmark Question'"
              @click="toggleCurrentBookmark"
            >
              <i :class="isBookmarked ? 'fa-solid fa-bookmark' : 'fa-regular fa-bookmark'"></i>
            </button>

            <button
              type="button"
              class="quick-save-btn"
              title="Save a new vocabulary word from this question"
              @click="openAddWordModal"
            >
              <i class="fa-solid fa-plus me-1"></i> Save Word
            </button>
          </div>
        </div>

        <h2 class="question-text">{{ currentQuestion?.question }}</h2>

        <!-- Options List -->
        <div class="options-list">
          <div
            v-for="(optionText, optionKey) in currentQuestion?.options"
            :key="optionKey"
            class="option-item"
            :class="getOptionClass(String(optionKey))"
            @click="handleSelectOption(String(optionKey))"
          >
            <span class="option-key">{{ optionKey }}</span>
            <span class="option-text">{{ optionText }}</span>
            <i v-if="getOptionIcon(String(optionKey))" :class="getOptionIcon(String(optionKey))" class="option-icon"></i>
          </div>
        </div>

        <!-- Explanation Block (Shown in practice mode or after submit) -->
        <div
          v-if="(isPracticeMode && isCurrentAnswered) || isSubmitted"
          class="explanation-card"
          :class="{ 'exp-correct': isCurrentAnswerCorrect, 'exp-incorrect': !isCurrentAnswerCorrect }"
        >
          <div class="explanation-title">
            <i :class="isCurrentAnswerCorrect ? 'fa-solid fa-circle-check text-success' : 'fa-solid fa-circle-xmark text-danger'" class="me-2"></i>
            {{ isCurrentAnswerCorrect ? 'Correct!' : 'Incorrect' }}
            <span v-if="!isCurrentAnswerCorrect" class="text-muted ms-2">
              (Correct: {{ currentQuestion?.answers?.join(', ') }})
            </span>
          </div>
          <p class="explanation-body">
            {{ currentQuestion?.explanation || 'No detailed explanation provided for this question.' }}
          </p>
        </div>

        <!-- Navigation Buttons -->
        <div class="nav-controls">
          <button
            type="button"
            class="btn btn-outline-secondary"
            :disabled="currentQuestionIndex === 0"
            @click="quizStore.prevQuestion"
          >
            <i class="fa-solid fa-chevron-left me-1"></i> Previous
          </button>

          <span class="nav-counter">{{ currentQuestionIndex + 1 }} / {{ totalQuestions }}</span>

          <button
            type="button"
            class="btn btn-outline-secondary"
            :disabled="currentQuestionIndex === totalQuestions - 1"
            @click="quizStore.nextQuestion"
          >
            Next <i class="fa-solid fa-chevron-right ms-1"></i>
          </button>
        </div>
      </div>

      <!-- Quick Navigator Grid -->
      <div class="navigator-card">
        <h3 class="nav-title">Questions Overview</h3>
        <div class="nav-grid">
          <button
            v-for="(_, index) in questions"
            :key="index"
            type="button"
            class="nav-grid-btn"
            :class="getGridBtnClass(index)"
            @click="quizStore.goToQuestion(index)"
          >
            {{ index + 1 }}
          </button>
        </div>
      </div>
    </div>

    <!-- Results Modal -->
    <div v-if="showResultModal" class="modal-overlay" @click.self="showResultModal = false">
      <div class="result-modal">
        <div class="result-header">
          <h2>Exam Summary</h2>
          <button type="button" class="btn-close" @click="showResultModal = false"></button>
        </div>

        <div class="result-body">
          <div class="score-circle">
            <span class="score-num">{{ submitResult?.scorePercentage || 0 }}%</span>
            <span class="score-label">Score</span>
          </div>

          <div class="stats-row">
            <div class="stat-pill text-success">
              <strong>{{ submitResult?.correctCount || 0 }}</strong> Correct
            </div>
            <div class="stat-pill text-danger">
              <strong>{{ submitResult?.incorrectCount || 0 }}</strong> Incorrect
            </div>
            <div class="stat-pill text-muted">
              <strong>{{ totalQuestions }}</strong> Total
            </div>
          </div>
        </div>

        <div class="result-footer">
          <button type="button" class="btn btn-outline-secondary" @click="handleReviewAnswers">
            Review Questions
          </button>
          <button type="button" class="btn btn-primary" @click="handleRestart">
            Try Again
          </button>
        </div>
      </div>
    </div>

    <!-- Quick Word Save Modal -->
    <div v-if="showAddWordModal" class="modal-overlay" @click.self="showAddWordModal = false">
      <div class="result-modal">
        <div class="result-header">
          <h2>Save Word to Desk</h2>
          <button type="button" class="btn-close" @click="showAddWordModal = false"></button>
        </div>

        <div class="result-body">
          <div class="mb-3 text-start">
            <label class="form-label fw-bold">Select Desk</label>
            <select v-model="selectedDeskId" class="form-select">
              <option v-for="d in desks" :key="d.id" :value="d.id">{{ d.title }}</option>
            </select>
          </div>

          <div class="mb-3 text-start">
            <label class="form-label fw-bold">Front (Word / Term)</label>
            <input v-model="newWordFront" type="text" class="form-control" placeholder="e.g. Consortium" />
          </div>

          <div class="mb-3 text-start">
            <label class="form-label fw-bold">Back (Meaning)</label>
            <textarea v-model="newWordBack" class="form-control" rows="3" placeholder="Definition / Note"></textarea>
          </div>
        </div>

        <div class="result-footer">
          <button type="button" class="btn btn-outline-secondary" @click="showAddWordModal = false">Cancel</button>
          <button
            type="button"
            class="btn btn-primary"
            :disabled="!newWordFront.trim() || !selectedDeskId"
            @click="saveWordToDesk"
          >
            Save to Flashcards
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useQuizStore } from '@/stores/WordsNote/QuizStore'
import { StudyAPI } from '@/apis/WordsNote/StudyAPI'
import type { StudyDeck } from '@/types/WordsNote'

const route = useRoute()
const router = useRouter()
const quizStore = useQuizStore()

const {
  currentQuizSet,
  questions,
  currentQuestionIndex,
  currentQuestion,
  userAnswers,
  bookmarkedQuestionIds,
  isSubmitted,
  submitResult,
  isLoading,
  error,
  totalQuestions,
  answeredCount,
  progressPercentage,
} = storeToRefs(quizStore)

const isExamMode = computed(() => route.query.mode === 'exam')
const isPracticeMode = computed(() => !isExamMode.value)

// Timer
const timeLeft = ref(40 * 60) // 40 mins
let timerInterval: any = null

const formattedTime = computed(() => {
  const m = Math.floor(timeLeft.value / 60)
  const s = timeLeft.value % 60
  return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`
})

// Modals
const showResultModal = ref(false)
const showAddWordModal = ref(false)
const desks = ref<StudyDeck[]>([])
const selectedDeskId = ref<string>('')
const newWordFront = ref<string>('')
const newWordBack = ref<string>('')

const isBookmarked = computed(() => {
  if (!currentQuestion.value) return false
  return bookmarkedQuestionIds.value.includes(currentQuestion.value.id)
})

const isCurrentAnswered = computed(() => {
  if (!currentQuestion.value) return false
  const ans = userAnswers.value[currentQuestion.value.id]
  return ans && ans.length > 0
})

const isCurrentAnswerCorrect = computed(() => {
  if (!currentQuestion.value || !isCurrentAnswered.value) return false
  const userAns = userAnswers.value[currentQuestion.value.id] || []
  const correct = currentQuestion.value.answers || []
  return (
    userAns.length === correct.length &&
    userAns.every((a) => correct.map((c) => c.toUpperCase()).includes(a.toUpperCase()))
  )
})

onMounted(async () => {
  const subjectId = String(route.params.id)
  await quizStore.loadQuestions(subjectId, {
    exam: isExamMode.value,
    examCount: isExamMode.value ? 40 : 50,
  })

  if (isExamMode.value) {
    timeLeft.value = 40 * 60
    timerInterval = setInterval(() => {
      if (timeLeft.value > 0) {
        timeLeft.value--
      } else {
        handleSubmit()
      }
    }, 1000)
  }

  try {
    const res = await StudyAPI.getDecks()
    desks.value = res.data
    if (desks.value.length > 0) {
      selectedDeskId.value = desks.value[0].id
    }
  } catch {}
})

onUnmounted(() => {
  if (timerInterval) clearInterval(timerInterval)
})

function handleSelectOption(optionKey: string) {
  if (!currentQuestion.value) return
  quizStore.selectAnswer(currentQuestion.value.id, optionKey, currentQuestion.value.choose === 1)
}

function toggleCurrentBookmark() {
  if (!currentQuestion.value) return
  quizStore.toggleBookmark(currentQuestion.value.id)
}

async function handleSubmit() {
  if (isExamMode.value && timerInterval) {
    clearInterval(timerInterval)
  }
  await quizStore.submitCurrentQuiz()
  showResultModal.value = true
}

function handleReviewAnswers() {
  showResultModal.value = false
}

function handleRestart() {
  showResultModal.value = false
  quizStore.resetQuiz()
  if (isExamMode.value) {
    timeLeft.value = 40 * 60
  }
}

function getOptionClass(optionKey: string) {
  if (!currentQuestion.value) return ''
  const qId = currentQuestion.value.id
  const selected = (userAnswers.value[qId] || []).includes(optionKey)

  if (isSubmitted.value || (isPracticeMode.value && selected)) {
    const isCorrect = (currentQuestion.value.answers || []).map((a) => a.toUpperCase()).includes(optionKey.toUpperCase())
    if (isCorrect) return 'option-correct'
    if (selected && !isCorrect) return 'option-incorrect'
  }

  return selected ? 'option-selected' : ''
}

function getOptionIcon(optionKey: string) {
  if (!currentQuestion.value) return ''
  const qId = currentQuestion.value.id
  const selected = (userAnswers.value[qId] || []).includes(optionKey)

  if (isSubmitted.value || (isPracticeMode.value && selected)) {
    const isCorrect = (currentQuestion.value.answers || []).map((a) => a.toUpperCase()).includes(optionKey.toUpperCase())
    if (isCorrect) return 'fa-solid fa-check text-success'
    if (selected && !isCorrect) return 'fa-solid fa-xmark text-danger'
  }
  return ''
}

function getGridBtnClass(index: number) {
  const q = questions.value[index]
  if (!q) return ''

  const isCurrent = index === currentQuestionIndex.value
  const answered = userAnswers.value[q.id]?.length > 0

  let cls = isCurrent ? 'grid-current ' : ''
  if (isSubmitted.value) {
    const userAns = userAnswers.value[q.id] || []
    const isCorrect =
      userAns.length === q.answers.length &&
      userAns.every((a) => q.answers.map((c) => c.toUpperCase()).includes(a.toUpperCase()))
    return cls + (isCorrect ? 'grid-correct' : 'grid-incorrect')
  }

  return cls + (answered ? 'grid-answered' : '')
}

function openAddWordModal() {
  if (window.getSelection) {
    const selected = window.getSelection()?.toString().trim()
    if (selected && selected.length < 50) {
      newWordFront.value = selected
    }
  }
  showAddWordModal.value = true
}

async function saveWordToDesk() {
  if (!newWordFront.value.trim() || !selectedDeskId.value) return
  try {
    await StudyAPI.createCard({
      collectionId: selectedDeskId.value,
      front: newWordFront.value.trim(),
      back: newWordBack.value.trim(),
      hint: `From ${currentQuizSet.value?.code || 'Quiz'}`,
      tags: [currentQuizSet.value?.code || 'Quiz'],
    })
    showAddWordModal.value = false
    newWordFront.value = ''
    newWordBack.value = ''
  } catch (err: any) {
    alert(err?.response?.data?.error || 'Failed to save word.')
  }
}
</script>

<style scoped>
.practice-container {
  max-width: 1040px;
  margin: 0 auto;
  padding: 1.5rem 1rem 4rem;
}

.practice-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 1rem;
  margin-bottom: 1rem;
}

.header-left,
.header-right {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.back-link {
  color: var(--wn-ink-muted, #64748b);
  text-decoration: none;
  font-size: 0.9rem;
  font-weight: 600;
}

.back-link:hover {
  color: var(--wn-ink, #0f172a);
}

.subject-pill {
  font-size: 0.85rem;
  font-weight: 700;
  padding: 0.25rem 0.65rem;
  border-radius: 6px;
  background: color-mix(in srgb, var(--color, #2563eb) 15%, transparent);
  color: var(--color, #2563eb);
}

.mode-pill {
  font-size: 0.8rem;
  font-weight: 600;
  padding: 0.2rem 0.5rem;
  border-radius: 6px;
  background: var(--wn-badge-bg, #f1f5f9);
  color: var(--wn-ink-muted, #64748b);
}

.timer-badge {
  font-size: 0.9rem;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  background: var(--wn-badge-bg, #f1f5f9);
  color: var(--wn-ink, #0f172a);
  padding: 0.35rem 0.75rem;
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
}

.timer-warning {
  background: rgba(220, 38, 38, 0.1);
  color: #dc2626;
}

.progress-wrap {
  width: 100%;
  height: 6px;
  background: var(--wn-badge-bg, #e2e8f0);
  border-radius: 9999px;
  overflow: hidden;
  margin-bottom: 1.5rem;
}

.progress-bar-fill {
  height: 100%;
  background: var(--wn-primary, #2563eb);
  transition: width 0.25s ease;
}

.practice-layout {
  display: grid;
  grid-template-columns: 1fr 280px;
  gap: 1.5rem;
  align-items: start;
}

@media (max-width: 900px) {
  .practice-layout {
    grid-template-columns: 1fr;
  }
}

.question-main-card {
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 16px;
  padding: 2rem;
}

.question-meta-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.25rem;
}

.q-number {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--wn-ink-muted, #64748b);
}

.q-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.icon-btn {
  background: none;
  border: 1px solid var(--wn-card-border, #cbd5e1);
  border-radius: 8px;
  padding: 0.35rem 0.6rem;
  color: var(--wn-ink-muted, #64748b);
  cursor: pointer;
  transition: all 0.15s ease;
}

.icon-btn.active {
  color: #f59e0b;
  border-color: #f59e0b;
}

.quick-save-btn {
  background: none;
  border: 1px dashed var(--wn-card-border, #94a3b8);
  border-radius: 8px;
  padding: 0.35rem 0.65rem;
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--wn-primary, #2563eb);
  cursor: pointer;
  transition: all 0.15s ease;
}

.quick-save-btn:hover {
  background: rgba(37, 99, 235, 0.05);
}

.question-text {
  font-size: 1.25rem;
  font-weight: 700;
  line-height: 1.5;
  color: var(--wn-ink, #0f172a);
  margin-bottom: 1.5rem;
}

.options-list {
  display: flex;
  flex-direction: column;
  gap: 0.85rem;
  margin-bottom: 1.75rem;
}

.option-item {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  padding: 0.9rem 1.1rem;
  border: 1.5px solid var(--wn-card-border, #e2e8f0);
  border-radius: 12px;
  cursor: pointer;
  background: var(--wn-card-bg, #ffffff);
  transition: all 0.15s ease;
}

.option-item:hover {
  border-color: var(--wn-primary, #2563eb);
  background: rgba(37, 99, 235, 0.02);
}

.option-key {
  font-weight: 700;
  font-size: 0.95rem;
  min-width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 6px;
  background: var(--wn-badge-bg, #f1f5f9);
  color: var(--wn-ink, #1e293b);
}

.option-text {
  flex-grow: 1;
  font-size: 1rem;
  color: var(--wn-ink, #1e293b);
  line-height: 1.4;
}

.option-selected {
  border-color: var(--wn-primary, #2563eb);
  background: rgba(37, 99, 235, 0.06);
}

.option-selected .option-key {
  background: var(--wn-primary, #2563eb);
  color: #ffffff;
}

.option-correct {
  border-color: #10b981;
  background: rgba(16, 185, 129, 0.08);
}

.option-correct .option-key {
  background: #10b981;
  color: #ffffff;
}

.option-incorrect {
  border-color: #ef4444;
  background: rgba(239, 68, 68, 0.08);
}

.option-incorrect .option-key {
  background: #ef4444;
  color: #ffffff;
}

.explanation-card {
  border-radius: 12px;
  padding: 1.1rem 1.25rem;
  margin-bottom: 1.75rem;
}

.exp-correct {
  background: rgba(16, 185, 129, 0.08);
  border: 1px solid rgba(16, 185, 129, 0.2);
}

.exp-incorrect {
  background: rgba(239, 68, 68, 0.08);
  border: 1px solid rgba(239, 68, 68, 0.2);
}

.explanation-title {
  font-weight: 700;
  font-size: 0.95rem;
  margin-bottom: 0.4rem;
}

.explanation-body {
  font-size: 0.9rem;
  line-height: 1.5;
  color: var(--wn-ink, #334155);
  margin: 0;
}

.nav-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-top: 1px solid var(--wn-card-border, #e2e8f0);
  padding-top: 1.25rem;
}

.nav-counter {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--wn-ink-muted, #64748b);
}

.navigator-card {
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 16px;
  padding: 1.25rem;
}

.nav-title {
  font-size: 1rem;
  font-weight: 700;
  color: var(--wn-ink, #0f172a);
  margin-bottom: 1rem;
}

.nav-grid {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 0.4rem;
}

.nav-grid-btn {
  aspect-ratio: 1;
  border: 1px solid var(--wn-card-border, #cbd5e1);
  background: var(--wn-card-bg, #ffffff);
  color: var(--wn-ink, #334155);
  font-size: 0.8rem;
  font-weight: 600;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.1s ease;
}

.nav-grid-btn:hover {
  background: var(--wn-badge-bg, #f1f5f9);
}

.grid-current {
  outline: 2px solid var(--wn-primary, #2563eb);
  outline-offset: 1px;
}

.grid-answered {
  background: var(--wn-badge-bg, #e2e8f0);
  font-weight: 700;
}

.grid-correct {
  background: #10b981 !important;
  color: #ffffff !important;
  border-color: #10b981 !important;
}

.grid-incorrect {
  background: #ef4444 !important;
  color: #ffffff !important;
  border-color: #ef4444 !important;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 999;
  padding: 1rem;
}

.result-modal {
  background: var(--wn-card-bg, #ffffff);
  border-radius: 16px;
  width: 100%;
  max-width: 440px;
  padding: 1.75rem;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
  text-align: center;
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.result-header h2 {
  font-size: 1.35rem;
  font-weight: 700;
  margin: 0;
}

.score-circle {
  width: 110px;
  height: 110px;
  border-radius: 50%;
  background: rgba(37, 99, 235, 0.08);
  border: 4px solid var(--wn-primary, #2563eb);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1.5rem;
}

.score-num {
  font-size: 1.75rem;
  font-weight: 800;
  color: var(--wn-primary, #2563eb);
  line-height: 1;
}

.score-label {
  font-size: 0.75rem;
  color: var(--wn-ink-muted, #64748b);
  text-transform: uppercase;
  font-weight: 600;
}

.stats-row {
  display: flex;
  justify-content: center;
  gap: 1.25rem;
  margin-bottom: 1.75rem;
}

.stat-pill {
  font-size: 0.95rem;
}

.result-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
}
</style>
