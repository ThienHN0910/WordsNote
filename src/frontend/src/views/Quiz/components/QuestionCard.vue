<template>
  <article class="exam-card glass-panel fade-in">
    <!-- Card Header Metadata -->
    <div class="card-header">
      <span class="card-code">{{ question.subjectId ? question.subjectId.toUpperCase() : 'MLN122' }}</span>
      <span class="card-sep">|</span>
      <span class="card-qnum">Multiple Choice Question {{ question.questionNumber || question.id }}</span>
      <span class="card-pos">#{{ posIndex + 1 }} · {{ posIndex + 1 }}/{{ queueLength }}</span>
    </div>

    <!-- Instructions / Choose limit -->
    <p v-if="chooseLimit > 1" class="card-choose">(Choose up to {{ chooseLimit }} answers)</p>
    <p v-else class="card-choose">(Choose 1 answer)</p>

    <!-- Question Text -->
    <h1 class="card-question">{{ question.question }}</h1>

    <!-- Option Buttons -->
    <div class="card-options">
      <button
        v-for="opt in normalizedOptions"
        :key="opt.key"
        type="button"
        class="opt"
        :class="{
          'selected': selectedKeys.includes(opt.key),
          'correct': isChecked && isCorrect(opt.key),
          'wrong': isChecked && selectedKeys.includes(opt.key) && !isCorrect(opt.key)
        }"
        @click="$emit('select-option', opt.key)"
      >
        <span class="opt-letter">{{ opt.key }}.</span>
        <span class="opt-text">{{ opt.text }}</span>
      </button>
    </div>

    <!-- Navigation Buttons Bar (Immediately below options) -->
    <div class="card-nav">
      <button
        type="button"
        class="btn-nav"
        :disabled="posIndex === 0"
        title="Phím tắt: ← hoặc A"
        @click="$emit('prev')"
      >
        <i class="fa-solid fa-arrow-left me-1"></i> Trước
      </button>

      <button
        type="button"
        class="btn-nav star-btn"
        :class="{ 'starred': isStarred }"
        title="Đánh dấu câu hỏi này (Phím S)"
        @click="$emit('toggle-star')"
      >
        <i :class="isStarred ? 'fa-solid fa-star' : 'fa-regular fa-star'"></i>
      </button>

      <button
        v-if="chooseLimit > 1"
        type="button"
        class="btn-nav btn-check"
        :disabled="isChecked || selectedKeys.length === 0"
        @click="$emit('check')"
      >
        <i class="fa-solid fa-check me-1"></i> Kiểm tra
      </button>

      <button
        type="button"
        class="btn-nav"
        title="Hiện hoặc ẩn đáp án"
        @click="$emit('toggle-reveal')"
      >
        <i class="fa-regular fa-eye me-1"></i>
        {{ isRevealed ? 'Ẩn ĐA' : 'Hiện ĐA' }}
      </button>

      <!-- Quick Word Save to Flashcards -->
      <button
        type="button"
        class="btn-nav save-word-btn"
        title="Lưu từ vựng vào bộ thẻ học SRS"
        @click="$emit('save-word')"
      >
        <i class="fa-solid fa-plus me-1"></i> Lưu từ
      </button>

      <button
        type="button"
        class="btn-nav btn-next"
        :disabled="posIndex >= queueLength - 1"
        title="Phím tắt: → hoặc D"
        @click="$emit('next')"
      >
        Tiếp <i class="fa-solid fa-arrow-right ms-1"></i>
      </button>
    </div>

    <!-- Feedback Box (After submission or click) -->
    <div v-if="isChecked" class="card-feedback" :class="isAnswerCorrect ? 'ok' : 'bad'">
      <span v-if="isAnswerCorrect"><i class="fa-solid fa-circle-check me-1"></i> Đúng!</span>
      <span v-else><i class="fa-solid fa-circle-xmark me-1"></i> Sai. Đáp án đúng: <strong>{{ correctLetters }}</strong></span>
    </div>

    <!-- Detailed Explanation Box -->
    <div
      v-if="(isChecked || isRevealed) && explanationText"
      class="card-explain"
      :class="{ 'explain-wrong': isChecked && !isAnswerCorrect }"
    >
      <div class="explain-header">
        <i class="fa-solid fa-lightbulb me-1"></i>
        <strong>GIẢI THÍCH CHI TIẾT {{ isChecked && !isAnswerCorrect ? '(BẠN CHỌN SAI)' : '' }}</strong>
      </div>
      <div class="explain-body">{{ explanationText }}</div>
    </div>

    <!-- Note Box -->
    <div v-if="(isChecked || isRevealed) && question.note" class="card-note">
      <strong><i class="fa-solid fa-note-sticky me-1"></i> GHI CHÚ BỔ SUNG:</strong> {{ question.note }}
    </div>

    <!-- Alt Variant Box -->
    <div v-if="(isChecked || isRevealed) && question.alt" class="card-alt">
      <strong><i class="fa-solid fa-code-compare me-1"></i> CÂU HỎI BIẾN THỂ TƯƠNG TỰ:</strong> {{ question.alt }}
    </div>
  </article>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { QuizQuestion } from '@/types/WordsNote'

