<template>
  <aside class="side-map glass-panel" aria-label="Bản đồ câu hỏi">
    <div class="map-head">
      <span><i class="fa-solid fa-map me-1"></i> Bản đồ</span>
      <span class="map-count">{{ questions.length }} câu</span>
    </div>

    <div class="map-legend">
      <span><i class="dot current"></i> đang làm</span>
      <span><i class="dot ok"></i> đúng</span>
      <span><i class="dot bad"></i> sai</span>
      <span><i class="dot unseen"></i> chưa</span>
    </div>

    <div class="q-map" role="list">
      <button
        v-for="(q, index) in questions"
        :key="q.id || index"
        type="button"
        class="q-cell"
        :class="{
          'current': currentQuestionId === q.id,
          'ok': getResult(q.id, q.questionNumber) === 'ok',
          'bad': getResult(q.id, q.questionNumber) === 'bad',
          'starred': isStarred(q.id, q.questionNumber)
        }"
        :title="`Câu ${q.questionNumber || (index + 1)}${getResult(q.id, q.questionNumber) === 'ok' ? ' (Đúng)' : getResult(q.id, q.questionNumber) === 'bad' ? ' (Sai)' : ''}`"
        @click="$emit('select-question', q.questionNumber || q.id)"
      >
        <span class="cell-num">{{ q.questionNumber || (index + 1) }}</span>
        <span v-if="isStarred(q.id, q.questionNumber)" class="cell-star">★</span>
      </button>

      <div v-if="questions.length === 0" class="q-map-empty">
        Đang tải bản đồ câu hỏi...
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import type { QuizQuestion } from '@/types/WordsNote'
import type { QuestionProgress } from '@/stores/WordsNote/QuizStore'

const props = defineProps<{
  questions: QuizQuestion[]
  currentQuestionId?: string | number | null
  progress: Record<string | number, QuestionProgress>
}>()

defineEmits<{
  (e: 'select-question', qIdOrNum: string | number): void
}>()

function getResult(id: string | number, num?: number): 'ok' | 'bad' | undefined {
  const p = props.progress[id] || (num ? props.progress[num] : undefined)
  return p?.result
}

function isStarred(id: string | number, num?: number): boolean {
  const p = props.progress[id] || (num ? props.progress[num] : undefined)
  return !!p?.star
}
</script>

<style scoped>
.side-map {
  position: sticky;
  top: 14px;
  background: var(--wn-card-bg, #ffffff);
  border: 1px solid var(--wn-card-border, #e2e8f0);
  border-radius: 12px;
  padding: 12px;
  height: calc(100vh - 90px);
  height: calc(100dvh - 90px);
  min-height: 280px;
  max-height: calc(100vh - 90px);
  max-height: calc(100dvh - 90px);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.map-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.78rem;
  font-weight: 700;
  color: var(--wn-ink-muted, #64748b);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  margin-bottom: 8px;
  flex: 0 0 auto;
}

.map-count {
  font-size: 0.75rem;
  background: rgba(148, 163, 184, 0.15);
  border-radius: 999px;
  padding: 2px 8px;
  color: var(--wn-ink, #0f172a);
}

.map-legend {
  display: flex;
  flex-wrap: wrap;
  gap: 6px 8px;
  margin-bottom: 10px;
  font-size: 0.7rem;
  color: var(--wn-ink-muted, #64748b);
  flex: 0 0 auto;
}

.map-legend .dot {
  display: inline-block;
  width: 8px;
  height: 8px;
  border-radius: 2px;
  margin-right: 3px;
  border: 1px solid rgba(148, 163, 184, 0.4);
  vertical-align: middle;
}

.map-legend .dot.current {
  background: #2563eb;
  border-color: #2563eb;
}

.map-legend .dot.ok {
  background: #10b981;
  border-color: #059669;
}

.map-legend .dot.bad {
  background: #ef4444;
  border-color: #dc2626;
}

.map-legend .dot.unseen {
  background: transparent;
}

.q-map {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(32px, 1fr));
  gap: 5px;
  overflow-x: hidden;
  overflow-y: auto;
  -webkit-overflow-scrolling: touch;
  flex: 1 1 auto;
  min-height: 180px;
  align-content: start;
  padding-right: 2px;
}

.q-cell {
  position: relative;
  height: 32px;
  border: 1px solid var(--wn-card-border, #e2e8f0);
  background: var(--wn-card-bg, #ffffff);
  color: var(--wn-ink, #0f172a);
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  padding: 0;
  transition: all 0.15s ease;
}

.q-cell:hover {
  border-color: #94a3b8;
  transform: translateY(-1px);
}

.q-cell.ok {
  background: rgba(16, 185, 129, 0.15);
  color: #059669;
  border-color: rgba(16, 185, 129, 0.4);
}

.q-cell.bad {
  background: rgba(239, 68, 68, 0.15);
  color: #dc2626;
  border-color: rgba(239, 68, 68, 0.4);
}

.q-cell.current {
  border: 2px solid #2563eb;
  box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.25);
  font-weight: 800;
}

.cell-star {
  position: absolute;
  top: 0px;
  right: 1px;
  font-size: 8px;
  color: #f59e0b;
}

.q-map-empty {
  grid-column: 1 / -1;
  font-size: 0.8rem;
  color: var(--wn-ink-muted, #64748b);
  text-align: center;
  padding: 20px 0;
}
</style>
