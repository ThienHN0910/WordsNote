<template>
  <div v-if="show" class="drawer-backdrop" @click.self="$emit('close')">
    <div class="drawer-panel glass-panel fade-in">
      <div class="drawer-header">
        <div class="search-input-wrapper">
          <i class="fa-solid fa-magnifying-glass search-icon"></i>
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm kiếm theo từ khóa hoặc số câu..."
            class="search-input"
            ref="searchInputRef"
            @keydown.esc="$emit('close')"
          />
          <button v-if="searchQuery" class="clear-btn" @click="searchQuery = ''">&times;</button>
        </div>
        <button class="close-btn" @click="$emit('close')">&times;</button>
      </div>

      <div class="drawer-body">
        <div v-if="filteredQuestions.length === 0" class="empty-results">
          <span>🔍 Không tìm thấy câu hỏi nào phù hợp với "{{ searchQuery }}".</span>
        </div>

        <div v-else class="results-list">
          <div
            v-for="q in filteredQuestions"
            :key="q.id"
            class="result-card"
            @click="selectResult(q.questionNumber || q.id)"
          >
            <div class="res-header">
              <span class="res-id">Câu {{ q.questionNumber || q.id }}</span>
              <span v-if="q.exam" class="res-exam">{{ q.exam }}</span>
            </div>
            <p class="res-text">{{ truncate(q.question, 120) }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick } from 'vue'
import type { QuizQuestion } from '@/types/WordsNote'

const props = withDefaults(
  defineProps<{
    show?: boolean
    questions?: QuizQuestion[]
  }>(),
  {
    show: false,
    questions: () => []
  }
)

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'select-question', id: string | number): void
}>()

const searchQuery = ref('')
const searchInputRef = ref<HTMLInputElement | null>(null)

watch(
  () => props.show,
  (newVal) => {
    if (newVal) {
      searchQuery.value = ''
      nextTick(() => {
        searchInputRef.value?.focus()
      })
    }
  }
)

const filteredQuestions = computed(() => {
  if (!searchQuery.value.trim()) return props.questions.slice(0, 50)
  const q = searchQuery.value.trim().toLowerCase()

  // If query is a number
  if (/^\d+$/.test(q)) {
    const num = parseInt(q, 10)
    const exact = props.questions.filter((item) => item.questionNumber === num || item.id === String(num))
    if (exact.length) return exact
  }

  return props.questions
    .filter((item) => {
      return (
        String(item.questionNumber || item.id).includes(q) ||
        (item.question && item.question.toLowerCase().includes(q)) ||
        (item.explanation && item.explanation.toLowerCase().includes(q))
      )
    })
    .slice(0, 50)
})

function selectResult(id: string | number) {
  emit('select-question', id)
  emit('close')
}

function truncate(str: string, len: number) {
  if (!str) return ''
  return str.length > len ? str.substring(0, len) + '...' : str
}
</script>

<style scoped>
.drawer-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.7);
  backdrop-filter: blur(6px);
  z-index: 1050;
  display: flex;
  justify-content: flex-end;
}

.drawer-panel {
  width: 100%;
  max-width: 480px;
  height: 100%;
  display: flex;
  flex-direction: column;
  background: var(--wn-card-bg, #ffffff);
  border-left: 1px solid var(--wn-card-border, #e2e8f0);
  box-shadow: -4px 0 24px rgba(0, 0, 0, 0.15);
}

.drawer-header {
  padding: 16px;
  border-bottom: 1px solid var(--wn-card-border, #e2e8f0);
  display: flex;
  align-items: center;
  gap: 12px;
}

.search-input-wrapper {
  flex: 1;
  display: flex;
  align-items: center;
  background: rgba(148, 163, 184, 0.1);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 10px;
  padding: 0 12px;
}

.search-icon {
  color: var(--wn-ink-muted, #64748b);
  margin-right: 8px;
  font-size: 0.9rem;
}

.search-input {
  flex: 1;
  height: 40px;
  background: transparent;
  border: none;
  color: var(--wn-ink, #0f172a);
  font-size: 0.9rem;
  outline: none;
}

.clear-btn,
.close-btn {
  background: transparent;
  border: none;
  color: var(--wn-ink-muted, #64748b);
  font-size: 1.25rem;
  cursor: pointer;
  padding: 0 4px;
}

.close-btn:hover,
.clear-btn:hover {
  color: var(--wn-ink, #0f172a);
}

.drawer-body {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
}

.empty-results {
  text-align: center;
  color: var(--wn-ink-muted, #64748b);
  padding: 40px 0;
  font-size: 0.88rem;
}

.results-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.result-card {
  background: rgba(148, 163, 184, 0.06);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  padding: 12px 14px;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.15s ease;
}

.result-card:hover {
  background: rgba(37, 99, 235, 0.08);
  border-color: #2563eb;
  transform: translateX(-2px);
}

.res-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
}

.res-id {
  font-size: 0.82rem;
  font-weight: 700;
  color: #2563eb;
}

.res-exam {
  font-size: 0.72rem;
  padding: 2px 8px;
  border-radius: 999px;
  background: rgba(148, 163, 184, 0.2);
  color: var(--wn-ink-muted, #64748b);
}

.res-text {
  margin: 0;
  font-size: 0.84rem;
  line-height: 1.45;
  color: var(--wn-ink, #0f172a);
}
</style>