const props = withDefaults(
  defineProps<{
    question: QuizQuestion
    posIndex?: number
    queueLength?: number
    selectedKeys?: string[]
    isChecked?: boolean
    isRevealed?: boolean
    isStarred?: boolean
  }>(),
  {
    posIndex: 0,
    queueLength: 1,
    selectedKeys: () => [],
    isChecked: false,
    isRevealed: false,
    isStarred: false
  }
)

defineEmits<{
  (e: 'select-option', key: string): void
  (e: 'toggle-star'): void
  (e: 'check'): void
  (e: 'toggle-reveal'): void
  (e: 'prev'): void
  (e: 'next'): void
  (e: 'save-word'): void
}>()

const chooseLimit = computed(() => props.question.choose || 1)

interface NormalizedOption {
  key: string
  text: string
}

const normalizedOptions = computed<NormalizedOption[]>(() => {
  const opts = props.question.options
  if (!opts) return []
  if (Array.isArray(opts)) {
    return opts.map((o, idx) => {
      if (typeof o === 'string') {
        const letter = String.fromCharCode(65 + idx)
        return { key: letter, text: o }
      }
      return o
    })
  }
  if (typeof opts === 'object') {
    return Object.entries(opts).map(([key, text]) => ({
      key,
      text: String(text)
    }))
  }
  return []
})

const explanationText = computed(() => {
  return props.question.explanation || (props.question as any).explain || ''
})

function isCorrect(key: string): boolean {
  return (props.question.answers || []).map((a) => a.trim().toUpperCase()).includes(key.trim().toUpperCase())
}

const correctLetters = computed(() => {
  return (props.question.answers || []).join(', ')
})

const isAnswerCorrect = computed(() => {
  if (!props.isChecked) return false
  const correctSet = new Set((props.question.answers || []).map((a) => a.trim().toUpperCase()))
  const selectedSet = new Set((props.selectedKeys || []).map((k) => k.trim().toUpperCase()))
  if (correctSet.size !== selectedSet.size) return false
  for (const k of correctSet) {
    if (!selectedSet.has(k)) return false
  }
  return true
})
</script>

<style scoped>
.exam-card {
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 12px;
  padding: 20px 22px 24px;
  min-height: 280px;
}

.card-header {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  gap: 6px;
  font-size: 0.76rem;
  color: var(--wn-ink-muted, #64748b);
  margin-bottom: 6px;
}

.card-code {
  font-weight: 700;
  color: #2563eb;
}

.card-sep {
  opacity: 0.4;
}

.card-pos {
  margin-left: auto;
  font-size: 0.75rem;
  font-weight: 600;
}

.card-choose {
  margin: 0 0 14px;
  font-size: 0.84rem;
  color: var(--wn-ink-muted, #64748b);
  font-style: italic;
}

.card-question {
  margin: 0 0 18px;
  font-size: 1.08rem;
  font-weight: 700;
  line-height: 1.55;
  color: var(--wn-ink, #0f172a);
}

.card-options {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.opt {
  display: flex;
  gap: 12px;
  width: 100%;
  text-align: left;
  border: 1.5px solid var(--wn-card-border, #e2e8f0);
  background: var(--wn-card-bg, #ffffff);
  border-radius: 10px;
  padding: 12px 16px;
  cursor: pointer;
  color: var(--wn-ink, #0f172a);
  transition: all 0.15s ease;
  font-size: 0.92rem;
  line-height: 1.45;
}

.opt:hover:not(:disabled) {
  border-color: #94a3b8;
  background: rgba(148, 163, 184, 0.08);
}

.opt.selected {
  background: rgba(37, 99, 235, 0.08);
  border-color: #2563eb;
}

.opt.correct {
  background: rgba(16, 185, 129, 0.15) !important;
  border-color: #10b981 !important;
  color: #065f46 !important;
}

:global(html[data-bs-theme="dark"]) .opt.correct,
:global(html.dark) .opt.correct {
  color: #a7f3d0 !important;
}

.opt.wrong {
  background: rgba(239, 68, 68, 0.15) !important;
  border-color: #ef4444 !important;
  color: #991b1b !important;
}

:global(html[data-bs-theme="dark"]) .opt.wrong,
:global(html.dark) .opt.wrong {
  color: #fca5a5 !important;
}

.opt-letter {
  font-weight: 800;
  min-width: 22px;
  color: #64748b;
}

.opt-text {
  flex: 1;
}

.card-nav {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  margin-top: 16px;
  margin-bottom: 14px;
}

.btn-nav {
  padding: 7px 14px;
  font-size: 0.82rem;
  font-weight: 600;
  border-radius: 8px;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: var(--wn-card-bg, #ffffff);
  color: var(--wn-ink, #0f172a);
  cursor: pointer;
  transition: all 0.15s ease;
}

.btn-nav:hover:not(:disabled) {
  border-color: #94a3b8;
  background: rgba(148, 163, 184, 0.1);
}

.btn-nav:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.btn-check {
  background: rgba(16, 185, 129, 0.12);
  border-color: #10b981;
  color: #059669;
}

.btn-next {
  background: #2563eb;
  border-color: #2563eb;
  color: #fff;
  margin-left: auto;
}

.btn-next:hover:not(:disabled) {
  background: #1d4ed8;
  border-color: #1d4ed8;
}

.star-btn.starred {
  background: rgba(245, 158, 11, 0.15);
  border-color: #f59e0b;
  color: #f59e0b;
}

.save-word-btn {
  color: #8b5cf6;
  border-color: rgba(139, 92, 246, 0.4);
}

.save-word-btn:hover {
  background: rgba(139, 92, 246, 0.1);
}

.card-feedback {
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 0.88rem;
  font-weight: 600;
  margin-bottom: 12px;
}

.card-feedback.ok {
  background: rgba(16, 185, 129, 0.12);
  border: 1px solid rgba(16, 185, 129, 0.4);
  color: #059669;
}

.card-feedback.bad {
  background: rgba(239, 68, 68, 0.12);
  border: 1px solid rgba(239, 68, 68, 0.4);
  color: #dc2626;
}

.card-explain {
  padding: 14px;
  border-radius: 8px;
  background: rgba(37, 99, 235, 0.06);
  border: 1px solid rgba(37, 99, 235, 0.2);
  margin-bottom: 12px;
  font-size: 0.86rem;
  line-height: 1.5;
}

.explain-header {
  font-size: 0.78rem;
  margin-bottom: 6px;
  color: #2563eb;
}

.card-explain.explain-wrong {
  background: rgba(249, 115, 22, 0.08);
  border-color: rgba(249, 115, 22, 0.35);
}

.card-explain.explain-wrong .explain-header {
  color: #ea580c;
}

.card-note {
  padding: 10px 14px;
  border-radius: 8px;
  background: rgba(245, 158, 11, 0.08);
  border: 1px solid rgba(245, 158, 11, 0.3);
  font-size: 0.84rem;
  color: #b45309;
  margin-bottom: 10px;
}

.card-alt {
  padding: 10px 14px;
  border-radius: 8px;
  background: rgba(148, 163, 184, 0.08);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  font-size: 0.84rem;
  color: var(--wn-ink-muted, #64748b);
}
</style>
